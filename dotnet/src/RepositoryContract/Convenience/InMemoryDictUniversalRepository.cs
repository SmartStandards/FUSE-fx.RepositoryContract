using System.Reflection;
using System.Linq;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Collections.Generic;
using System.Data.ModelDescription.Convenience;
using System.Data.Fuse.Convenience.Internal;
using System.Configuration.Assemblies;
using System.IO;
using System.Data.Fuse.SchemaResolving;

namespace System.Data.Fuse.Convenience {

  public class InMemoryDictUniversalRepository : UniversalRepositoryBase {

    protected readonly Func<PropertyInfo, Dictionary<string, object>, object, bool> _HandlePropertyModelToEntity;
    protected readonly Func<PropertyInfo, object, Dictionary<string, object>, bool> _HandlePropertyEntityToModel;

    SchemaRoot _SchemaRoot;

    public InMemoryDictUniversalRepository(SchemaRoot schemaRoot, IEntityResolver entityResolver) :
    base(entityResolver) {
      _SchemaRoot = schemaRoot;
    }

    // gets the key type of the entity. 
    // Uses the SchemaRoot.GetPrimaryKeyProperties method to get the key properties of the entity.
    // if there is only one key property, returns the type of that property.
    // if there are multiple key properties, returns the type of the ComposityKey-class with the matching
    // number of fields.
    private Type GetKeyType(Type entityType) {
      List<PropertyInfo> keyProperties = _SchemaRoot.GetPrimaryKeyProperties(entityType);

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

      if (entityType == null) {
        return null; 
      }

      Type modelType = typeof(Dictionary<string, object>);

      Type keyType = GetKeyType(entityType);

      Type repoFacadeType = typeof(DynamicRepositoryFacade<,>);
      repoFacadeType = repoFacadeType.MakeGenericType(modelType, typeof(object));

      Type repoType = typeof(InMemoryRepository<,>);
      repoType = repoType.MakeGenericType(entityType, keyType);
      object localRepo = Activator.CreateInstance(repoType, _SchemaRoot);

      object universalModelVsEntityRepo;
      if (_HandlePropertyModelToEntity == null && _HandlePropertyEntityToModel == null) {
        MethodInfo factoryMethod = typeof(ConversionHelper).GetMethod(nameof(ConversionHelper.CreateDictVsEntityRepositry));
        factoryMethod = factoryMethod.MakeGenericMethod(entityType, keyType);
        universalModelVsEntityRepo = factoryMethod.Invoke(
          null, 
          new object[] { _SchemaRoot, this, localRepo, NavigationRole.Lookup | NavigationRole.Dependent, true }
        );
      } else {
        Type universalModelVsEntityRepoType = typeof(DictVsEntityRepository<,>);
        universalModelVsEntityRepoType = universalModelVsEntityRepoType.MakeGenericType(entityType, keyType);
        universalModelVsEntityRepo = Activator.CreateInstance(
          universalModelVsEntityRepoType, localRepo, _HandlePropertyModelToEntity, _HandlePropertyEntityToModel
        );
      }

      return (RepositoryUntypingFacade)Activator.CreateInstance(repoFacadeType, universalModelVsEntityRepo);
    }

    public override string GetOriginIdentity() {
      //HACK: hier nochmal was sinnvolles ausdenken
      return "inmemory-" + this.GetHashCode().ToString();
    }

    public override RepositoryCapabilities GetCapabilities() {
      return new RepositoryCapabilities();
    }

  }

}
