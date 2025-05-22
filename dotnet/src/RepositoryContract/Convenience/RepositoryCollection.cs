using System.Collections.Generic;
using System.Linq;
using System.Data.ModelDescription;
using System.Data.Fuse.Convenience.Internal;
using System.Data.Fuse.SchemaResolving;

namespace System.Data.Fuse.Convenience {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public class RepositoryCollection : UniversalRepositoryBase, IDataStore  {

    private Dictionary<Type, object> _ReposByEntityName = new Dictionary<Type, object>();
    private Dictionary<Type, Type> _KeyTypesByEntityName = new Dictionary<Type, Type>();

    private SchemaRoot _SchemaRoot;

    public RepositoryCollection(IEntityResolver entityResolver): base(entityResolver) {
    }

    public SchemaRoot GetSchemaRoot() {
      return _SchemaRoot;
    }

    public void RegisterRepository<TEntity, TKey>(IRepository<TEntity, TKey> repository) where TEntity : class {

      lock (_ReposByEntityName) {
        if (_ReposByEntityName.ContainsKey(typeof(TEntity))) {
          return;
        }
        _ReposByEntityName.Add(typeof(TEntity), repository);
      }

      lock (_ReposByEntityName) {
         _KeyTypesByEntityName.Add(typeof(TEntity), (typeof(TKey)));
      }

      _SchemaRoot = ModelReader.GetSchema(_ReposByEntityName.Keys.ToArray(), true);
    }

    public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class {
      if (_ReposByEntityName == null) { 
        return null;
      }
      lock (_ReposByEntityName) {
        if (!_ReposByEntityName.ContainsKey(typeof(TEntity))) {
          return null; 
        }
        return (IRepository<TEntity, TKey>) _ReposByEntityName[typeof(TEntity)];
      }
    }

    protected override RepositoryUntypingFacade CreateInnerRepo(Type entityType) {

      if (_ReposByEntityName == null) {
        return null; 
      }

      lock (_ReposByEntityName) { 
        if (!_ReposByEntityName.ContainsKey(entityType)) {
          return null; 
        }

        object repo = _ReposByEntityName[entityType];
        Type keyType = _KeyTypesByEntityName[entityType];

        Type repoFacadeType = typeof(DynamicRepositoryFacade<,>);
        repoFacadeType = repoFacadeType.MakeGenericType(entityType, keyType);
        return (RepositoryUntypingFacade)Activator.CreateInstance(repoFacadeType, repo);

      }
    }

    public override string GetOriginIdentity() {
      //HACK: hier muss was sinnvolles hin...
      return "RepositoryCollection";
    }
    public override RepositoryCapabilities GetCapabilities() {
      //HACK: hier muss was sinnvolles hin...
      return new RepositoryCapabilities();
    }

    public void BeginTransaction() {
      throw new NotImplementedException();
    }
    public void CommitTransaction() {
      throw new NotImplementedException();
    }
    public void RollbackTransaction() {
      throw new NotImplementedException();
    }

    public Tuple<Type, Type>[] GetManagedTypes() {
      return _KeyTypesByEntityName
        .Select(kvp => new Tuple<Type, Type>(kvp.Key, kvp.Value))
        .ToArray();
    }
  }

}
