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
using System.Data.Fuse.Convenience;
using System.Collections.Generic;

namespace System.Data.Fuse.Ef {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public class EfDataStore<TDbContext> : IEfDataStore where TDbContext : DbContext {

    #region " SchemaRoot/Metadata Caching "

    private static SchemaRoot _SchemaRoot = null;
    public SchemaRoot GetSchemaRoot() {
      //TODO: wollen wir so eine funktionalität überhaupt hier,
      //oder verzichten wir lifer auf den konstruktor ohne schema
      if (_SchemaRoot == null) {
        Type dbContextType = null;
        _ContextInstanceProvider.VisitCurrentDbContext(
            (dbContext) => dbContextType = dbContext.GetType()
         );
        _SchemaRoot = SchemaCache.GetSchemaRootForContext(dbContextType);
      }
      return _SchemaRoot;
    }

    #endregion

    private IDbContextInstanceProvider _ContextInstanceProvider;
    public IDbContextInstanceProvider ContextInstanceProvider {
      get {
        return _ContextInstanceProvider;
      }
    }

    public EfDataStore(IDbContextInstanceProvider contextInstanceProvider) {
      _ContextInstanceProvider = contextInstanceProvider;
    }

    [Obsolete("This overload is unsave, because it doesnt care about lifetime management of the dbcontext!")]
    public EfDataStore(TDbContext dbContext) {
      _ContextInstanceProvider = new LongLivingDbContextInstanceProvider(dbContext);
    }

    public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class {

      //check if TEntity is part of the model
      _ContextInstanceProvider.VisitCurrentDbContext(
        (dbContext) => {
          string[] typeNames = SchemaCache.GetEntityTypeNamesForContext(dbContext);
          if (!(typeNames.Any(tn => tn == typeof(TEntity).FullName || tn == typeof(TEntity).Name))) {
            throw new InvalidOperationException(
              $"Entity type {typeof(TEntity).FullName} is not part of the model for this context."
            );
          }
        }
      );

      //HACK: sollte doch nicht immer eine neue instanz sein oder?
      return new EfRepository<TEntity, TKey>(_ContextInstanceProvider);
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

      Type[] types = (
        from p in typeof(TDbContext).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
        where p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition().Name.StartsWith("DbSet")
        select p.PropertyType.GetGenericArguments()[0]
      ).ToArray();

      if (types.Length == 0) {
        return new Tuple<Type, Type>[0];
      } else {
        return types.Select(t => new Tuple<Type, Type>(t, ConversionHelper.GetKeyType(t, GetSchemaRoot()))).ToArray();
      }
    }  

  }

}
