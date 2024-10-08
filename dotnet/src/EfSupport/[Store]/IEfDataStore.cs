#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif
using System.Data.Fuse.Ef.InstanceManagement;

namespace System.Data.Fuse.Ef {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public interface IEfDataStore : IDataStore {

    IDbContextInstanceProvider ContextInstanceProvider { get; }

  }

}
