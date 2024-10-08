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
  public class DelegateBasedDbContextInstanceProvider : IDbContextInstanceProvider {

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Func<DbContext> _ContextInstanceGetter;

    public DelegateBasedDbContextInstanceProvider(Func<DbContext>  contextInstanceGetter) {
      _ContextInstanceGetter = contextInstanceGetter;
    }

    public void VisitCurrentDbContext(Action<DbContext> visitorMethod) {
      DbContext currentInstance = _ContextInstanceGetter.Invoke();
      visitorMethod.Invoke(currentInstance);
    }

    public TReturn VisitCurrentDbContext<TReturn>(Func<DbContext, TReturn> visitorMethod) {
      DbContext currentInstance = _ContextInstanceGetter.Invoke();
      return visitorMethod.Invoke(currentInstance);
    }

  }

  /// <summary>
  /// Provides short-time access to a managed instance of an DbContext
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public class DelegateBasedDbContextInstanceProvider<TDbContext> : DelegateBasedDbContextInstanceProvider,
    IDbContextInstanceProvider<TDbContext>
    where TDbContext : DbContext {

    public DelegateBasedDbContextInstanceProvider(Func<TDbContext> contextInstanceGetter) :
    base(()=>contextInstanceGetter.Invoke()) {
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