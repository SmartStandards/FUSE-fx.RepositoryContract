#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
  public class EfUniversalRepository : UniversalRepositoryBase {

    #region " SchemaRoot/Metadata Caching "

    private static SchemaRoot _SchemaRoot = null;
    public SchemaRoot GetSchemaRoot() {
      //TODO: wollen wir so eine funktionalität überhaupt hier,
      //oder verzichten wir lifer auf den konstruktor ohne schema
      if (_SchemaRoot == null) {
        Type dbContextType = null;
        _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => dbContextType = dbContext.GetType());
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

    public EfUniversalRepository(IDbContextInstanceProvider contextInstanceProvider, IEntityResolver entityResolver) :
    base(entityResolver) {
      _ContextInstanceProvider = contextInstanceProvider;
    }

    [Obsolete("This overload is unsave, because it doesnt care about lifetime management of the dbcontext!")]
    public EfUniversalRepository(DbContext dbContext, IEntityResolver entityResolver) :
    base(entityResolver) {
      _ContextInstanceProvider = new LongLivingDbContextInstanceProvider(dbContext);
    }

    // gets the key type of the entity. 
    // Uses the SchemaRoot.GetPrimaryKeyProperties method to get the key properties of the entity.
    // if there is only one key property, returns the type of that property.
    // if there are multiple key properties, returns the type of the ComposityKey-class with the matching
    // number of fields.
    private static Type GetKeyType(Type entityType, SchemaRoot schemaRoot) {
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

      Type keyType = GetKeyType(entityType, this.GetSchemaRoot());

      Type repoFacadeType = typeof(DynamicRepositoryFacade<,>);
      repoFacadeType = repoFacadeType.MakeGenericType(entityType, keyType);

      Type repoType = typeof(EfRepository<,>);
      repoType = repoType.MakeGenericType(entityType, keyType);

      object repo = Activator.CreateInstance(
        repoType, _ContextInstanceProvider, this.GetSchemaRoot()
      );

      return (RepositoryUntypingFacade)Activator.CreateInstance(repoFacadeType, repo);
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
