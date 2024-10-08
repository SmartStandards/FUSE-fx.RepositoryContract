#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif
using System.Reflection;
using System.Linq;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Collections.Generic;
using System.Data.ModelDescription.Convenience;
using System.Data.Fuse.Convenience.Internal;
using System.Data.Fuse.Ef.InstanceManagement;
using System.Data.Fuse.SchemaResolving;

namespace System.Data.Fuse.Ef {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public class EfDictUniversalRepository : UniversalRepositoryBase, ISchemaProvider {

    protected readonly Func<PropertyInfo, Dictionary<string, object>, object, bool> _HandlePropertyModelToEntity = null;
    protected readonly Func<PropertyInfo, object, Dictionary<string, object>, bool> _HandlePropertyEntityToModel = null;

    #region " SchemaRoot/Metadata Caching "

    private static SchemaRoot _SchemaRoot = null;
    public SchemaRoot GetSchemaRoot() {
      //TODO: wollen wir so eine funktionalität überhaupt hier,
      //oder verzichten wir lifer auf den konstruktor ohne schema
      if (_SchemaRoot == null) {
        Type dbContextType = null;
        _ContextInstanceProvider.VisitCurrentDbContext(
            (dbContext) => dbContextType = dbContext.GetType()
         );
        _SchemaRoot = SchemaCache.GetSchemaRootForContext(dbContextType);
      }
      return _SchemaRoot;
    }

    #endregion

    private IDbContextInstanceProvider _ContextInstanceProvider;
    public IDbContextInstanceProvider ContextInstanceProvider {
      get {
        return _ContextInstanceProvider;
      }
    }

    public EfDictUniversalRepository(
      IDbContextInstanceProvider contextInstanceProvider, IEntityResolver entityResolver
    ) :
    base(entityResolver) {
      _ContextInstanceProvider = contextInstanceProvider;
    }

    public EfDictUniversalRepository(
      IDbContextInstanceProvider contextInstanceProvider, IEntityResolver entityResolver,
      Func<PropertyInfo, Dictionary<string, object>, object, bool> handlePropertyModelToEntity,
      Func<PropertyInfo, object, Dictionary<string, object>, bool> handlePropertyEntityToModel
    ) :
    base(entityResolver) {
      _ContextInstanceProvider = contextInstanceProvider;
      _HandlePropertyModelToEntity = handlePropertyModelToEntity;
      _HandlePropertyEntityToModel = handlePropertyEntityToModel;
    }

    // gets the key type of the entity. 
    // Uses the SchemaRoot.GetPrimaryKeyProperties method to get the key properties of the entity.
    // if there is only one key property, returns the type of that property.
    // if there are multiple key properties, returns the type of the ComposityKey-class with the matching
    // number of fields.
    private Type GetKeyType(Type entityType) {
      SchemaRoot schemaRoot = GetSchemaRoot();
      List<PropertyInfo> keyProperties = schemaRoot.GetPrimaryKeyProperties(entityType);

      // If there is only one key property, return its type
      if (keyProperties.Count == 1) {
        return keyProperties[0].PropertyType;
      }

      // If there are multiple key properties, return the type of the CompositeKey class
      // with the matching number of fields
      switch (keyProperties.Count) {
        case 2:
          return typeof(CompositeKey2<,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
        case 3:
          return typeof(CompositeKey3<,,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
        case 4:
          return typeof(CompositeKey4<,,,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
        case 5:
          return typeof(CompositeKey5<,,,,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
        default:
          throw new InvalidOperationException("Unsupported number of key properties");
      }
    }

    protected override RepositoryUntypingFacade CreateInnerRepo(Type entityType) {

      Type keyType = GetKeyType(entityType);
      Type modelType = typeof(Dictionary<string, object>);

      Type repoFacadeType = typeof(DynamicRepositoryFacade<,>);
      repoFacadeType = repoFacadeType.MakeGenericType(modelType, typeof(object));

      Type repoType = typeof(EfRepository<,>);
      repoType = repoType.MakeGenericType(entityType, keyType);
      object efRepo = Activator.CreateInstance(repoType, _ContextInstanceProvider, this.GetSchemaRoot());

      object universalModelVsEntityRepo;
      if (_HandlePropertyModelToEntity == null && _HandlePropertyEntityToModel == null) {
        MethodInfo factoryMethod = typeof(ConversionHelper).GetMethod(nameof(ConversionHelper.CreateDictVsEntityRepositry));
        factoryMethod = factoryMethod.MakeGenericMethod(entityType, keyType);
        universalModelVsEntityRepo = factoryMethod.Invoke(
          null, new object[] {
            GetSchemaRoot(),
            this,
            efRepo,
            NavigationRole.Lookup | NavigationRole.Dependent | NavigationRole.Principal,
            false
          }
        );
      } else {
        Type universalModelVsEntityRepoType = typeof(DictVsEntityRepository<,>);
        universalModelVsEntityRepoType = universalModelVsEntityRepoType.MakeGenericType(entityType, keyType);
        universalModelVsEntityRepo = Activator.CreateInstance(
          universalModelVsEntityRepoType, efRepo, _HandlePropertyModelToEntity, _HandlePropertyEntityToModel
        );
      }

      return (RepositoryUntypingFacade)Activator.CreateInstance(repoFacadeType, universalModelVsEntityRepo);
    }

    private string _GeneratedOriginIdentity = null;
    public override string GetOriginIdentity() {
      if (string.IsNullOrWhiteSpace(_GeneratedOriginIdentity)) {
        _GeneratedOriginIdentity = _ContextInstanceProvider.VisitCurrentDbContext(
          (dbContext) => dbContext.GetGeneratedOriginName()   
        );
      }
      return _GeneratedOriginIdentity;
    }

    public override RepositoryCapabilities GetCapabilities() {
      return new RepositoryCapabilities();
    }

  }

}
