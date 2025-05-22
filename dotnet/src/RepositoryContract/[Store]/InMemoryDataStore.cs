using System.Data.Fuse.Convenience;
using System.Data.Fuse.SchemaResolving;
using System.Data.ModelDescription;

namespace System.Data.Fuse.Convenience {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public class InMemoryDataStore : IDataStore {

    private SchemaRoot _SchemaRoot;

    public InMemoryDataStore(SchemaRoot schemaRoot) {
      _SchemaRoot = schemaRoot;
    }

    public SchemaRoot GetSchemaRoot() {
      return _SchemaRoot;
    }

    public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class {  
      return new InMemoryRepository<TEntity, TKey>(_SchemaRoot);
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
      return Array.Empty<Tuple<Type, Type>>(); // TODO
    }
  }

}
