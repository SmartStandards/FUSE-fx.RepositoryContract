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
  public interface IDbConnectionProvider {

    void VisitCurrentConnection(Action<IDbConnection> visitorMethod);

    TReturn VisitCurrentConnection<TReturn>(Func<IDbConnection, TReturn> visitorMethod);

  }

}