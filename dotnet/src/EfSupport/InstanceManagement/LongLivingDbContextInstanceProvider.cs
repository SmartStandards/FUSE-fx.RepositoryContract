#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Metadata.Edm;
#endif
using System.Diagnostics;

namespace System.Data.Fuse.Ef.InstanceManagement {

  /// <summary>
  /// Provides short-time access to a managed instance of an DbContext
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public class LongLivingDbContextInstanceProvider : IDbContextInstanceProvider, IDisposable {

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Func<DbContext> _Factory;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private DbContext _CurrentInstance = null;

    public LongLivingDbContextInstanceProvider(Type dbContextType, bool lazy) {
      _Factory = ()=> (DbContext) Activator.CreateInstance(dbContextType);
      if (!lazy) {
        _CurrentInstance = _Factory.Invoke();
      }
    }

    public LongLivingDbContextInstanceProvider(Func<DbContext> factory, bool lazy) {
      _Factory = factory;
      if (!lazy) {
        _CurrentInstance = _Factory.Invoke();
      }
    }

    /// <summary></summary>
    /// <param name="instance">WARNING, the lifetime wont be managed here - you need to dispose the given context manually!</param>
    public LongLivingDbContextInstanceProvider(DbContext instance) {
      _CurrentInstance = instance;
      _Factory = null;
    }

    public void VisitCurrentDbContext(Action<DbContext> visitorMethod) {
      if(_CurrentInstance == null && _Factory != null) {
        _CurrentInstance = _Factory.Invoke();
      }
      lock (_CurrentInstance) {
        visitorMethod.Invoke(_CurrentInstance);
      }
    }

    public TReturn VisitCurrentDbContext<TReturn>(Func<DbContext, TReturn> visitorMethod) {
      if (_CurrentInstance == null && _Factory != null) {
        _CurrentInstance = _Factory.Invoke();
      }
      lock (_CurrentInstance) {
        return visitorMethod.Invoke(_CurrentInstance);
      }
    }

    public void Dispose() {

      //only if lifetime is not managed externally
      if (_Factory == null) {
        return;
      }

      if (_CurrentInstance != null) {

        if (_Factory == null) {
          _CurrentInstance.Dispose();
        }

        _CurrentInstance = null;
      }
    }

  }

  /// <summary>
  /// Provides short-time access to a managed instance of an DbContext
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public class LongLivingDbContextInstanceProvider<TDbContext> : LongLivingDbContextInstanceProvider,
  IDbContextInstanceProvider<TDbContext>
  where TDbContext : DbContext {

    /// <summary></summary>
    /// <param name="instance">WARNING, the lifetime wont be managed here - you need to dispose the given context manually!</param>
    public LongLivingDbContextInstanceProvider(TDbContext instance) :
    base((DbContext) instance) {
    }

    /// <summary>
    /// This will use the default constructor.
    /// </summary>
    public LongLivingDbContextInstanceProvider(bool lazy) :
    base(typeof(TDbContext), lazy) {
    }

    public LongLivingDbContextInstanceProvider(Func<TDbContext> factory, bool lazy) :
    base(()=> factory.Invoke(), lazy) {
    }

    public void VisitCurrentDbContext(Action<TDbContext> visitorMethod) {
      base.VisitCurrentDbContext((DbContext dbcontext) => {
        visitorMethod.Invoke((TDbContext)dbcontext);
      });
    }

    public TReturn VisitCurrentDbContext<TReturn>(Func<TDbContext, TReturn> visitorMethod) {
      return base.VisitCurrentDbContext<TReturn>((DbContext dbcontext) => {
        return visitorMethod.Invoke((TDbContext)dbcontext);
      });
    }

  }

}
