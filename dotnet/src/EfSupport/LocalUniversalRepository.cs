using System.Reflection;
using System.Linq;
using System.Data.ModelDescription;
using System.Collections.Generic;
using System.Data.ModelDescription.Convenience;

namespace System.Data.Fuse.Convenience {

  public class LocalUniversalRepository : UniversalRepository {

    private SchemaRoot _SchemaRoot;
    private Assembly _Assembly;

    public LocalUniversalRepository(Assembly assembly, SchemaRoot schema) {
      this._SchemaRoot = schema;
      this._Assembly = assembly;
    }

    protected override UniversalRepositoryFacade CreateInnerRepo(string entityName) {
      Type[] allTypes = _Assembly.GetTypes();
      Type entityType = allTypes.Where((Type t) => t.Name == entityName).FirstOrDefault();
      if (entityType == null) { return null; }

      Type keyType = ConversionHelper.GetKeyType(entityType, _SchemaRoot);

      Type repoFacadeType = typeof(DynamicRepositoryFacade<,>);
      repoFacadeType = repoFacadeType.MakeGenericType(entityType, keyType);

      Type repoType = typeof(LocalRepository<,>);
      repoType = repoType.MakeGenericType(entityType, keyType);
      object repo = Activator.CreateInstance(repoType, _SchemaRoot);

      return (UniversalRepositoryFacade)Activator.CreateInstance(repoFacadeType, repo);
    }

  }

}
