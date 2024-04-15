using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data.Fuse.Convenience;

#if !NETCOREAPP
using System.Data.Entity;
#endif
using System.Data.ModelDescription;
using System.Data.ModelDescription.Convenience;
using System.Linq;
using System.Reflection;
using System.Linq.Dynamic.Core;

namespace System.Data.Fuse.Ef {

  public interface IEfDataStore : IDataStore {
    SchemaRoot SchemaRoot { get; }
  }

  public interface IEfDataStore<TDbContext> : IEfDataStore where TDbContext : DbContext {
    TDbContext DbContext { get; }
  }

  public class EfDataStore<TDbContext> : IEfDataStore<TDbContext> where TDbContext : DbContext {
    private readonly TDbContext _DbContext;
    private static SchemaRoot _SchemaRoot;
    private static Assembly _EntityAssembly;

    public SchemaRoot SchemaRoot {
      get {
        if (_SchemaRoot == null) {
#if NETCOREAPP
          string[] typenames = _DbContext.Model.GetEntityTypes().Select(t => t.Name).ToArray();
#else
          string[] typenames = _DbContext.GetManagedTypeNames();
#endif
          _SchemaRoot = ModelReader.GetSchema(_EntityAssembly, typenames);
        }
        return _SchemaRoot;
      }
    }

    public EfDataStore(TDbContext dbContext, Assembly entityAssembly) {
      this._DbContext = dbContext;
      _EntityAssembly = entityAssembly;
    }

    public TDbContext DbContext => _DbContext;

    public void BeginTransaction() {
      throw new NotImplementedException();
    }

    public void CommitTransaction() {
      throw new NotImplementedException();
    }

    public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class {

      //check if TEntity is part of the model
#if NETCOREAPP
      if (!(_DbContext.Model.GetEntityTypes().Any(t => t.Name == typeof(TEntity).FullName))) {
        throw new InvalidOperationException(
          $"Entity type {typeof(TEntity).FullName} is not part of the model for this context."
        );
      }
#else
     if (!(_DbContext.GetManagedTypeNames().Any(tn => tn == typeof(TEntity).FullName))) {
        throw new InvalidOperationException(
          $"Entity type {typeof(TEntity).FullName} is not part of the model for this context."
        );
      }
#endif

      return new EfRepository<TEntity, TKey>(_DbContext);
    }

    public void RollbackTransaction() {
      throw new NotImplementedException();
    }

    public SchemaRoot GetSchemaRoot() {
      return SchemaRoot;
    }
  }

}
