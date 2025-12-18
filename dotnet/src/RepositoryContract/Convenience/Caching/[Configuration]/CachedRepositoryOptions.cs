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
  /// Options for CachedRepository wrapper.
  /// </summary>
  public sealed class CachedRepositoryOptions<TEntity, TKey> where TEntity : class {

    private int[] _AccessExtensionSeconds;
    private TimeSpan _AbsoluteMaxLifetime;
    private bool _UseAbsoluteMaxLifetime;
    private ChangeProcessing _ChangeProcessing;
    private PrefetchTrigger _PrefetchTrigger;
    private CacheReadMode _ReadMode;

    private bool _EnableQueryCacheOptIn;
    private int _AutoEnableQueryCacheAboveEntityCount;
    private int _MaxQueryCacheEntries;

    private Func<TEntity, TKey> _KeySelector;
    private string[] _KeyFieldNames;
    private Func<Dictionary<string, object>, TKey> _KeyFromFieldDictionary;

    /// <summary>
    /// An array of access extension durations (seconds).
    /// Stage is increased per successful cache access up to last element.
    /// </summary>
    public int[] AccessExtensionSeconds {
      get { return _AccessExtensionSeconds; }
      set { _AccessExtensionSeconds = value; }
    }

    /// <summary>
    /// If enabled, cache entries will never outlive this absolute max lifetime since creation.
    /// </summary>
    public TimeSpan AbsoluteMaxLifetime {
      get { return _AbsoluteMaxLifetime; }
      set { _AbsoluteMaxLifetime = value; }
    }

    /// <summary>
    /// Enables absolute max lifetime constraint.
    /// </summary>
    public bool UseAbsoluteMaxLifetime {
      get { return _UseAbsoluteMaxLifetime; }
      set { _UseAbsoluteMaxLifetime = value; }
    }

    /// <summary>
    /// Controls how updates/adds/deletes influence the cache.
    /// </summary>
    public ChangeProcessing ChangeProcessing {
      get { return _ChangeProcessing; }
      set { _ChangeProcessing = value; }
    }

    /// <summary>
    /// Controls auto prefetch triggers.
    /// </summary>
    public PrefetchTrigger PrefetchTrigger {
      get { return _PrefetchTrigger; }
      set { _PrefetchTrigger = value; }
    }

    /// <summary>
    /// Read behavior when a cached query entry is expired.
    /// </summary>
    public CacheReadMode ReadMode {
      get { return _ReadMode; }
      set { _ReadMode = value; }
    }

    /// <summary>
    /// Enables query-scoped result caching (opt-in).
    /// </summary>
    public bool EnableQueryCacheOptIn {
      get { return _EnableQueryCacheOptIn; }
      set { _EnableQueryCacheOptIn = value; }
    }

    /// <summary>
    /// Automatically enables query cache if CountAll() is greater than this threshold.
    /// Default recommendation: 200.
    /// </summary>
    public int AutoEnableQueryCacheAboveEntityCount {
      get { return _AutoEnableQueryCacheAboveEntityCount; }
      set { _AutoEnableQueryCacheAboveEntityCount = value; }
    }

    /// <summary>
    /// Limits number of cached query entries to keep memory bounded.
    /// </summary>
    public int MaxQueryCacheEntries {
      get { return _MaxQueryCacheEntries; }
      set { _MaxQueryCacheEntries = value; }
    }

    /// <summary>
    /// Mandatory for best-effort patching of query results and key-cache.
    /// </summary>
    public Func<TEntity, TKey> KeySelector {
      get { return _KeySelector; }
      set { _KeySelector = value; }
    }

    /// <summary>
    /// Optional: list of key field names expected inside a Dictionary&lt;string, object&gt; from GetEntityFields*.
    /// Used for patching field dictionaries.
    /// </summary>
    public string[] KeyFieldNames {
      get { return _KeyFieldNames; }
      set { _KeyFieldNames = value; }
    }

    /// <summary>
    /// Optional: custom extractor for key from a field dictionary.
    /// If not provided, KeyFieldNames will be used with simple conversions when possible.
    /// </summary>
    public Func<Dictionary<string, object>, TKey> KeyFromFieldDictionary {
      get { return _KeyFromFieldDictionary; }
      set { _KeyFromFieldDictionary = value; }
    }

    /// <summary>
    /// Creates default options.
    /// </summary>
    public CachedRepositoryOptions() {
      _AccessExtensionSeconds = new int[] { 5, 15, 60 };
      _AbsoluteMaxLifetime = TimeSpan.FromMinutes(30);
      _UseAbsoluteMaxLifetime = false;
      _ChangeProcessing = ChangeProcessing.Patch;
      _PrefetchTrigger = PrefetchTrigger.None;
      _ReadMode = CacheReadMode.AllowStaleWhileRefresh;

      _EnableQueryCacheOptIn = false;
      _AutoEnableQueryCacheAboveEntityCount = 200;
      _MaxQueryCacheEntries = 250;

      _KeySelector = null;
      _KeyFieldNames = new string[0];
      _KeyFromFieldDictionary = null;
    }

  }

}
