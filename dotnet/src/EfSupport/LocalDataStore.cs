using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;

namespace System.Data.Fuse.Ef {

  public class LocalDataStore : IDataStore {

    private SchemaRoot _SchemaRoot;

    public LocalDataStore(SchemaRoot schemaRoot) {
      _SchemaRoot = schemaRoot;
    }

    public void BeginTransaction() {
      throw new NotImplementedException();
    }

    public void CommitTransaction() {
      throw new NotImplementedException();
    }

    public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class {  
      return new LocalRepository<TEntity, TKey>(_SchemaRoot);
    }

    public SchemaRoot GetSchemaRoot() {
      return _SchemaRoot;
    }

    public void RollbackTransaction() {
      throw new NotImplementedException();
    }
  }

}
