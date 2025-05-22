using System.Collections.Generic;
using System.Data.ModelDescription;
using System.Linq;
using System.Reflection;

namespace System.Data.Fuse.Convenience {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public class RepoFactoryDataStore : IDataStore {

    private static Dictionary<Type, Func<object>> _RepositoryFactories = new Dictionary<Type, Func<object>>();
    private static List<Tuple<Type, Type>> _ManagedTypes = new List<Tuple<Type, Type>>();
    private static SchemaRoot _SchemaRoot;

    public static void RegisterRepositoryFactory<TModel,TKey>(
      Func<IRepository<TModel,TKey>> factory
    ) where TModel : class {
      if (_RepositoryFactories is null) {
        _RepositoryFactories = new Dictionary<Type, Func<object>>();
      }
      if (_RepositoryFactories.ContainsKey(typeof(TModel))) {
        return;
      }
      _RepositoryFactories[typeof(TModel)] = () => factory();
      _SchemaRoot = ModelReader.GetSchema(
        typeof(TModel).Assembly, _RepositoryFactories.Keys.Select((k) => k.Name).ToArray()
      );
      _ManagedTypes.Add(new Tuple<Type, Type>(typeof(TModel), typeof(TKey)));
    }

    public void BeginTransaction() {
    }

    public void CommitTransaction() {
    }

    public Tuple<Type, Type>[] GetManagedTypes() {
      return _ManagedTypes.ToArray();
    }

    public IRepository<TModel, TKey> GetRepository<TModel, TKey>() where TModel : class {
      if (_RepositoryFactories is null) {
        return null;
      }
      if (!_RepositoryFactories.ContainsKey(typeof(TModel))) {
        return null;
      }
      return (IRepository<TModel, TKey>)_RepositoryFactories[typeof(TModel)]();
    }

    public SchemaRoot GetSchemaRoot() {
      return _SchemaRoot;
    }

    public void RollbackTransaction() {
    }
  }

}