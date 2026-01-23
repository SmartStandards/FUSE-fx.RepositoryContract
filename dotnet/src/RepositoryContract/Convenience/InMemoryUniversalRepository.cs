using System.Data.ModelDescription;
using System.Data.Fuse.Convenience.Internal;
using System.Data.Fuse.SchemaResolving;

namespace System.Data.Fuse.Convenience {

  public class InMemoryUniversalRepository : UniversalRepositoryBase {

    private SchemaRoot _SchemaRoot;
    private bool _MatchStringsCaseInsensitive = true; //more equal to SQL behavior
    private bool _NewModeWithoutLinqDynamic = false;

    [Obsolete("Use constructor with explicit 'matchStringsCaseInsensitive' parameter")]
    public InMemoryUniversalRepository(SchemaRoot schemaRoot, IEntityResolver entityResolver) :
      base(entityResolver)
    {
      _SchemaRoot = schemaRoot;
    }

    public InMemoryUniversalRepository(SchemaRoot schemaRoot, IEntityResolver entityResolver, bool matchStringsCaseInsensitive) :
      base(entityResolver)
    {
      _SchemaRoot = schemaRoot;
      _MatchStringsCaseInsensitive = matchStringsCaseInsensitive;
      _NewModeWithoutLinqDynamic = true; //gibts gratis mit diesem constructor (schleichende migration)
    }

    protected override RepositoryUntypingFacade CreateInnerRepo(Type entityType) {

      Type keyType = ConversionHelper.GetKeyType(entityType, _SchemaRoot);

      Type repoFacadeType = typeof(DynamicRepositoryFacade<,>);
      repoFacadeType = repoFacadeType.MakeGenericType(entityType, keyType);

      Type repoType = typeof(InMemoryRepository<,>);
      repoType = repoType.MakeGenericType(entityType, keyType);
      object repo;
      if (_NewModeWithoutLinqDynamic) {
        repo = Activator.CreateInstance(repoType, _SchemaRoot, _MatchStringsCaseInsensitive);
      }
      else {
        repo = Activator.CreateInstance(repoType, _SchemaRoot);
      }

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
