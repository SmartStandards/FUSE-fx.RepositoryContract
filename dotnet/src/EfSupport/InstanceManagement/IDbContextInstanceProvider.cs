#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Metadata.Edm;
#endif

namespace System.Data.Fuse.Ef.InstanceManagement {

  /// <summary>
  /// Provides short-time access to a managed instance of an DbContext
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public interface IDbContextInstanceProvider {

    void VisitCurrentDbContext(Action<DbContext> visitorMethod);

    TReturn VisitCurrentDbContext<TReturn>(Func<DbContext, TReturn> visitorMethod);

  }


  /// <summary>
  /// Provides short-time access to a managed instance of an DbContext
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public interface IDbContextInstanceProvider<TDbContext> : IDbContextInstanceProvider
  where TDbContext : DbContext {

    void VisitCurrentDbContext(Action<TDbContext> visitorMethod);

    TReturn VisitCurrentDbContext<TReturn>(Func<TDbContext, TReturn> visitorMethod);

  }

}