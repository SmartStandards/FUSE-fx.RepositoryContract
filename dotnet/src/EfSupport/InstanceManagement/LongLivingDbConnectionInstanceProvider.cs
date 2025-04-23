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
  public class LongLivingDbConnectionInstanceProvider : IDbConnectionProvider, IDisposable {

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Func<IDbConnection> _Factory;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IDbConnection _CurrentInstance = null;

    public LongLivingDbConnectionInstanceProvider(Func<IDbConnection> factory, bool lazy) {
      _Factory = factory;
      if (!lazy) {
        _CurrentInstance = _Factory.Invoke();
      }
    }

    /// <summary></summary>
    /// <param name="instance">WARNING, the lifetime wont be managed here - you need to dispose the given context manually!</param>
    public LongLivingDbConnectionInstanceProvider(IDbConnection instance) {
      _CurrentInstance = instance;
      _Factory = null;
    }

    public void VisitCurrentConnection(Action<IDbConnection> visitorMethod) {
      if(_CurrentInstance == null && _Factory != null) {
        _CurrentInstance = _Factory.Invoke();
      }
      visitorMethod.Invoke(_CurrentInstance);   
    }

    public TReturn VisitCurrentConnection<TReturn>(Func<IDbConnection, TReturn> visitorMethod) {
      if (_CurrentInstance == null && _Factory != null) {
        _CurrentInstance = _Factory.Invoke();
      }
      return visitorMethod.Invoke(_CurrentInstance);
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

}
