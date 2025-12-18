using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Fuse.Convenience.Caching {

  public sealed partial class CachedRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class {

    internal sealed class CacheEntry {

      private readonly object _SyncRoot = new object();

      private object _Value;
      private DateTime _CreatedUtc;
      private DateTime _ExpiresUtc;
      private int _Stage;
      private int _Generation;
      private bool _IsRefreshing;
      private Task _RefreshTask;
      private DateTime _LastAccessUtc;

      public object SyncRoot {
        get { return _SyncRoot; }
      }

      public object Value {
        get { return _Value; }
        set { _Value = value; }
      }

      public DateTime CreatedUtc {
        get { return _CreatedUtc; }
        set { _CreatedUtc = value; }
      }

      public DateTime ExpiresUtc {
        get { return _ExpiresUtc; }
        set { _ExpiresUtc = value; }
      }

      public int Stage {
        get { return _Stage; }
        set { _Stage = value; }
      }

      public int Generation {
        get { return _Generation; }
        set { _Generation = value; }
      }

      public bool IsRefreshing {
        get { return _IsRefreshing; }
        set { _IsRefreshing = value; }
      }

      public Task RefreshTask {
        get { return _RefreshTask; }
        set { _RefreshTask = value; }
      }

      public DateTime LastAccessUtc {
        get { return _LastAccessUtc; }
        set { _LastAccessUtc = value; }
      }

    }

  }

}
