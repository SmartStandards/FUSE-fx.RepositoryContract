using System.Data.ModelDescription;
using System.Data.Fuse.Convenience.Internal;
using System.Data.Fuse.SchemaResolving;

namespace System.Data.Fuse.Convenience {

  public class InMemoryUniversalRepository : UniversalRepositoryBase {

    private SchemaRoot _SchemaRoot;

    public InMemoryUniversalRepository(SchemaRoot schemaRoot, IEntityResolver entityResolver) :
    base(entityResolver) {
      _SchemaRoot = schemaRoot;
    }

    protected override RepositoryUntypingFacade CreateInnerRepo(Type entityType) {

      Type keyType = ConversionHelper.GetKeyType(entityType, _SchemaRoot);

      Type repoFacadeType = typeof(DynamicRepositoryFacade<,>);
      repoFacadeType = repoFacadeType.MakeGenericType(entityType, keyType);

      Type repoType = typeof(InMemoryRepository<,>);
      repoType = repoType.MakeGenericType(entityType, keyType);
      object repo = Activator.CreateInstance(repoType, _SchemaRoot);

      return (RepositoryUntypingFacade)Activator.CreateInstance(repoFacadeType, repo);
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
