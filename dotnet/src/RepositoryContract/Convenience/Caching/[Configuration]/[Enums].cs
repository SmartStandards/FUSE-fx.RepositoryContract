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

  /// <summary>
  /// Controls how cache reacts to changes that go through this wrapper.
  /// </summary>
  public enum ChangeProcessing {
    /// <summary>
    /// Only forward to inner repository; do not touch the cache.
    /// </summary>
    Decoupled = 0,

    /// <summary>
    /// Patch cache entries (key-cache always; query-cache best-effort).
    /// </summary>
    Patch = 1,

    /// <summary>
    /// Forward to inner repository and invalidate cache entries.
    /// </summary>
    Invalidate = 2
  }

  /// <summary>
  /// Controls when prefetching should occur.
  /// </summary>
  [Flags]
  public enum PrefetchTrigger {
    None = 0,
    OnStart = 1,
    OnInvalidate = 2
  }

  /// <summary>
  /// Read behavior when cached data is expired.
  /// </summary>
  public enum CacheReadMode {
    /// <summary>
    /// Expired cache blocks until refresh completed.
    /// </summary>
    Strict = 0,

    /// <summary>
    /// Expired cache returns stale data while refresh runs in background.
    /// </summary>
    AllowStaleWhileRefresh = 1
  }

}
