#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Metadata.Edm;
#endif
using System.Diagnostics;

namespace System.Data.Fuse.Sql.InstanceManagement {

  /// <summary>
  /// Provides short-time access to a managed instance of an DbContext
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public class ShortLivingDbConnectionInstanceProvider : IDbConnectionProvider {

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Func<IDbConnection> _Factory;

    public ShortLivingDbConnectionInstanceProvider(Func<IDbConnection> factory) {
      _Factory = factory;
    }

    public void VisitCurrentConnection(Action<IDbConnection> visitorMethod) {
      using (IDbConnection context = _Factory.Invoke()) {
        context.Open();
        try {
          visitorMethod.Invoke(context);
        } finally {
          context.Close();
        }
      }
    }

    public TReturn VisitCurrentConnection<TReturn>(Func<IDbConnection, TReturn> visitorMethod) {
      using (IDbConnection context = _Factory.Invoke()) {
        context.Open();
        try {
          return visitorMethod.Invoke(context);

        } finally {
          context.Close();
        }
      }
    }

  }

}