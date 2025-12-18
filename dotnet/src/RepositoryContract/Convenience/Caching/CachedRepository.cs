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
  /// Generic IRepository wrapper providing key-cache + optional query-scoped result caching.
  /// </summary>
  public sealed partial class CachedRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class {

    private readonly IRepository<TEntity, TKey> _InnerRepository;
    private readonly CachedRepositoryOptions<TEntity, TKey> _Options;

    private readonly ConcurrentDictionary<TKey, TEntity> _EntityByKeyCache;
    private readonly ConcurrentDictionary<TKey, EntityRef<TKey>> _RefByKeyCache;
    private readonly ConcurrentDictionary<TKey, Dictionary<string, object>> _FieldsByKeyCache;

    private readonly ConcurrentDictionary<string, CacheEntry> _QueryCache;
    private readonly ConcurrentQueue<string> _QueryCacheFifoKeys;

    private readonly object _QueryCacheEvictionSync = new object();

    private int _QueryCacheEnabledMode; // 0 unknown, 1 disabled, 2 enabled
    private DateTime _QueryCacheEnabledModeCheckedUtc;

    private int _PrefetchStarted; // 0 = false, 1 = true (bool not supported by Interlocked-Class!)

    private long _HitCount;
    private long _MissCount;
    private long _RefreshCount;
    private int _ActiveRefreshCount;

    // DateTime atomar über Ticks speichern (UTC)
    private long _LastRefreshAttemptUtcTicks;
    private long _LastSuccessfulRefreshUtcTicks;

    /// <summary>
    /// Creates a caching wrapper.
    /// </summary>
    public CachedRepository(IRepository<TEntity, TKey> innerRepository, CachedRepositoryOptions<TEntity, TKey> options) {

      if (innerRepository == null) {
        throw new ArgumentNullException("innerRepository");
      }
      if (options == null) {
        throw new ArgumentNullException("options");
      }

      _InnerRepository = innerRepository;
      _Options = options;

      _EntityByKeyCache = new ConcurrentDictionary<TKey, TEntity>();
      _RefByKeyCache = new ConcurrentDictionary<TKey, EntityRef<TKey>>();
      _FieldsByKeyCache = new ConcurrentDictionary<TKey, Dictionary<string, object>>();

      _QueryCache = new ConcurrentDictionary<string, CacheEntry>(StringComparer.Ordinal);
      _QueryCacheFifoKeys = new ConcurrentQueue<string>();

      _QueryCacheEnabledMode = 0;
      _QueryCacheEnabledModeCheckedUtc = DateTime.MinValue;

      this.TryStartPrefetchIfRequested();
    }

    /// <summary>
    /// Returns identity of the wrapped origin (not the cache).
    /// </summary>
    public string GetOriginIdentity() {
      return _InnerRepository.GetOriginIdentity();
    }

    /// <summary>
    /// Returns capabilities of the wrapped repository.
    /// </summary>
    public RepositoryCapabilities GetCapabilities() {
      return _InnerRepository.GetCapabilities();
    }

    public EntityRef<TKey>[] GetEntityRefs(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      this.TryStartPrefetchIfRequested();
      string cacheKey = this.BuildQueryKey("GetEntityRefs", filter, null, null, sortedBy, limit, skip);

      EntityRef<TKey>[] result = this.TryGetOrLoadQuery(cacheKey, () => {
        EntityRef<TKey>[] loaded = _InnerRepository.GetEntityRefs(filter, sortedBy, limit, skip);
        this.SeedKeyRefCache(loaded);
        return loaded;
      });

      return result;
    }

    public EntityRef<TKey>[] GetEntityRefsBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      this.TryStartPrefetchIfRequested();
      string cacheKey = this.BuildQueryKey("GetEntityRefsBySearchExpression", null, searchExpression, null, sortedBy, limit, skip);

      EntityRef<TKey>[] result = this.TryGetOrLoadQuery(cacheKey, () => {
        EntityRef<TKey>[] loaded = _InnerRepository.GetEntityRefsBySearchExpression(searchExpression, sortedBy, limit, skip);
        this.SeedKeyRefCache(loaded);
        return loaded;
      });

      return result;
    }

    public EntityRef<TKey>[] GetEntityRefsByKey(TKey[] keysToLoad) {
      this.TryStartPrefetchIfRequested();
      if (keysToLoad == null) {
        return new EntityRef<TKey>[0];
      }

      EntityRef<TKey>[] fromCache = new EntityRef<TKey>[keysToLoad.Length];
      List<TKey> missing = new List<TKey>(keysToLoad.Length);

      int i;
      for (i = 0; i < keysToLoad.Length; i++) {
        TKey key = keysToLoad[i];
        EntityRef<TKey> cached;
        if (_RefByKeyCache.TryGetValue(key, out cached)) {
          fromCache[i] = cached;
          this.IncrementHit();
        }
        else {
          fromCache[i] = null;
          missing.Add(key);
          this.IncrementMiss();
        }
      }

      if (missing.Count == 0) {
        return this.FilterNull(fromCache);
      }

      EntityRef<TKey>[] loaded = _InnerRepository.GetEntityRefsByKey(missing.ToArray());
      this.SeedKeyRefCache(loaded);

      Dictionary<TKey, EntityRef<TKey>> loadedMap = this.ToRefMap(loaded);

      for (i = 0; i < keysToLoad.Length; i++) {
        if (fromCache[i] == null) {
          TKey key = keysToLoad[i];
          EntityRef<TKey> loadedRef;
          if (loadedMap.TryGetValue(key, out loadedRef)) {
            fromCache[i] = loadedRef;
          }
        }
      }

      return this.FilterNull(fromCache);
    }

    public TEntity[] GetEntities(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      this.TryStartPrefetchIfRequested();
      string cacheKey = this.BuildQueryKey("GetEntities", filter, null, null, sortedBy, limit, skip);

      TEntity[] result = this.TryGetOrLoadQuery(cacheKey, () => {
        TEntity[] loaded = _InnerRepository.GetEntities(filter, sortedBy, limit, skip);
        this.SeedKeyEntityCache(loaded);
        return loaded;
      });

      return result;
    }

    public TEntity[] GetEntitiesBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      this.TryStartPrefetchIfRequested();
      string cacheKey = this.BuildQueryKey("GetEntitiesBySearchExpression", null, searchExpression, null, sortedBy, limit, skip);

      TEntity[] result = this.TryGetOrLoadQuery(cacheKey, () => {
        TEntity[] loaded = _InnerRepository.GetEntitiesBySearchExpression(searchExpression, sortedBy, limit, skip);
        this.SeedKeyEntityCache(loaded);
        return loaded;
      });

      return result;
    }

    public TEntity[] GetEntitiesByKey(TKey[] keysToLoad) {
      this.TryStartPrefetchIfRequested();
      if (keysToLoad == null) {
        return new TEntity[0];
      }

      TEntity[] fromCache = new TEntity[keysToLoad.Length];
      List<TKey> missing = new List<TKey>(keysToLoad.Length);

      int i;
      for (i = 0; i < keysToLoad.Length; i++) {
        TKey key = keysToLoad[i];
        TEntity cached;
        if (_EntityByKeyCache.TryGetValue(key, out cached)) {
          fromCache[i] = cached;
          this.IncrementHit();
        }
        else {
          fromCache[i] = null;
          missing.Add(key);
          this.IncrementMiss();
        }
      }

      if (missing.Count == 0) {
        return this.FilterNull(fromCache);
      }

      TEntity[] loaded = _InnerRepository.GetEntitiesByKey(missing.ToArray());
      this.SeedKeyEntityCache(loaded);

      Dictionary<TKey, TEntity> loadedMap = this.ToEntityMap(loaded);

      for (i = 0; i < keysToLoad.Length; i++) {
        if (fromCache[i] == null) {
          TKey key = keysToLoad[i];
          TEntity loadedEntity;
          if (loadedMap.TryGetValue(key, out loadedEntity)) {
            fromCache[i] = loadedEntity;
          }
        }
      }

      return this.FilterNull(fromCache);
    }

    public Dictionary<string, object>[] GetEntityFields(ExpressionTree filter, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      this.TryStartPrefetchIfRequested();
      string cacheKey = this.BuildQueryKey("GetEntityFields", filter, null, includedFieldNames, sortedBy, limit, skip);

      Dictionary<string, object>[] result = this.TryGetOrLoadQuery(cacheKey, () => {
        Dictionary<string, object>[] loaded = _InnerRepository.GetEntityFields(filter, includedFieldNames, sortedBy, limit, skip);
        this.SeedKeyFieldsCache(loaded);
        return loaded;
      });

      return result;
    }

    public Dictionary<string, object>[] GetEntityFieldsBySearchExpression(string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      this.TryStartPrefetchIfRequested();
      string cacheKey = this.BuildQueryKey("GetEntityFieldsBySearchExpression", null, searchExpression, includedFieldNames, sortedBy, limit, skip);

      Dictionary<string, object>[] result = this.TryGetOrLoadQuery(cacheKey, () => {
        Dictionary<string, object>[] loaded = _InnerRepository.GetEntityFieldsBySearchExpression(searchExpression, includedFieldNames, sortedBy, limit, skip);
        this.SeedKeyFieldsCache(loaded);
        return loaded;
      });

      return result;
    }

    public Dictionary<string, object>[] GetEntityFieldsByKey(TKey[] keysToLoad, string[] includedFieldNames) {
      this.TryStartPrefetchIfRequested();
      if (keysToLoad == null) {
        return new Dictionary<string, object>[0];
      }

      Dictionary<string, object>[] fromCache = new Dictionary<string, object>[keysToLoad.Length];
      List<TKey> missing = new List<TKey>(keysToLoad.Length);

      int i;
      for (i = 0; i < keysToLoad.Length; i++) {
        TKey key = keysToLoad[i];
        Dictionary<string, object> cached;
        if (_FieldsByKeyCache.TryGetValue(key, out cached)) {
          fromCache[i] = this.FilterFieldDictionary(cached, includedFieldNames);
          this.IncrementHit();
        }
        else {
          fromCache[i] = null;
          missing.Add(key);
          this.IncrementMiss();
        }
      }

      if (missing.Count == 0) {
        return this.FilterNull(fromCache);
      }

      Dictionary<string, object>[] loaded = _InnerRepository.GetEntityFieldsByKey(missing.ToArray(), includedFieldNames);
      this.SeedKeyFieldsCache(loaded);

      Dictionary<TKey, Dictionary<string, object>> loadedMap = this.ToFieldsMap(loaded);

      for (i = 0; i < keysToLoad.Length; i++) {
        if (fromCache[i] == null) {
          TKey key = keysToLoad[i];
          Dictionary<string, object> loadedFields;
          if (loadedMap.TryGetValue(key, out loadedFields)) {
            fromCache[i] = loadedFields;
          }
        }
      }

      return this.FilterNull(fromCache);
    }

    public int CountAll() {
      this.TryStartPrefetchIfRequested();
      return _InnerRepository.CountAll();
    }

    public int Count(ExpressionTree filter) {
      this.TryStartPrefetchIfRequested();

      if (this.IsQueryCacheEnabled()) {
        string cacheKey = this.BuildQueryKey("Count", filter, null, null, null, 0, 0);
        int result = this.TryGetOrLoadQuery(cacheKey, () => {
          int loaded = _InnerRepository.Count(filter);
          return loaded;
        });
        return result;
      }

      return _InnerRepository.Count(filter);
    }

    public int CountBySearchExpression(string searchExpression) {
      this.TryStartPrefetchIfRequested();

      if (this.IsQueryCacheEnabled()) {
        string cacheKey = this.BuildQueryKey("CountBySearchExpression", null, searchExpression, null, null, 0, 0);
        int result = this.TryGetOrLoadQuery(cacheKey, () => {
          int loaded = _InnerRepository.CountBySearchExpression(searchExpression);
          return loaded;
        });
        return result;
      }

      return _InnerRepository.CountBySearchExpression(searchExpression);
    }

    public bool ContainsKey(TKey key) {
      this.TryStartPrefetchIfRequested();

      if (_EntityByKeyCache.ContainsKey(key)) {
        this.IncrementHit();
        return true;
      }

      bool exists = _InnerRepository.ContainsKey(key);
      if (exists) {
        this.IncrementMiss();
      }
      else {
        this.IncrementMiss();
      }
      return exists;
    }

    public Dictionary<string, object> AddOrUpdateEntityFields(Dictionary<string, object> fields) {
      Dictionary<string, object> result = _InnerRepository.AddOrUpdateEntityFields(fields);
      this.ProcessChangeFromFields(result, ChangeKind.UpsertFields);
      return result;
    }

    public TEntity AddOrUpdateEntity(TEntity entity) {
      TEntity result = _InnerRepository.AddOrUpdateEntity(entity);
      this.ProcessChangeFromEntity(result, ChangeKind.UpsertEntity);
      return result;
    }

    public Dictionary<string, object> TryUpdateEntityFields(Dictionary<string, object> fields) {
      Dictionary<string, object> result = _InnerRepository.TryUpdateEntityFields(fields);
      this.ProcessChangeFromFields(result, ChangeKind.UpdateFields);
      return result;
    }

    public TEntity TryUpdateEntity(TEntity entity) {
      TEntity result = _InnerRepository.TryUpdateEntity(entity);
      this.ProcessChangeFromEntity(result, ChangeKind.UpdateEntity);
      return result;
    }

    public TKey TryAddEntity(TEntity entity) {
      TKey key = _InnerRepository.TryAddEntity(entity);
      this.ProcessChangeFromEntity(entity, ChangeKind.AddEntity);
      return key;
    }

    public TKey[] MassupdateByKeys(TKey[] keysToUpdate, Dictionary<string, object> fields) {
      TKey[] updatedKeys = _InnerRepository.MassupdateByKeys(keysToUpdate, fields);
      this.ProcessMassChange(updatedKeys);
      return updatedKeys;
    }

    public TKey[] Massupdate(ExpressionTree filter, Dictionary<string, object> fields) {
      TKey[] updatedKeys = _InnerRepository.Massupdate(filter, fields);
      this.ProcessMassChange(updatedKeys);
      return updatedKeys;
    }

    public TKey[] MassupdateBySearchExpression(string searchExpression, Dictionary<string, object> fields) {
      TKey[] updatedKeys = _InnerRepository.MassupdateBySearchExpression(searchExpression, fields);
      this.ProcessMassChange(updatedKeys);
      return updatedKeys;
    }

    public TKey[] TryDeleteEntities(TKey[] keysToDelete) {
      TKey[] deletedKeys = _InnerRepository.TryDeleteEntities(keysToDelete);
      this.ProcessDelete(deletedKeys);
      return deletedKeys;
    }

    public bool TryUpdateKey(TKey currentKey, TKey newKey) {
      bool ok = _InnerRepository.TryUpdateKey(currentKey, newKey);
      if (ok) {
        this.ProcessKeyChange(currentKey, newKey);
      }
      return ok;
    }

    // -----------------------------
    // Internals
    // -----------------------------

    private enum ChangeKind {
      UpsertFields = 0,
      UpdateFields = 1,
      UpsertEntity = 2,
      UpdateEntity = 3,
      AddEntity = 4
    }

    private void TryStartPrefetchIfRequested() {
      if (_PrefetchStarted == 1) {
        return;
      }

      if ((_Options.PrefetchTrigger & PrefetchTrigger.OnStart) == PrefetchTrigger.OnStart) {

        if (Interlocked.Exchange(ref _PrefetchStarted, 1) == 1) {
          return;
        }

        // Prefetch starten
        this.QueueBackgroundRefreshAllQueriesBestEffort("Prefetch.OnStart");
      }
    }

    private bool IsQueryCacheEnabled() {
      if (_Options.EnableQueryCacheOptIn) {
        return true;
      }

      DateTime nowUtc = DateTime.UtcNow;
      if (_QueryCacheEnabledMode != 0) {
        if ((nowUtc - _QueryCacheEnabledModeCheckedUtc).TotalSeconds < 30) {
          return _QueryCacheEnabledMode == 2;
        }
      }

      int countAll;
      try {
        countAll = _InnerRepository.CountAll();
      }
      catch (Exception ex) {
        //DevLogger.LogError(ex);
        _QueryCacheEnabledMode = 1;
        _QueryCacheEnabledModeCheckedUtc = nowUtc;
        return false;
      }

      _QueryCacheEnabledModeCheckedUtc = nowUtc;
      if (countAll > _Options.AutoEnableQueryCacheAboveEntityCount) {
        _QueryCacheEnabledMode = 2;
        return true;
      }

      _QueryCacheEnabledMode = 1;
      return false;
    }

    private void IncrementHit() {
      Interlocked.Increment(ref _HitCount);
    }

    private void IncrementMiss() {
      Interlocked.Increment(ref _MissCount);
    }

    private void IncrementRefresh() {
      Interlocked.Increment(ref _RefreshCount);
    }

    private void IncrementActiveRefresh() {
      Interlocked.Increment(ref _ActiveRefreshCount);
    }

    private void DecrementActiveRefresh() {
      Interlocked.Decrement(ref _ActiveRefreshCount);
    }

    private string BuildQueryKey(string methodName, ExpressionTree filter, string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit, int skip) {
      StringBuilder sb = new StringBuilder(512);
      sb.Append(methodName);
      sb.Append("|");
      sb.Append("origin=");
      sb.Append(this.GetOriginIdentity());
      sb.Append("|");

      if (filter != null) {
        sb.Append("filter=");
        sb.Append(this.StabilizeExpressionTree(filter));
        sb.Append("|");
      }
      else {
        sb.Append("filter=");
        sb.Append("-");
        sb.Append("|");
      }

      if (searchExpression != null) {
        sb.Append("search=");
        sb.Append(searchExpression);
        sb.Append("|");
      }
      else {
        sb.Append("search=");
        sb.Append("-");
        sb.Append("|");
      }

      if (includedFieldNames != null) {
        sb.Append("fields=");
        sb.Append(string.Join(",", includedFieldNames));
        sb.Append("|");
      }
      else {
        sb.Append("fields=");
        sb.Append("-");
        sb.Append("|");
      }

      if (sortedBy != null) {
        sb.Append("sort=");
        sb.Append(string.Join(",", sortedBy));
        sb.Append("|");
      }
      else {
        sb.Append("sort=");
        sb.Append("-");
        sb.Append("|");
      }

      sb.Append("limit=");
      sb.Append(limit.ToString(CultureInfo.InvariantCulture));
      sb.Append("|");

      sb.Append("skip=");
      sb.Append(skip.ToString(CultureInfo.InvariantCulture));

      return sb.ToString();
    }

    private string StabilizeExpressionTree(ExpressionTree tree) {
      StringBuilder sb = new StringBuilder(256);
      this.AppendExpressionTree(sb, tree);
      return sb.ToString();
    }

    private void AppendExpressionTree(StringBuilder sb, ExpressionTree tree) {
      if (tree == null) {
        sb.Append("null");
        return;
      }

      sb.Append("{");
      sb.Append("all=");
      sb.Append(tree.MatchAll ? "1" : "0");
      sb.Append(",neg=");
      sb.Append(tree.Negate ? "1" : "0");

      if (tree.Predicates != null) {
        FieldPredicate[] preds = tree.Predicates
          .OrderBy((p) => p.FieldName, StringComparer.Ordinal)
          .ThenBy((p) => p.Operator, StringComparer.Ordinal)
          .ThenBy((p) => p.ToString(), StringComparer.Ordinal)
          .ToArray();

        sb.Append(",p=[");
        int i;
        for (i = 0; i < preds.Length; i++) {
          if (i > 0) {
            sb.Append(";");
          }
          FieldPredicate p = preds[i];
          sb.Append(p.FieldName);
          sb.Append(p.Operator);
          sb.Append(p.ToString());
        }
        sb.Append("]");
      }

      if (tree.SubTree != null) {
        sb.Append(",s=[");
        int j;
        for (j = 0; j < tree.SubTree.Count; j++) {
          if (j > 0) {
            sb.Append("|");
          }
          this.AppendExpressionTree(sb, tree.SubTree[j]);
        }
        sb.Append("]");
      }

      sb.Append("}");
    }

    private T TryGetOrLoadQuery<T>(string cacheKey, Func<T> loader) {
      bool queryCacheEnabled = this.IsQueryCacheEnabled();
      if (!queryCacheEnabled) {
        this.IncrementMiss();
        return loader();
      }

      CacheEntry entry = _QueryCache.GetOrAdd(cacheKey, (k) => {
        CacheEntry created = new CacheEntry();
        created.Value = null;
        created.CreatedUtc = DateTime.UtcNow;
        created.LastAccessUtc = DateTime.UtcNow;
        created.Stage = 0;
        created.Generation = 0;
        created.IsRefreshing = false;
        created.RefreshTask = null;
        created.ExpiresUtc = this.ComputeNextExpiryUtc(created.CreatedUtc, created.Stage, created.CreatedUtc);
        _QueryCacheFifoKeys.Enqueue(k);
        this.EnforceQueryCacheLimit();
        return created;
      });

      DateTime nowUtc = DateTime.UtcNow;

      lock (entry.SyncRoot) {

        entry.LastAccessUtc = nowUtc;

        if (entry.Value != null) {
          if (nowUtc <= entry.ExpiresUtc) {
            entry.Stage = this.ComputeNextStage(entry.Stage);
            entry.ExpiresUtc = this.ComputeNextExpiryUtc(entry.CreatedUtc, entry.Stage, nowUtc);
            this.IncrementHit();
            return (T)entry.Value;
          }

          // expired but has stale value
          if (_Options.ReadMode == CacheReadMode.AllowStaleWhileRefresh) {
            this.IncrementHit();
            this.TryStartRefreshSingleFlight(entry, cacheKey, loader, nowUtc, "StaleWhileRefresh");
            return (T)entry.Value;
          }

          // strict mode => block until refreshed
          this.TryStartRefreshSingleFlight(entry, cacheKey, loader, nowUtc, "StrictExpired");
          if (entry.RefreshTask != null) {
            entry.RefreshTask.Wait();
          }

          if (entry.Value != null) {
            this.IncrementHit();
            return (T)entry.Value;
          }

          this.IncrementMiss();
          return loader();
        }

        // cold miss
        this.IncrementMiss();

        entry.IsRefreshing = true;
        entry.Generation = entry.Generation + 1;

        Interlocked.Exchange(ref _LastRefreshAttemptUtcTicks , nowUtc.Ticks);

        this.IncrementRefresh();
        this.IncrementActiveRefresh();

        try {
          T loaded = loader();
          entry.Value = loaded;
          entry.CreatedUtc = nowUtc;
          entry.Stage = 0;
          entry.ExpiresUtc = this.ComputeNextExpiryUtc(entry.CreatedUtc, entry.Stage, nowUtc);
          entry.IsRefreshing = false;

          Interlocked.Exchange(ref _LastSuccessfulRefreshUtcTicks , DateTime.UtcNow.Ticks);
          return loaded;
        }
        finally {
          entry.IsRefreshing = false;
          this.DecrementActiveRefresh();
        }

      }
    }

    private void TryStartRefreshSingleFlight<T>(CacheEntry entry, string cacheKey, Func<T> loader, DateTime nowUtc, string reason) {
      if (entry.IsRefreshing) {
        return;
      }

      entry.IsRefreshing = true;
      entry.Generation = entry.Generation + 1;
      int generation = entry.Generation;

      Interlocked.Exchange(ref _LastRefreshAttemptUtcTicks, nowUtc.Ticks);

      this.IncrementRefresh();
      this.IncrementActiveRefresh();

      Task refreshTask = Task.Factory.StartNew(() => {
        try {
          T loaded = loader();

          lock (entry.SyncRoot) {
            if (entry.Generation != generation) {
              return;
            }

            entry.Value = loaded;
            entry.CreatedUtc = DateTime.UtcNow;
            entry.Stage = 0;
            entry.ExpiresUtc = this.ComputeNextExpiryUtc(entry.CreatedUtc, entry.Stage, DateTime.UtcNow);
            entry.IsRefreshing = false;
            entry.RefreshTask = null;

            Interlocked.Exchange(ref _LastSuccessfulRefreshUtcTicks, DateTime.UtcNow.Ticks);
          }
        }
        catch (Exception ex) {

          //DevLogger.LogError(ex);

          lock (entry.SyncRoot) {
            entry.IsRefreshing = false;
            entry.RefreshTask = null;
          }

        }
        finally {
          this.DecrementActiveRefresh();
        }
      }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

      entry.RefreshTask = refreshTask;

      //DevLogger.LogTrace(0, 99999, "Cache refresh started: " + reason + " key=" + cacheKey);
    }

    private DateTime ComputeNextExpiryUtc(DateTime createdUtc, int stage, DateTime nowUtc) {
      int[] steps = _Options.AccessExtensionSeconds;
      if (steps == null || steps.Length == 0) {
        return nowUtc.AddSeconds(10);
      }

      int idx = stage;
      if (idx < 0) {
        idx = 0;
      }
      if (idx >= steps.Length) {
        idx = steps.Length - 1;
      }

      DateTime next = nowUtc.AddSeconds(steps[idx]);

      if (_Options.UseAbsoluteMaxLifetime) {
        DateTime absoluteMax = createdUtc.Add(_Options.AbsoluteMaxLifetime);
        if (next > absoluteMax) {
          next = absoluteMax;
        }
      }

      return next;
    }

    private int ComputeNextStage(int stage) {
      int[] steps = _Options.AccessExtensionSeconds;
      if (steps == null) {
        return 0;
      }
      if (steps.Length <= 1) {
        return 0;
      }
      if (stage < steps.Length - 1) {
        return stage + 1;
      }
      return stage;
    }

    private void EnforceQueryCacheLimit() {
      int max = _Options.MaxQueryCacheEntries;
      if (max <= 0) {
        return;
      }

      lock (_QueryCacheEvictionSync) {
        while (_QueryCache.Count > max) {
          string key;
          if (!_QueryCacheFifoKeys.TryDequeue(out key)) {
            break;
          }
          CacheEntry removed;
          _QueryCache.TryRemove(key, out removed);
        }
      }
    }

    private void QueueBackgroundRefreshAllQueriesBestEffort(string reason) {
      Task.Factory.StartNew(() => {
        try {
          string[] keys = _QueryCache.Keys.ToArray();
          int i;
          for (i = 0; i < keys.Length; i++) {
            string key = keys[i];
            CacheEntry entry;
            if (!_QueryCache.TryGetValue(key, out entry)) {
              continue;
            }

            lock (entry.SyncRoot) {
              if (entry.Value != null) {
                entry.ExpiresUtc = DateTime.UtcNow.AddSeconds(0);
              }
            }
          }
        }
        catch (Exception ex) {
          //DevLogger.LogError(ex);
        }
      }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

      //DevLogger.LogTrace(0, 99999, "Prefetch scheduled: " + reason);
    }

    private void SeedKeyEntityCache(TEntity[] entities) {
      if (entities == null) {
        return;
      }
      if (_Options.KeySelector == null) {
        return;
      }

      int i;
      for (i = 0; i < entities.Length; i++) {
        TEntity entity = entities[i];
        if (entity == null) {
          continue;
        }
        TKey key = _Options.KeySelector(entity);
        _EntityByKeyCache[key] = entity;
      }
    }

    private void SeedKeyRefCache(EntityRef<TKey>[] refs) {
      if (refs == null) {
        return;
      }

      int i;
      for (i = 0; i < refs.Length; i++) {
        EntityRef<TKey> er = refs[i];
        if (er == null) {
          continue;
        }

        TKey key;
        if (this.TryGetEntityRefKey(er, out key)) {
          _RefByKeyCache[key] = er;
        }
      }
    }

    private void SeedKeyFieldsCache(Dictionary<string, object>[] fields) {
      if (fields == null) {
        return;
      }

      int i;
      for (i = 0; i < fields.Length; i++) {
        Dictionary<string, object> bag = fields[i];
        if (bag == null) {
          continue;
        }

        TKey key;
        if (this.TryExtractKeyFromFields(bag, out key)) {
          _FieldsByKeyCache[key] = bag;
        }
      }
    }

    private bool TryGetEntityRefKey(EntityRef<TKey> entityRef, out TKey key) {
      key = default(TKey);

      if (entityRef == null) {
        return false;
      }

      PropertyInfo prop = entityRef.GetType().GetProperty("Key", BindingFlags.Public | BindingFlags.Instance);
      if (prop == null) {
        return false;
      }

      object value = prop.GetValue(entityRef, null);
      if (value == null) {
        return false;
      }

      if (value is TKey) {
        key = (TKey)value;
        return true;
      }

      return false;
    }

    private bool TryExtractKeyFromFields(Dictionary<string, object> fields, out TKey key) {
      key = default(TKey);

      if (fields == null) {
        return false;
      }

      if (_Options.KeyFromFieldDictionary != null) {
        try {
          key = _Options.KeyFromFieldDictionary(fields);
          return true;
        }
        catch (Exception ex) { 
          //DevLogger.LogError(ex);
          return false;
        }
      }

      string[] names = _Options.KeyFieldNames;
      if (names == null || names.Length == 0) {
        return false;
      }

      if (names.Length == 1) {
        object raw;
        if (!fields.TryGetValue(names[0], out raw)) {
          return false;
        }
        if (raw == null) {
          return false;
        }
        if (raw is TKey) {
          key = (TKey)raw;
          return true;
        }

        try {
          key = (TKey)Convert.ChangeType(raw, typeof(TKey), CultureInfo.InvariantCulture);
          return true;
        }
        catch (Exception ex) {
          //DevLogger.LogError(ex);
          return false;
        }
      }

      // Composite keys are not supported here without a custom extractor.
      return false;
    }

    private void ProcessChangeFromEntity(TEntity entity, ChangeKind kind) {
      if (_Options.ChangeProcessing == ChangeProcessing.Decoupled) {
        return;
      }

      if (_Options.ChangeProcessing == ChangeProcessing.Invalidate) {
        this.InvalidateAll("Change.Entity." + kind.ToString());
        return;
      }

      // Patch
      if (entity == null) {
        return;
      }

      if (_Options.KeySelector == null) {
        //DevLogger.LogTrace(0, 99999, "Patch skipped: KeySelector is missing.");
        this.InvalidateAll("PatchFallback.MissingKeySelector");
        return;
      }

      TKey key = _Options.KeySelector(entity);

      _EntityByKeyCache[key] = entity;
      this.PatchQueryCachesByKey(key, entity, null, false);
    }

    private void ProcessChangeFromFields(Dictionary<string, object> fields, ChangeKind kind) {
      if (_Options.ChangeProcessing == ChangeProcessing.Decoupled) {
        return;
      }

      if (_Options.ChangeProcessing == ChangeProcessing.Invalidate) {
        this.InvalidateAll("Change.Fields." + kind.ToString());
        return;
      }

      // Patch
      if (fields == null) {
        return;
      }

      TKey key;
      if (!this.TryExtractKeyFromFields(fields, out key)) {
        ///DevLogger.LogTrace(0, 99999, "Patch skipped: key cannot be extracted from field dictionary.");
        this.InvalidateAll("PatchFallback.MissingKeyInFields");
        return;
      }

      _FieldsByKeyCache[key] = fields;
      this.PatchQueryCachesByKey(key, null, fields, false);
    }

    private void ProcessDelete(TKey[] deletedKeys) {
      if (_Options.ChangeProcessing == ChangeProcessing.Decoupled) {
        return;
      }

      if (_Options.ChangeProcessing == ChangeProcessing.Invalidate) {
        this.InvalidateAll("Delete.Invalidate");
        return;
      }

      // Patch delete
      if (deletedKeys == null) {
        return;
      }

      int i;
      for (i = 0; i < deletedKeys.Length; i++) {
        TKey key = deletedKeys[i];
        TEntity removedEntity;
        EntityRef<TKey> removedRef;
        Dictionary<string, object> removedFields;

        _EntityByKeyCache.TryRemove(key, out removedEntity);
        _RefByKeyCache.TryRemove(key, out removedRef);
        _FieldsByKeyCache.TryRemove(key, out removedFields);

        this.PatchQueryCachesByKey(key, null, null, true);
      }
    }

    private void ProcessMassChange(TKey[] updatedKeys) {
      if (_Options.ChangeProcessing == ChangeProcessing.Decoupled) {
        return;
      }

      if (_Options.ChangeProcessing == ChangeProcessing.Invalidate) {
        this.InvalidateAll("Massupdate.Invalidate");
        return;
      }

      // Patch best-effort: we can only invalidate query caches or refresh by keys.
      // To avoid expensive reloads here, we patch by removing potentially stale entities and let next read re-load.
      if (updatedKeys == null) {
        return;
      }

      int i;
      for (i = 0; i < updatedKeys.Length; i++) {
        TKey key = updatedKeys[i];
        TEntity removedEntity;
        _EntityByKeyCache.TryRemove(key, out removedEntity);

        Dictionary<string, object> removedFields;
        _FieldsByKeyCache.TryRemove(key, out removedFields);

        EntityRef<TKey> removedRef;
        _RefByKeyCache.TryRemove(key, out removedRef);
      }

      this.InvalidateQueryCacheOnly("Massupdate.PatchInvalidateQueries");
    }

    private void ProcessKeyChange(TKey currentKey, TKey newKey) {
      if (_Options.ChangeProcessing == ChangeProcessing.Decoupled) {
        return;
      }

      if (_Options.ChangeProcessing == ChangeProcessing.Invalidate) {
        this.InvalidateAll("KeyUpdate.Invalidate");
        return;
      }

      TEntity entity;
      if (_EntityByKeyCache.TryRemove(currentKey, out entity)) {
        _EntityByKeyCache[newKey] = entity;
      }

      EntityRef<TKey> er;
      if (_RefByKeyCache.TryRemove(currentKey, out er)) {
        _RefByKeyCache[newKey] = er;
      }

      Dictionary<string, object> fields;
      if (_FieldsByKeyCache.TryRemove(currentKey, out fields)) {
        _FieldsByKeyCache[newKey] = fields;
      }

      this.InvalidateQueryCacheOnly("KeyUpdate.PatchInvalidateQueries");
    }

    private void InvalidateAll(string reason) {
      _EntityByKeyCache.Clear();
      _RefByKeyCache.Clear();
      _FieldsByKeyCache.Clear();

      _QueryCache.Clear();

      if ((_Options.PrefetchTrigger & PrefetchTrigger.OnInvalidate) == PrefetchTrigger.OnInvalidate) {
        this.QueueBackgroundRefreshAllQueriesBestEffort("InvalidateAll." + reason);
      }
    }

    private void InvalidateQueryCacheOnly(string reason) {
      _QueryCache.Clear();

      if ((_Options.PrefetchTrigger & PrefetchTrigger.OnInvalidate) == PrefetchTrigger.OnInvalidate) {
        this.QueueBackgroundRefreshAllQueriesBestEffort("InvalidateQueries." + reason);
      }
    }

    private void PatchQueryCachesByKey(TKey key, TEntity entityOrNull, Dictionary<string, object> fieldsOrNull, bool isDelete) {
      if (!this.IsQueryCacheEnabled()) {
        return;
      }

      // Scan & patch best-effort (bounded by MaxQueryCacheEntries).
      // If key extraction is impossible, we invalidate to avoid silent inconsistencies.

      string[] queryKeys = _QueryCache.Keys.ToArray();
      int i;
      for (i = 0; i < queryKeys.Length; i++) {
        string qk = queryKeys[i];

        CacheEntry entry;
        if (!_QueryCache.TryGetValue(qk, out entry)) {
          continue;
        }

        lock (entry.SyncRoot) {
          if (entry.Value == null) {
            continue;
          }

          object value = entry.Value;

          if (value is TEntity[]) {
            if (entityOrNull == null && isDelete) {
              entry.Value = this.RemoveEntityByKey((TEntity[])value, key);
            }
            else if (entityOrNull != null) {
              entry.Value = this.UpsertEntityByKey((TEntity[])value, key, entityOrNull);
            }
          }
          else if (value is EntityRef<TKey>[]) {
            if (isDelete) {
              entry.Value = this.RemoveEntityRefByKey((EntityRef<TKey>[])value, key);
            }
          }
          else if (value is Dictionary<string, object>[]) {
            if (isDelete) {
              entry.Value = this.RemoveFieldsByKey((Dictionary<string, object>[])value, key);
            }
            else if (fieldsOrNull != null) {
              entry.Value = this.UpsertFieldsByKey((Dictionary<string, object>[])value, key, fieldsOrNull);
            }
          }
          else {
            // Count results etc. => invalidate to avoid inconsistent totals.
            _QueryCache.TryRemove(qk, out entry);
          }
        }
      }
    }

    private TEntity[] RemoveEntityByKey(TEntity[] input, TKey key) {
      if (input == null || input.Length == 0) {
        return input;
      }
      if (_Options.KeySelector == null) {
        return input;
      }

      List<TEntity> list = new List<TEntity>(input.Length);
      int i;
      for (i = 0; i < input.Length; i++) {
        TEntity e = input[i];
        if (e == null) {
          continue;
        }
        TKey k = _Options.KeySelector(e);
        if (!EqualityComparer<TKey>.Default.Equals(k, key)) {
          list.Add(e);
        }
      }
      return list.ToArray();
    }

    private TEntity[] UpsertEntityByKey(TEntity[] input, TKey key, TEntity entity) {
      if (input == null) {
        input = new TEntity[0];
      }
      if (_Options.KeySelector == null) {
        return input;
      }

      int i;
      for (i = 0; i < input.Length; i++) {
        TEntity e = input[i];
        if (e == null) {
          continue;
        }
        TKey k = _Options.KeySelector(e);
        if (EqualityComparer<TKey>.Default.Equals(k, key)) {
          TEntity[] copy = (TEntity[])input.Clone();
          copy[i] = entity;
          return copy;
        }
      }

      // Not found: append (best-effort; may be wrong for paging, but avoids data loss).
      List<TEntity> list = new List<TEntity>(input.Length + 1);
      list.AddRange(input);
      list.Add(entity);
      return list.ToArray();
    }

    private EntityRef<TKey>[] RemoveEntityRefByKey(EntityRef<TKey>[] input, TKey key) {
      if (input == null || input.Length == 0) {
        return input;
      }

      List<EntityRef<TKey>> list = new List<EntityRef<TKey>>(input.Length);
      int i;
      for (i = 0; i < input.Length; i++) {
        EntityRef<TKey> er = input[i];
        if (er == null) {
          continue;
        }
        TKey k;
        if (!this.TryGetEntityRefKey(er, out k)) {
          // Cannot safely patch -> return unchanged
          return input;
        }
        if (!EqualityComparer<TKey>.Default.Equals(k, key)) {
          list.Add(er);
        }
      }
      return list.ToArray();
    }

    private Dictionary<string, object>[] RemoveFieldsByKey(Dictionary<string, object>[] input, TKey key) {
      if (input == null || input.Length == 0) {
        return input;
      }

      List<Dictionary<string, object>> list = new List<Dictionary<string, object>>(input.Length);
      int i;
      for (i = 0; i < input.Length; i++) {
        Dictionary<string, object> bag = input[i];
        if (bag == null) {
          continue;
        }
        TKey k;
        if (!this.TryExtractKeyFromFields(bag, out k)) {
          return input;
        }
        if (!EqualityComparer<TKey>.Default.Equals(k, key)) {
          list.Add(bag);
        }
      }
      return list.ToArray();
    }

    private Dictionary<string, object>[] UpsertFieldsByKey(Dictionary<string, object>[] input, TKey key, Dictionary<string, object> fields) {
      if (input == null) {
        input = new Dictionary<string, object>[0];
      }

      int i;
      for (i = 0; i < input.Length; i++) {
        Dictionary<string, object> bag = input[i];
        if (bag == null) {
          continue;
        }
        TKey k;
        if (!this.TryExtractKeyFromFields(bag, out k)) {
          return input;
        }
        if (EqualityComparer<TKey>.Default.Equals(k, key)) {
          Dictionary<string, object>[] copy = (Dictionary<string, object>[])input.Clone();
          copy[i] = fields;
          return copy;
        }
      }

      List<Dictionary<string, object>> list = new List<Dictionary<string, object>>(input.Length + 1);
      list.AddRange(input);
      list.Add(fields);
      return list.ToArray();
    }

    private Dictionary<TKey, TEntity> ToEntityMap(TEntity[] entities) {
      Dictionary<TKey, TEntity> map = new Dictionary<TKey, TEntity>();
      if (entities == null) {
        return map;
      }
      if (_Options.KeySelector == null) {
        return map;
      }

      int i;
      for (i = 0; i < entities.Length; i++) {
        TEntity e = entities[i];
        if (e == null) {
          continue;
        }
        TKey k = _Options.KeySelector(e);
        map[k] = e;
      }
      return map;
    }

    private Dictionary<TKey, EntityRef<TKey>> ToRefMap(EntityRef<TKey>[] refs) {
      Dictionary<TKey, EntityRef<TKey>> map = new Dictionary<TKey, EntityRef<TKey>>();
      if (refs == null) {
        return map;
      }

      int i;
      for (i = 0; i < refs.Length; i++) {
        EntityRef<TKey> er = refs[i];
        if (er == null) {
          continue;
        }
        TKey key;
        if (this.TryGetEntityRefKey(er, out key)) {
          map[key] = er;
        }
      }
      return map;
    }

    private Dictionary<TKey, Dictionary<string, object>> ToFieldsMap(Dictionary<string, object>[] fields) {
      Dictionary<TKey, Dictionary<string, object>> map = new Dictionary<TKey, Dictionary<string, object>>();
      if (fields == null) {
        return map;
      }

      int i;
      for (i = 0; i < fields.Length; i++) {
        Dictionary<string, object> bag = fields[i];
        if (bag == null) {
          continue;
        }
        TKey key;
        if (this.TryExtractKeyFromFields(bag, out key)) {
          map[key] = bag;
        }
      }
      return map;
    }

    private T[] FilterNull<T>(T[] arr) where T : class {
      if (arr == null) {
        return new T[0];
      }

      int count = 0;
      int i;
      for (i = 0; i < arr.Length; i++) {
        if (arr[i] != null) {
          count++;
        }
      }

      if (count == arr.Length) {
        return arr;
      }

      T[] filtered = new T[count];
      int idx = 0;
      for (i = 0; i < arr.Length; i++) {
        if (arr[i] != null) {
          filtered[idx] = arr[i];
          idx++;
        }
      }

      return filtered;
    }

    private Dictionary<string, object> FilterFieldDictionary(Dictionary<string, object> bag, string[] includedFieldNames) {
      if (bag == null) {
        return null;
      }
      if (includedFieldNames == null || includedFieldNames.Length == 0) {
        return bag;
      }

      Dictionary<string, object> filtered = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
      int i;
      for (i = 0; i < includedFieldNames.Length; i++) {
        string f = includedFieldNames[i];
        object v;
        if (bag.TryGetValue(f, out v)) {
          filtered[f] = v;
        }
      }

      // Keep key fields if configured to avoid data loss.
      if (_Options.KeyFieldNames != null) {
        int k;
        for (k = 0; k < _Options.KeyFieldNames.Length; k++) {
          string kn = _Options.KeyFieldNames[k];
          object kv;
          if (bag.TryGetValue(kn, out kv)) {
            filtered[kn] = kv;
          }
        }
      }

      return filtered;
    }

  }

}
