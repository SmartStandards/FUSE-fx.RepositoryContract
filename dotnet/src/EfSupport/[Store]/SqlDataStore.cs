#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif
using System.Data.ModelDescription;
using System.Linq;
using System.Reflection;
using System.Linq.Dynamic.Core;
using System.Data.Fuse.Ef.InstanceManagement;
using System.Data.Fuse.Sql;
using System.Data.ModelDescription.Convenience;
using System.Data.Fuse.Sql.InstanceManagement;

namespace System.Data.Fuse.Ef {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public class SqlDataStore : IDataStore {

    #region " SchemaRoot/Metadata Caching "

    private static SchemaRoot _SchemaRoot = null;
    public SchemaRoot GetSchemaRoot() {
      return _SchemaRoot;
    }

    #endregion


    private readonly Func<EntitySchema, string> _TableNameGetter = null;

    private IDbConnectionProvider _ConnectionProvider;
    private readonly Tuple<Type, Type>[] _ManagedTypes;
    private readonly string _SchemaName = null;

    public IDbConnectionProvider ConnectionProvider {
      get {
        return _ConnectionProvider;
      }
    }

    public SqlDataStore(
      IDbConnectionProvider connectionProvider,
      Tuple<Type, Type>[] managedTypes,
      Func<EntitySchema, string> tableNameGetter = null,
      string schemaName = null
    ) {
      _ConnectionProvider = connectionProvider;
      _ManagedTypes = managedTypes;
      _SchemaRoot = ModelReader.GetSchema(managedTypes.Select((mt) => mt.Item1).ToArray(), true);
      this._TableNameGetter = tableNameGetter != null ? tableNameGetter : (EntitySchema es) => es.NamePlural;
      this._SchemaName = schemaName;
    }

    public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class {

      //HACK: sollte doch nicht immer eine neue instanz sein oder?
      EntitySchema schema = _SchemaRoot.GetSchema(typeof(TEntity).Name);
      string tableName = _TableNameGetter.Invoke(schema);
      return new SqlRepository<TEntity, TKey>(_ConnectionProvider, _SchemaRoot, tableName, _SchemaName);
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
      return _ManagedTypes;
    }
  }

}
