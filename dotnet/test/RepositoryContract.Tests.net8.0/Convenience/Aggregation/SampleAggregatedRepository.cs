using System;
using System.Collections.Generic;
using System.Data.Fuse.Convenience;
using System.Data.Fuse.Convenience.Aggregation;

namespace System.Data.Fuse.Convenience {

  /// <summary>
  /// Sample aggregated repository combining PrimaryEntity and SecondaryEntity
  /// into a synthetic SampleView with a composed natural key.
  /// </summary>
  public sealed class SampleAggregatedRepository : AggregatedRepositoryBase<
    SampleAggregatedEntity, //AGGREGATED ENTITY
    string,
    PrimaryEntity, //PRIMARY INNER ENTITY
    int,
    SecondaryEntity, //SECONDARY INNER ENTITY
    int
  > {

    private readonly IRepositoryUpdateAdapter _PrimaryUpdateAdapter;
    private readonly IRepositoryUpdateAdapter _SecondaryUpdateAdapter;
    private readonly IRepository<PrimaryEntity, int> _PrimaryRepository;
    private readonly IRepository<SecondaryEntity, int> _SecondaryRepository;

    public SampleAggregatedRepository(
      IRepository<PrimaryEntity, int> primaryRepository,
      IRepository<SecondaryEntity, int> secondaryRepository,
      PredicateRoutingMap routing,
      int batchSize
    ) : base(
        "SampleAggregatedRepository",
        primaryRepository,
        secondaryRepository,
        routing,
        batchSize
      ) {

      if (primaryRepository == null) {
        throw new ArgumentNullException(nameof(primaryRepository));
      }
      if (secondaryRepository == null) {
        throw new ArgumentNullException(nameof(secondaryRepository));
      }

      _PrimaryRepository = primaryRepository;
      _SecondaryRepository = secondaryRepository;
      _PrimaryUpdateAdapter = new RepositoryUpdateAdapter<PrimaryEntity, int>(primaryRepository);
      _SecondaryUpdateAdapter = new RepositoryUpdateAdapter<SecondaryEntity, int>(secondaryRepository);

    }

    // -------------------------------------------------
    // Key handling
    // -------------------------------------------------

    protected override string GetKeyForAggregation(SampleAggregatedEntity aggEntity) {
      return $"P:{aggEntity.PrimaryId}|S:{aggEntity.SecondaryId}";
    }

    protected override SampleAggregatedEntity[] GetEntitiesByAggKey(string[] viewKeys) {

      if (viewKeys == null || viewKeys.Length == 0) {
        return new SampleAggregatedEntity[0];
      }

      List<int> primaryIds = new List<int>();

      int i = 0;
      while (i < viewKeys.Length) {
        string key = viewKeys[i];
        if (!string.IsNullOrEmpty(key)) {
          int p = key.IndexOf("P:", StringComparison.OrdinalIgnoreCase);
          int s = key.IndexOf("|S:", StringComparison.OrdinalIgnoreCase);
          if (p >= 0 && s > p) {
            string idPart = key.Substring(p + 2, s - (p + 2));
            int parsed;
            if (int.TryParse(idPart, out parsed)) {
              primaryIds.Add(parsed);
            }
          }
        }
        i++;
      }

      PrimaryEntity[] primary = _PrimaryRepository.GetEntitiesByKey(primaryIds.ToArray());

      Dictionary<int, SecondaryEntity> secondary = this.LoadSecondaryLookup(primary);

      return this.MapToAggregatedEntities(primary, secondary);
    }

    // -------------------------------------------------
    // Mapping & loading
    // -------------------------------------------------

    protected override int[] ExtractSecondaryKeys(PrimaryEntity[] primaryEntities) {
      HashSet<int> keys = new HashSet<int>();

      int i = 0;
      while (i < primaryEntities.Length) {
        keys.Add(primaryEntities[i].SecondaryId);
        i++;
      }

      int[] result = new int[keys.Count];
      keys.CopyTo(result);
      return result;
    }

    protected override int GetSecondaryKey(SecondaryEntity entity) {
      return entity.Id;
    }

    protected override SampleAggregatedEntity[] MapToAggregatedEntities(
      PrimaryEntity[] primaryEntities,
      Dictionary<int, SecondaryEntity> secondaryByKey) {

      SampleAggregatedEntity[] result = new SampleAggregatedEntity[primaryEntities.Length];

      int i = 0;
      while (i < primaryEntities.Length) {
        PrimaryEntity p = primaryEntities[i];

        SecondaryEntity s;
        secondaryByKey.TryGetValue(p.SecondaryId, out s);

        SampleAggregatedEntity view = new SampleAggregatedEntity();
        view.PrimaryId = p.Id;
        view.SecondaryId = p.SecondaryId;
        view.PrimaryNumber = p.PrimaryNumber;
        view.PrimaryText = p.PrimaryText;

        if (s != null) {
          view.SecondaryNumber = s.SecondaryNumber;
          view.SecondaryText = s.SecondaryText;
        }

        view.AlloverKey = this.GetKeyForAggregation(view);
        result[i] = view;
        i++;
      }

      return result;
    }

    protected override bool EvaluateOnAggregatedEntity(SampleAggregatedEntity view, ExpressionTree filter) {
      return LinqEvaluator.Matches(view, filter);
    }

    // -------------------------------------------------
    // Updates (bidirectional, explicit)
    // -------------------------------------------------

    protected override SourceUpdatePlan BuildSourceUpdatePlan(SampleAggregatedEntity entity) {

      if (entity == null) {
        throw new ArgumentNullException(nameof(entity));
      }

      List<SourceUpdate> updates = new List<SourceUpdate>();

      // ---- Primary update ----
      Dictionary<string, object> primaryFields =
        new Dictionary<string, object>();

      primaryFields["Id"] = entity.PrimaryId;
      primaryFields["SecondaryId"] = entity.SecondaryId;
      primaryFields["PrimaryNumber"] = entity.PrimaryNumber;
      primaryFields["PrimaryText"] = entity.PrimaryText;

      SourceUpdate primaryUpdate = new SourceUpdate();
      primaryUpdate.Repository = this._PrimaryUpdateAdapter;
      primaryUpdate.Fields = primaryFields;
      updates.Add(primaryUpdate);

      // ---- Secondary update ----
      Dictionary<string, object> secondaryFields =
        new Dictionary<string, object>();

      secondaryFields["Id"] = entity.SecondaryId;
      secondaryFields["SecondaryNumber"] = entity.SecondaryNumber;
      secondaryFields["SecondaryText"] = entity.SecondaryText;

      SourceUpdate secondaryUpdate = new SourceUpdate();
      secondaryUpdate.Repository = this._SecondaryUpdateAdapter;
      secondaryUpdate.Fields = secondaryFields;
      updates.Add(secondaryUpdate);

      SourceUpdatePlan plan = new SourceUpdatePlan();
      plan.Updates = updates.ToArray();
      return plan;
    }

    // -------------------------------------------------
    // Field helpers
    // -------------------------------------------------

    protected override SampleAggregatedEntity CreateAggregatedEntityInstance() {
      return new SampleAggregatedEntity();
    }

    protected override void ApplyFieldDictionaryToAggregatedEntitiy(
      SampleAggregatedEntity entity,
      Dictionary<string, object> fields
    ) {

      if (fields == null) {
        return;
      }

      foreach (var kv in fields) {
        var prop = typeof(SampleAggregatedEntity).GetProperty(kv.Key);
        if (prop != null) {
          prop.SetValue(entity, kv.Value);
        }
      }
    }

    protected override void WriteAggregatedEntityToFieldDictionary(
      SampleAggregatedEntity entity,
      Dictionary<string, object> fields) {

      foreach (var prop in typeof(SampleAggregatedEntity).GetProperties()) {
        fields[prop.Name] = prop.GetValue(entity);
      }
    }

  }

}
