using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Data.ModelDescription;

namespace System.Data.Fuse.Convenience {

  public class RepositoryCollection : UniversalRepository, IDataStore  {

    private Dictionary<string, object> _ReposByEntityName = new Dictionary<string, object>();
    private Dictionary<string, Type> _KeyTypesByEntityName = new Dictionary<string, Type>();
    private SchemaRoot _SchemaRoot;

    private Assembly _Assembly;

    public void BeginTransaction() {
      throw new NotImplementedException();
    }

    public void CommitTransaction() {
      throw new NotImplementedException();
    }

    public void RollbackTransaction() {
      throw new NotImplementedException();
    }

    public void RegisterRepository<TEntity, TKey>(IRepository<TEntity, TKey> repository) where TEntity : class {
      if (_Assembly == null) {
        _Assembly = Assembly.GetAssembly(typeof(TEntity));
      }
      if (_ReposByEntityName.ContainsKey(typeof(TEntity).Name)) {
        return;
      }
      _ReposByEntityName.Add(typeof(TEntity).Name, repository);
      _KeyTypesByEntityName.Add(typeof(TEntity).Name, (typeof(TKey)));
      _SchemaRoot = ModelReader.GetSchema(_Assembly, _ReposByEntityName.Keys.ToArray());
    }

    public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class {
      if (_ReposByEntityName == null) { return null; }
      if (!_ReposByEntityName.ContainsKey(typeof(TEntity).Name)) { return null; }
      return (IRepository<TEntity, TKey>)_ReposByEntityName[typeof(TEntity).Name];
    }

    protected override UniversalRepositoryFacade CreateInnerRepo(string entityName) {

      if (_Assembly == null) return null;

      Type[] allTypes = _Assembly.GetTypes();
      Type entityType = allTypes.Where((Type t) => t.Name == entityName).FirstOrDefault();
      if (entityType == null) { return null; }

      if (_ReposByEntityName == null) { return null; }
      if (!_ReposByEntityName.ContainsKey(entityName)) { return null; }
      object repo = _ReposByEntityName[entityName];
      Type keyType = _KeyTypesByEntityName[entityName];

      Type repoFacadeType = typeof(DynamicRepositoryFacade<,>);
      repoFacadeType = repoFacadeType.MakeGenericType(entityType, keyType);
      return (UniversalRepositoryFacade)Activator.CreateInstance(repoFacadeType, repo);
    }

    public SchemaRoot GetSchemaRoot() {
      return _SchemaRoot;
    }
  }

}
