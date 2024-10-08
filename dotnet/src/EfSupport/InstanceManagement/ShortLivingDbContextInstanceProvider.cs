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
  public class ShortLivingDbContextInstanceProvider : IDbContextInstanceProvider {

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Func<DbContext> _Factory;

    public ShortLivingDbContextInstanceProvider(Type dbContextType) {
      _Factory = ()=> (DbContext) Activator.CreateInstance(dbContextType);
    }

    public ShortLivingDbContextInstanceProvider(Func<DbContext> factory) {
      _Factory = factory;
    }

    public void VisitCurrentDbContext(Action<DbContext> visitorMethod) {
      using (DbContext context = _Factory.Invoke()) {
        visitorMethod.Invoke(context);
      }
    }

    public TReturn VisitCurrentDbContext<TReturn>(Func<DbContext, TReturn> visitorMethod) {
      using (DbContext context = _Factory.Invoke()) {
        return visitorMethod.Invoke(context);
      }
    }

  }

  /// <summary>
  /// Provides short-time access to a managed instance of an DbContext
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public class ShortLivingDbContextInstanceProvider<TDbContext> : ShortLivingDbContextInstanceProvider,
  IDbContextInstanceProvider<TDbContext>
  where TDbContext : DbContext {

    /// <summary>
    /// This will use the default constructor.
    /// </summary>
    public ShortLivingDbContextInstanceProvider() :
    base(typeof(TDbContext)) {
    }

    public ShortLivingDbContextInstanceProvider(Func<TDbContext> factory) :
    base(()=> factory.Invoke()) {
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