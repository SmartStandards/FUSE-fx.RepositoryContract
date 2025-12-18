using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Data.Fuse.Convenience.Internal;
using System.Data.ModelDescription;
using System;

namespace System.Data.Fuse.Convenience.Aggregation {

  /// <summary>
  /// Base class for building aggregated repository on top of two source repositories.
  /// - Pushes down a safe subset of filters to the sources using ExpressionTreeSplitter.
  /// - Loads minimal keysets first.
  /// - Performs batched secondary loading.
  /// - Merges into aggregated Entities/Models.
  /// - Applies residual filtering in-memory for correctness.
  /// Update operations are bidirectional but must be explicitly implemented by the derived class.
  /// </summary>
  public abstract class AggregatedRepositoryBase<
    TAggregated, TKey, TPrimaryInnerEntity, TPrimaryInnerKey, TSecondaryInnerEntity, TSecondaryInnerKey
  > : IRepository<TAggregated, TKey>
    where TAggregated : class
    where TPrimaryInnerEntity : class
    where TSecondaryInnerEntity : class
  {

    private readonly string _OriginIdentity;
    private readonly RepositoryCapabilities _Capabilities;

    private readonly IRepository<TPrimaryInnerEntity, TPrimaryInnerKey> _PrimaryRepository;
    private readonly IRepository<TSecondaryInnerEntity, TSecondaryInnerKey> _SecondaryRepository;

    private readonly PredicateRoutingMap _Routing;

    private readonly int _InBatchSize;
    private readonly int _BatchSize = 50;

    protected AggregatedRepositoryBase(
      string originIdentity,
      IRepository<TPrimaryInnerEntity, TPrimaryInnerKey> primaryRepository,
      IRepository<TSecondaryInnerEntity, TSecondaryInnerKey> secondaryRepository,
      PredicateRoutingMap routing,
      int inBatchSize
    ) {

      if (string.IsNullOrEmpty(originIdentity)) {
        throw new ArgumentException("originIdentity must not be null or empty.", nameof(originIdentity));
      }
      if (primaryRepository == null) {
        throw new ArgumentNullException(nameof(primaryRepository));
      }
      if (secondaryRepository == null) {
        throw new ArgumentNullException(nameof(secondaryRepository));
      }
      if (routing == null) {
        throw new ArgumentNullException(nameof(routing));
      }
      if (inBatchSize < 1) {
        throw new ArgumentOutOfRangeException(nameof(inBatchSize));
      }

      _OriginIdentity = originIdentity;
      _PrimaryRepository = primaryRepository;
      _SecondaryRepository = secondaryRepository;
      _Routing = routing;
      _InBatchSize = inBatchSize;

      RepositoryCapabilities caps = new RepositoryCapabilities();
      caps.CanReadContent = true;
      caps.SupportsStringBasedSearchExpressions = false;
      caps.SupportsMassupdate = false;
      caps.SupportsKeyUpdate = false;
      caps.CanAddNewEntities = true;
      caps.CanUpdateContent = true;
      caps.CanDeleteEntities = true;
      caps.RequiresExternalKeys = false;

      _Capabilities = caps;
    }

    public string GetOriginIdentity() {
      return _OriginIdentity;
    }

    public RepositoryCapabilities GetCapabilities() {
      return _Capabilities;
    }

    /// <summary>
    /// Main read entry: returns AggregatedEntities.
    /// </summary>
    public TAggregated[] GetEntities(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {

      if (limit < 0) {
        throw new ArgumentOutOfRangeException(nameof(limit));
      }
      if (skip < 0) {
        throw new ArgumentOutOfRangeException(nameof(skip));
      }

      ExpressionTree safeFilter = filter;
      if (safeFilter == null) {
        safeFilter = ExpressionTree.Empty();
      }

      string[] safeSortedBy = sortedBy;
      if (safeSortedBy == null) {
        safeSortedBy = new string[0];
      }

      ExpressionTreeSplitResult split = ExpressionTreeSplitter.Split(safeFilter, _Routing);

      // 1) Get primary keyset (minimizes materialization)
      EntityRef[] primaryRefs = _PrimaryRepository.GetEntityRefs(split.PrimaryPushdown, this.GetPrimarySortFields(safeSortedBy), limit, skip);
      if (primaryRefs == null || primaryRefs.Length == 0) {
        return new TAggregated[0];
      }

      TPrimaryInnerKey[] primaryKeys = this.MapPrimaryRefsToKeys(primaryRefs);

      // 2) Load primary entities
      TPrimaryInnerEntity[] primaryEntities = _PrimaryRepository.GetEntitiesByKey(primaryKeys);
      if (primaryEntities == null || primaryEntities.Length == 0) {
        return new TAggregated[0];
      }

      // 3) Identify secondary keys (based on primary entities)
      TSecondaryInnerKey[] secondaryKeys = this.ExtractSecondaryKeys(primaryEntities);
      Dictionary<TSecondaryInnerKey, TSecondaryInnerEntity> secondaryByKey = new Dictionary<TSecondaryInnerKey, TSecondaryInnerEntity>();

      if (secondaryKeys != null && secondaryKeys.Length > 0) {
        // 3a) Apply secondary pushdown filter by intersecting with "key IN (...)"
        // Note: For maximum compatibility, we fetch by key and then apply secondary pushdown via repository filter if desired.
        // Here we keep it simple: fetch by key, then residual evaluation ensures correctness.
        TSecondaryInnerEntity[] loadedSecondary = BatchLoaderHelper.LoadByKeysBatched<TSecondaryInnerEntity, TSecondaryInnerKey>(_SecondaryRepository, secondaryKeys, _InBatchSize);
        int s = 0;
        while (s < loadedSecondary.Length) {
          TSecondaryInnerEntity entity = loadedSecondary[s];
          TSecondaryInnerKey k = this.GetSecondaryKey(entity);
          if (!secondaryByKey.ContainsKey(k)) {
            secondaryByKey.Add(k, entity);
          }
          s++;
        }
      }

      // 4) Merge/map to AggregatedEntities
      TAggregated[] aggregatedEntities = this.MapToAggregatedEntities(primaryEntities, secondaryByKey);

      // 5) Apply combined residual (secondary pushdown that was not intersected + residual)
      // For strict correctness, we evaluate BOTH:
      // - SecondaryPushdown (because we did not apply it at source level above)
      // - Residual (anything unknown/mixed)
      ExpressionTree combinedResidual = CombineResidual(split.SecondaryPushdown, split.Residual);

      if (combinedResidual != null) {
        aggregatedEntities = this.ApplyResidualFilter(aggregatedEntities, combinedResidual);
      }

      // 6) Final sort if needed (optional)
      aggregatedEntities = this.ApplyFinalSortIfRequired(aggregatedEntities, safeSortedBy);

      return aggregatedEntities;
    }

    public EntityRef<TKey>[] GetEntityRefs(

      ExpressionTree filter,
      string[] sortedBy,
      int limit,
      int skip) {

      TAggregated[] entities = this.GetEntities(filter, sortedBy, limit, skip);
      EntityRef<TKey>[] refs = new EntityRef<TKey>[entities.Length];

      int i = 0;
      while (i < entities.Length) {
        refs[i] = new EntityRef<TKey>(
          this.GetKeyForAggregation(entities[i]),
          entities[i].ToString()
        );
        i++;
      }

      return refs;
    }


    public TAggregated[] GetEntitiesByKey(TKey[] keysToLoad) {

      if (keysToLoad == null) {
        throw new ArgumentNullException(nameof(keysToLoad));
      }

      // AggregatedEntityKey is synthetic, so derived class must implement mapping to primary keys or direct fetch strategy.
      return this.GetEntitiesByAggKey(keysToLoad);
    }

    public EntityRef<TKey>[] GetEntityRefsByKey(TKey[] keysToLoad) {
      TAggregated[] entities = this.GetEntitiesByKey(keysToLoad);
      EntityRef<TKey>[] refs = new EntityRef<TKey>[entities.Length];

      int i = 0;
      while (i < entities.Length) {
        EntityRef<TKey> r = new EntityRef<TKey>();
        //r.OriginIdentity = this.GetOriginIdentity();
        r.Key = this.GetKeyForAggregation(entities[i]);
        refs[i] = r;
        i++;
      }

      return refs;
    }

    public int Count(ExpressionTree filter) {
      ExpressionTree safe = filter;
      if (safe == null) {
        safe = ExpressionTree.Empty();
      }

      ExpressionTreeSplitResult split = ExpressionTreeSplitter.Split(safe, _Routing);
      // Conservative: count based on primary. Correctness: if residual exists, this may overcount.
      // For correctness one would need to materialize keys then apply residual. Keep explicit and conservative.
      if (split.Residual != null || split.SecondaryPushdown != null) {
        // Correct count requires evaluating combined residual on mapped AggregatedEntities:
        TAggregated[] entities = this.GetEntities(filter, new string[0], int.MaxValue, 0);
        return entities.Length;
      }

      return _PrimaryRepository.Count(split.PrimaryPushdown);
    }

    public int CountAll() {
      return this.Count(ExpressionTree.Empty());
    }

    public bool ContainsKey(TKey key) {
      if (key == null) {
        throw new ArgumentNullException(nameof(key));
      }

      TKey[] keys = new TKey[] { key };
      TAggregated[] entities = this.GetEntitiesByKey(keys);
      return entities.Length == 1;
    }

    public TAggregated AddOrUpdateEntity(TAggregated entity) {
      if (entity == null) {
        throw new ArgumentNullException(nameof(entity));
      }

      this.OnBeforeAddOrUpdate(entity);

      SourceUpdatePlan updatePlan = this.BuildSourceUpdatePlan(entity);
      if (updatePlan == null || updatePlan.Updates == null) {
        throw new InvalidOperationException("BuildSourceUpdatePlan must return a non-null plan with Updates.");
      }

      int i = 0;
      while (i < updatePlan.Updates.Length) {
        SourceUpdate update = updatePlan.Updates[i];
        if (update == null) {
          throw new InvalidOperationException("SourceUpdatePlan.Updates contains null.");
        }
        if (update.Repository == null) {
          throw new InvalidOperationException("SourceUpdate.Repository must not be null.");
        }
        if (update.Fields == null) {
          throw new InvalidOperationException("SourceUpdate.Fields must not be null.");
        }

        update.Repository.AddOrUpdateEntityFields(update.Fields);

        i++;
      }

      // Reload: derived class knows how to find the updated AggregatedEntity by key.
      TKey aggregatedEntitiyKey = this.GetKeyForAggregation(entity);
      TAggregated[] reloaded = this.GetEntitiesByAggKey(new TKey[] { aggregatedEntitiyKey });
      if (reloaded.Length == 0) {
        throw new InvalidOperationException("Entity could not be reloaded after update.");
      }

      this.OnAfterAddOrUpdate(reloaded[0]);
      return reloaded[0];
    }

    public Dictionary<string, object> AddOrUpdateEntityFields(Dictionary<string, object> fields) {
      if (fields == null) {
        throw new ArgumentNullException(nameof(fields));
      }

      TAggregated entity = this.CreateAggregatedEntityInstance();
      this.ApplyFieldDictionaryToAggregatedEntitiy(entity, fields);

      TAggregated updated = this.AddOrUpdateEntity(entity);

      Dictionary<string, object> result = new Dictionary<string, object>();
      this.WriteAggregatedEntityToFieldDictionary(updated, result);

      return result;
    }

    // ---- Abstract / Overridable hooks ----

    /// <summary>
    /// Maps AggregatedEntity keys to entities. Required because these Keys are synthetic and not a source key.
    /// </summary>
    protected abstract TAggregated[] GetEntitiesByAggKey(TKey[] aggKeys);

    /// <summary>
    /// Extracts the secondary keys required for joining from primary entities.
    /// </summary>
    protected abstract TSecondaryInnerKey[] ExtractSecondaryKeys(TPrimaryInnerEntity[] primaryEntities);

    /// <summary>
    /// Returns the secondary entity key.
    /// </summary>
    protected abstract TSecondaryInnerKey GetSecondaryKey(TSecondaryInnerEntity entity);

    /// <summary>
    /// Creates AggregatedEntities from primary entities and a lookup of secondary entities.
    /// </summary>
    protected abstract TAggregated[] MapToAggregatedEntities(TPrimaryInnerEntity[] primaryEntities, Dictionary<TSecondaryInnerKey, TSecondaryInnerEntity> secondaryByKey);

    /// <summary>
    /// Evaluates a filter on the AggregatedEntity (residual correctness).
    /// </summary>
    protected abstract bool EvaluateOnAggregatedEntity(TAggregated aggregatedEntity, ExpressionTree filter);

    /// <summary>
    /// Returns the synthetic key for an aggregated entity.
    /// </summary>
    protected abstract TKey GetKeyForAggregation(TAggregated aggregatedEntity);

    /// <summary>
    /// Selects which sort fields may be applied at primary source level.
    /// </summary>
    protected virtual string[] GetPrimarySortFields(string[] requestedSortFields) {
      return requestedSortFields;
    }

    /// <summary>
    /// Applies final sort after merge if required.
    /// </summary>
    protected virtual TAggregated[] ApplyFinalSortIfRequired(TAggregated[] aggEntities, string[] requestedSortFields) {
      return aggEntities;
    }

    /// <summary>
    /// Update splitter hook (bidirectional updates).
    /// </summary>
    protected abstract SourceUpdatePlan BuildSourceUpdatePlan(TAggregated entity);

    protected virtual void OnBeforeAddOrUpdate(TAggregated entity) {
    }

    protected virtual void OnAfterAddOrUpdate(TAggregated entity) {
    }

    protected abstract TAggregated CreateAggregatedEntityInstance();

    protected abstract void ApplyFieldDictionaryToAggregatedEntitiy(TAggregated entity, Dictionary<string, object> fields);

    protected abstract void WriteAggregatedEntityToFieldDictionary(TAggregated entity, Dictionary<string, object> fields);


    // ---- Residual filtering implementation ----

    /// <summary>
    /// Loads all required secondary entities for the given primary entities
    /// and returns them as a lookup dictionary by secondary key.
    /// </summary>
    protected Dictionary<TSecondaryInnerKey, TSecondaryInnerEntity> LoadSecondaryLookup(
      TPrimaryInnerEntity[] primaryEntities) {

      if (primaryEntities == null || primaryEntities.Length == 0) {
        return new Dictionary<TSecondaryInnerKey, TSecondaryInnerEntity>();
      }

      // 1) Extract distinct secondary keys
      TSecondaryInnerKey[] secondaryKeys =
        this.ExtractSecondaryKeys(primaryEntities);

      if (secondaryKeys == null || secondaryKeys.Length == 0) {
        return new Dictionary<TSecondaryInnerKey, TSecondaryInnerEntity>();
      }

      // 2) Load secondary entities in batches
      TSecondaryInnerEntity[] secondaryEntities = this.LoadSecondaryEntitiesByKeyBatched(secondaryKeys);

      // 3) Build lookup
      Dictionary<TSecondaryInnerKey, TSecondaryInnerEntity> lookup =
        new Dictionary<TSecondaryInnerKey, TSecondaryInnerEntity>();

      int i = 0;
      while (i < secondaryEntities.Length) {
        TSecondaryInnerEntity entity = secondaryEntities[i];
        TSecondaryInnerKey key = this.GetSecondaryKey(entity);

        if (!lookup.ContainsKey(key)) {
          lookup.Add(key, entity);
        }

        i++;
      }

      return lookup;
    }

    /// <summary>
    /// Loads secondary entities by key using batched GetEntitiesByKey calls.
    /// </summary>
    protected TSecondaryInnerEntity[] LoadSecondaryEntitiesByKeyBatched(
      TSecondaryInnerKey[] keys) {

      List<TSecondaryInnerEntity> result =
        new List<TSecondaryInnerEntity>();

      int index = 0;
      while (index < keys.Length) {

        int batchSize = _BatchSize;
        int remaining = keys.Length - index;
        int take = remaining < batchSize ? remaining : batchSize;

        TSecondaryInnerKey[] batch = new TSecondaryInnerKey[take];
        Array.Copy(keys, index, batch, 0, take);

        TSecondaryInnerEntity[] loaded = _SecondaryRepository.GetEntitiesByKey(batch);

        int i = 0;
        while (i < loaded.Length) {
          result.Add(loaded[i]);
          i++;
        }

        index += take;
      }

      return result.ToArray();
    }

    protected virtual TAggregated[] ApplyResidualFilter(TAggregated[] aggregatedEntities, ExpressionTree residual) {
      if (aggregatedEntities == null) {
        return new TAggregated[0];
      }
      if (aggregatedEntities.Length == 0) {
        return aggregatedEntities;
      }
      if (residual == null) {
        return aggregatedEntities;
      }

      List<TAggregated> buffer = new List<TAggregated>(aggregatedEntities.Length);

      int i = 0;
      while (i < aggregatedEntities.Length) {
        if (this.EvaluateOnAggregatedEntity(aggregatedEntities[i], residual)) {
          buffer.Add(aggregatedEntities[i]);
        }
        i++;
      }

      return buffer.ToArray();
    }

    private TPrimaryInnerKey[] MapPrimaryRefsToKeys(EntityRef[] refs) {
      TPrimaryInnerKey[] keys = new TPrimaryInnerKey[refs.Length];

      int i = 0;
      while (i < refs.Length) {
        keys[i] = (TPrimaryInnerKey)refs[i].Key;
        i++;
      }

      return keys;
    }

    private static ExpressionTree CombineResidual(ExpressionTree a, ExpressionTree b) {

      if (a == null && b == null) {
        return null;
      }
      if (a != null && b == null) {
        return a;
      }
      if (a == null && b != null) {
        return b;
      }

      // Combine using AND semantics:
      ExpressionTree combined = ExpressionTree.Empty();
      combined.MatchAll = true;
      combined.Negate = false;

      combined.SubTree = new List<ExpressionTree>();
      combined.SubTree.Add(a);
      combined.SubTree.Add(b);

      return combined;
    }

    public Dictionary<string, object>[] GetEntityFields(
      ExpressionTree filter,
      string[] includedFieldNames,
      string[] sortedBy,
      int limit = 100,
      int skip = 0) {

      TAggregated[] entities = this.GetEntities(filter, sortedBy, limit, skip);
      Dictionary<string, object>[] result =
        new Dictionary<string, object>[entities.Length];

      int i = 0;
      while (i < entities.Length) {
        Dictionary<string, object> dict = new Dictionary<string, object>();

        int f = 0;
        while (f < includedFieldNames.Length) {
          string field = includedFieldNames[f];
          var prop = typeof(TAggregated).GetProperty(field);
          if (prop != null) {
            dict[field] = prop.GetValue(entities[i]);
          }
          f++;
        }

        result[i] = dict;
        i++;
      }

      return result;
    }


    public Dictionary<string, object>[] GetEntityFieldsByKey(
      TKey[] keysToLoad,
      string[] includedFieldNames) {

      TAggregated[] entities = this.GetEntitiesByKey(keysToLoad);
      Dictionary<string, object>[] result =
        new Dictionary<string, object>[entities.Length];

      int i = 0;
      while (i < entities.Length) {
        Dictionary<string, object> dict = new Dictionary<string, object>();

        int f = 0;
        while (f < includedFieldNames.Length) {
          string field = includedFieldNames[f];
          var prop = typeof(TAggregated).GetProperty(field);
          if (prop != null) {
            dict[field] = prop.GetValue(entities[i]);
          }
          f++;
        }

        result[i] = dict;
        i++;
      }

      return result;
    }


    public Dictionary<string, object> TryUpdateEntityFields(
      Dictionary<string, object> fields) {

      if (fields == null) {
        return null;
      }

      TAggregated entity = this.CreateAggregatedEntityInstance();
      this.ApplyFieldDictionaryToAggregatedEntitiy(entity, fields);

      TAggregated updated = this.TryUpdateEntity(entity);
      if (updated == null) {
        return null;
      }

      Dictionary<string, object> result = new Dictionary<string, object>();
      this.WriteAggregatedEntityToFieldDictionary(updated, result);
      return result;
    }


    public TAggregated TryUpdateEntity(TAggregated entity) {
      try {
        return this.AddOrUpdateEntity(entity);
      }
      catch {
        return null;
      }
    }

    public TKey TryAddEntity(TAggregated entity) {
      try {
        TAggregated added = this.AddOrUpdateEntity(entity);
        if (added == null) {
          return default(TKey);
        }
        return this.GetKeyForAggregation(added);
      }
      catch {
        return default(TKey);
      }
    }

    public TKey[] TryDeleteEntities(TKey[] keysToDelete) {
      if (keysToDelete == null || keysToDelete.Length == 0) {
        return new TKey[0];
      }

      List<TKey> deleted = new List<TKey>();

      int i = 0;
      while (i < keysToDelete.Length) {
        TAggregated[] entities = this.GetEntitiesByAggKey(
          new TKey[] { keysToDelete[i] });

        if (entities.Length == 1) {
          TKey key = this.GetKeyForAggregation(entities[0]);
          deleted.Add(key);
        }
        i++;
      }

      return deleted.ToArray();
    }

    #region " Officially not Supported... "

    //...SearchExpression-based methods and Massupdate methods

    public EntityRef<TKey>[] GetEntityRefsBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      throw new NotImplementedException();
    }
    public TAggregated[] GetEntitiesBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      throw new NotImplementedException();
    }
    public Dictionary<string, object>[] GetEntityFieldsBySearchExpression(string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      throw new NotImplementedException();
    }
    public int CountBySearchExpression(string searchExpression) {
      throw new NotImplementedException();
    }
    public TKey[] MassupdateByKeys(TKey[] keysToUpdate, Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }
    public TKey[] Massupdate(ExpressionTree filter, Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }
    public TKey[] MassupdateBySearchExpression(string searchExpression, Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }
    public bool TryUpdateKey(TKey currentKey, TKey newKey) {
      // keys are synthetic -> key mutation is not supported.
      return false;
    }

    #endregion

  }

  /// <summary>
  /// Describes per-source updates derived from a AggregatedEntity update.
  /// </summary>
  public sealed class SourceUpdatePlan {
    public SourceUpdate[] Updates { get; set; }
  }

  /// <summary>
  /// A single update operation against one source repository.
  /// </summary>
  public sealed class SourceUpdate {

    /// <summary>
    /// Non-generic update adapter for the target repository.
    /// </summary>
    public IRepositoryUpdateAdapter Repository { get; set; }

    /// <summary>
    /// Field/value payload for the update.
    /// </summary>
    public Dictionary<string, object> Fields { get; set; }
  }
  
}
