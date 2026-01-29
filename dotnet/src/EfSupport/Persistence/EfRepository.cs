using System.Collections.Generic;
using System.Data.Fuse.Convenience;
#if !NETCOREAPP
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
#endif
using System.Data.ModelDescription;
using System.Data.ModelDescription.Convenience;
using System.Linq;
using System.Reflection;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Data.Fuse.Ef.InstanceManagement;
using System.Data.Fuse.LinqSupport;
#if !NETCOREAPP
using System.Data.Fuse.WcfSupport;
#endif

namespace System.Data.Fuse.Ef {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public class EfRepository<TEntity, TKey>
    : IRepository<TEntity, TKey>
#if !NETCOREAPP
    , IWcfRepository<TEntity, TKey>
#endif
    where TEntity : class {

    #region " SchemaRoot/Metadata Caching "

    private static SchemaRoot _SchemaRoot = null;
    public SchemaRoot GetSchemaRoot() {
      //TODO: wollen wir so eine funktionalität überhaupt hier,
      //oder verzichten wir lifer auf den konstruktor ohne schema
      if (_SchemaRoot == null) {
        Type dbContextType = null;
        _ContextInstanceProvider.VisitCurrentDbContext(
            (dbContext) => dbContextType = dbContext.GetType()
         );
        _SchemaRoot = SchemaCache.GetSchemaRootForContext(dbContextType);
      }
      return _SchemaRoot;
    }

    #endregion

    private readonly Func<TEntity, TKey> _KeyExtractor;

    private IDbContextInstanceProvider _ContextInstanceProvider;
    public IDbContextInstanceProvider ContextInstanceProvider {
      get {
        return _ContextInstanceProvider;
      }
    }

    public EfRepository(IDbContextInstanceProvider contextInstanceProvider) {
      _ContextInstanceProvider = contextInstanceProvider;
      _KeyExtractor = ConversionHelper.GetKeyExtractor<TEntity, TKey>(this.GetSchemaRoot());
    }

    [Obsolete("This overload is unsave, because it doesnt care about lifetime management of the dbcontext!")]
    public EfRepository(DbContext dbContext) {
      _ContextInstanceProvider = new LongLivingDbContextInstanceProvider(dbContext);
      _KeyExtractor = ConversionHelper.GetKeyExtractor<TEntity, TKey>(this.GetSchemaRoot());
    }

    public EfRepository(IDbContextInstanceProvider contextInstanceProvider, SchemaRoot schemaRoot) {
      _ContextInstanceProvider = contextInstanceProvider;
      _SchemaRoot = schemaRoot;
      _KeyExtractor = ConversionHelper.GetKeyExtractor<TEntity, TKey>(_SchemaRoot);
    }

    [Obsolete("This overload is unsave, because it doesnt care about lifetime management of the dbcontext!")]
    public EfRepository(DbContext dbContext, SchemaRoot schemaRoot) {
      _ContextInstanceProvider = new LongLivingDbContextInstanceProvider(dbContext);
      _SchemaRoot = schemaRoot;
      _KeyExtractor = ConversionHelper.GetKeyExtractor<TEntity, TKey>(_SchemaRoot);
    }

    private static List<List<PropertyInfo>> _UniqueKeySets;
    private static List<PropertyInfo> _PrimaryKeySet;

    protected List<List<PropertyInfo>> Keysets {
      get {
        if (_UniqueKeySets == null) {
          _UniqueKeySets = InitUniqueKeySets();
        }
        return _UniqueKeySets;
      }
    }

    protected List<PropertyInfo> PrimaryKeySet {
      get {
        if (_PrimaryKeySet == null) {
          _PrimaryKeySet = InitPrimaeyKeySet();
        }
        return _PrimaryKeySet;
      }
    }

    protected List<List<PropertyInfo>> InitUniqueKeySets() {
      return GetSchemaRoot().GetUniqueKeysetsProperties(typeof(TEntity));
    }

    protected List<PropertyInfo> InitPrimaeyKeySet() {
      return GetSchemaRoot().GetPrimaryKeyProperties(typeof(TEntity)).ToList();
    }

    public TEntity AddOrUpdateEntity(TEntity entity) {
      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {

        object[] keySetValues = entity.GetValues(PrimaryKeySet);
        TEntity existingEntity = dbContext.Set<TEntity>().Find(keySetValues.ToArray());

        //AI
        // If no primary key is found, try to find the entity by unique key
        if (existingEntity == null) {
          foreach (var keyset in Keysets) {
            object[] keysetValues = entity.GetValues(keyset);
            existingEntity = dbContext.Set<TEntity>().Find(keysetValues);
            if (existingEntity != null) {
              break;
            }
          }
        }

        if (existingEntity == null) {
          // when adding a new entity, we have to unset the key fields that are identity fields
          this.UnsetIdentityFields(entity);

          dbContext.Set<TEntity>().Add(entity);
          dbContext.SaveChanges();
          return entity;
        } else {
          CopyFields(entity, existingEntity);
          dbContext.SaveChanges();
          return existingEntity;
        }

      });
    }

    private void UnsetIdentityFields(TEntity entity) {
      EntitySchema schema = GetSchemaRoot().GetSchema(typeof(TEntity).Name);
      foreach (var propertyInfo in typeof(TEntity).GetProperties()) {
        var fieldSchema = schema.Fields.FirstOrDefault(f => f.Name == propertyInfo.Name);
        if (fieldSchema != null && fieldSchema.DbGeneratedIdentity) {
          // Set the identity field to its default value
          if (propertyInfo.PropertyType.IsValueType) {
            propertyInfo.SetValue(entity, Activator.CreateInstance(propertyInfo.PropertyType));
          } else {
            propertyInfo.SetValue(entity, null);
          }
        }
      }
    }

    //AI - minor adjustments
    public Dictionary<string, object> AddOrUpdateEntityFields(
      Dictionary<string, object> fields
    ) {
      return _ContextInstanceProvider.VisitCurrentDbContext<Dictionary<string, object>>((dbContext) => {

        // Check for existing entity by primary key
        object[] primaryKeyValues = fields.TryGetValuesByFields(PrimaryKeySet);
        TEntity existingEntity = (primaryKeyValues == null)
          ? null : dbContext.Set<TEntity>().Find(primaryKeyValues);

        // If not found by primary key, check for existing entity by unique keysets
        if (existingEntity == null) {
          foreach (var keyset in Keysets) {
            object[] keysetValues = fields.GetValuesFromDictionary(keyset);
            existingEntity = (keysetValues == null)
              ? null : dbContext.Set<TEntity>().Find(keysetValues);
            if (existingEntity != null) {
              break;
            }
          }
        }

        if (existingEntity != null) {
          // If existing entity found, update it
          CopyFields2(fields, existingEntity);
        } else {
          // Create a new instance of TEntity
          TEntity entity = Activator.CreateInstance<TEntity>();
          CopyFields2(fields, entity);
          existingEntity = entity;
          // If no existing entity found, add new entity
          this.UnsetIdentityFields(entity);
          dbContext.Set<TEntity>().Add(entity);
        }

        dbContext.SaveChanges();

        // Convert the updated entity back to a dictionary and return it
        Dictionary<string, object> conflictingFields = new Dictionary<string, object>();
        foreach (var propertyInfo in typeof(TEntity).GetProperties()) {
          var updatedValue = propertyInfo.GetValue(existingEntity);
          if (
            !fields.TryGetValue(propertyInfo.Name, out var originalValue) ||
            !Equals(updatedValue, originalValue)
          ) {
            conflictingFields[propertyInfo.Name] = updatedValue;
          }
        }

        return conflictingFields;
      });
    }
    private void CopyFields(TEntity from, TEntity to) {
      EntitySchema schema = GetSchemaRoot().GetSchema(typeof(TEntity).Name);
      //HACK: viel zu teuer, immerwieder die properties zu laden
      foreach (PropertyInfo propertyInfo in typeof(TEntity).GetProperties()) {
        if (!schema.Fields.Any((f) => f.Name == propertyInfo.Name)) continue;
        propertyInfo.SetValue(to, propertyInfo.GetValue(from, null), null);
      }
    }
    private static void CopyFields2(Dictionary<string, object> fields, TEntity entity) {
      foreach (var field in fields) {
        //HACK: viel zu teuer, immerwieder die properties zu laden
        PropertyInfo propertyInfo = typeof(TEntity).GetProperty(field.Key);
        if (propertyInfo != null) {
          object fieldValue = field.Value;
#if NETCOREAPP
          if (fieldValue.GetType() == typeof(JsonElement)) {
            fieldValue = GetValue(propertyInfo, (JsonElement)fieldValue);
          }
#endif
          propertyInfo.SetValue(entity, fieldValue);
        }
      }
    }

#if NETCOREAPP
    private static object GetValue(PropertyInfo prop, JsonElement propertyValue) {
      if (prop.PropertyType == typeof(string)) {
        return propertyValue.GetString();
      } else if (prop.PropertyType == typeof(long)) {
        return propertyValue.GetInt64();
      } else if (prop.PropertyType == typeof(bool)) {
        return propertyValue.GetBoolean();
      } else if (prop.PropertyType == typeof(DateTime)) {
        return propertyValue.GetDateTime();
      } else if (prop.PropertyType == typeof(int)) {
        return propertyValue.GetInt32();
      } else if (prop.PropertyType == typeof(Guid)) {
        return propertyValue.GetGuid();
      } else if (prop.PropertyType == typeof(decimal)) {
        return propertyValue.GetDecimal();
      } else if (prop.PropertyType == typeof(float)) {
        return propertyValue.GetSingle();
      } else if (prop.PropertyType == typeof(uint)) {
        return propertyValue.GetUInt32();
      } else if (prop.PropertyType == typeof(UInt16)) {
        return propertyValue.GetUInt16();
      } else {
        return null;
      }
    }
#endif

    public bool ContainsKey(TKey key) {
      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {
        return (dbContext.Set<TEntity>().Find(key.GetKeyFieldValues()) != null);
      });
    }

    public int Count(ExpressionTree filter) {
      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {
        //TODO: Verwender bitte umbauen auf 'System.Data.Fuse.LinqSupport.ExpressionTreeMapper.BuildLinqExpressionFromTree'
        return dbContext.Set<TEntity>().Count(filter.CompileToDynamicLinq(GetSchemaRoot().GetSchema(typeof(TEntity).Name)));
      });
    }

    public int CountAll() {
      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {
        return dbContext.Set<TEntity>().Count();
      });
    }

    public int CountBySearchExpression(string searchExpression) {
      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {
        return dbContext.Set<TEntity>().Count(searchExpression);
      });
    }

    public RepositoryCapabilities GetCapabilities() {
      return RepositoryCapabilities.All;
    }

    public TEntity[] GetEntities(
      ExpressionTree filter, string[] sortedBy, int limit = 500, int skip = 0
    ) {
      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {

        IQueryable<TEntity> entities;
        if (filter == null) {
          entities = dbContext.Set<TEntity>().AsNoTracking();
        } else {
          //HACK: internal usage of System.Data.Fuse.LinqSupport
          entities = dbContext.Set<TEntity>().AsNoTracking().Where(filter.CompileToDynamicLinq(GetSchemaRoot().GetSchema(typeof(TEntity).Name)));
        }

        entities = entities.ApplySortingViaLinqDynamic(sortedBy);
        entities = entities.ApplyPaging(limit, skip);

        return entities.ToArray();
      });
    }

    /// <summary>
    /// Loads entities by their keys.
    /// WARNING: the returned array will contain NULL entries for non-existing entities!
    /// </summary>
    /// <param name="keysToLoad"></param>
    /// <returns>
    /// Returns an array with exact the same size as the given 'keysToLoad' array.
    /// Non existing keys will be represented by a NULL entry at the corresponding position.
    /// </returns>
    public TEntity[] GetEntitiesByKey(TKey[] keysToLoad) {

      if (keysToLoad.Length == 0) {
        return new TEntity[0];
      }

      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {

        Expression<Func<TEntity, bool>> queryByKeys = keysToLoad.BuildInArrayPredicate<TEntity, TKey>(PrimaryKeySet.ToArray());

        //TEntity[] result = dbContext.Set<TEntity>().AsNoTracking().Where(queryByKeys).ToArray();
        //TODO: BUG!! aktuell werden nur die entities zurückgegeben, die existieren,
        //            -> keine NULL-felder im array, pot. verdrehte Sortierung!  
        // Lösung ist schon hier vvvvvvv - muss aber erst noch getestet werden...

        TEntity[] result = new TEntity[keysToLoad.Length];
        //materialization-loop
        foreach (TEntity loadedEntity in dbContext.Set<TEntity>().AsNoTracking().AsNoTracking().Where(queryByKeys)) {
          TKey key = _KeyExtractor(loadedEntity);
          ConversionHelper.SortIntoResultArray(result, loadedEntity, key, keysToLoad);
        }

        return result;
      });
    }

    public TEntity[] GetEntitiesBySearchExpression(
      string searchExpression, string[] sortedBy, int limit = 500, int skip = 0
    ) {
      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {
        var entities = dbContext.Set<TEntity>().AsNoTracking().Where(searchExpression);

        //HACK: internal usage of System.Data.Fuse.LinqSupport
        entities = entities.ApplySortingViaLinqDynamic(sortedBy);
        entities = entities.ApplyPaging(limit, skip);

        return entities.ToArray();
      });
    }

    //AI
    public Dictionary<string, object>[] GetEntityFields(
      ExpressionTree filter,
      string[] includedFieldNames, string[] sortedBy, int limit = 500, int skip = 0
    ) {
      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {
        //HACK: internal usage of System.Data.Fuse.LinqSupport
        IQueryable<TEntity> entities = dbContext.Set<TEntity>().AsNoTracking().Where(filter.CompileToDynamicLinq(GetSchemaRoot().GetSchema(typeof(TEntity).Name)));

        //HACK: internal usage of System.Data.Fuse.LinqSupport
        entities = entities.ApplySortingViaLinqDynamic(sortedBy);
        entities = entities.ApplyPaging(limit, skip);

        // Build the select expression
        string selectExpression = "new(" + string.Join(", ", includedFieldNames) + ")";

        // Use the select expression to select the fields
        dynamic[] selectedFields = entities.Select(selectExpression).ToDynamicArray();

        // Convert the selected fields to dictionaries
        Dictionary<string, object>[] result = selectedFields.Select(sf => {
          var dict = new Dictionary<string, object>();
          foreach (var fieldName in includedFieldNames) {
            dict[fieldName] = sf.GetType().GetProperty(fieldName).GetValue(sf);
          }
          return dict;
        }).ToArray();

        return result;
      });
    }

    //private static IQueryable<TEntity> ApplySorting(string[] sortedBy, IQueryable<TEntity> entities) {
    //  if (sortedBy == null) {
    //    return entities;
    //  }
    //  foreach (var sortField in sortedBy) {
    //    if (sortField.StartsWith("^")) {
    //      string descSortField = sortField.Substring(1); // remove the "^" prefix
    //      //HACK: internal usage of System.Linq.Dynamic.Core
    //      entities = entities.OrderBy(descSortField + " descending");
    //    } else {
    //      entities = entities.OrderBy(sortField);
    //    }
    //  }

    //  return entities;
    //}

    //private static IQueryable<TEntity> ApplyPaging(int limit, int skip, IQueryable<TEntity> entities) {
    //  if (skip == 0 && limit == 0) {
    //    return entities;
    //  } else if (limit == 0) {
    //    return entities.Skip(skip);
    //  } else if (skip == 0) {
    //    return entities.Take(limit);
    //  } else {
    //    return entities.Skip(skip).Take(limit);
    //  }
    //}

    /// <summary>
    /// Loads entity fields by their keys, in exact order as the given key-array.
    /// WARNING: the returned array will contain NULL entries for non-existing entities!
    /// </summary>
    /// <param name="keysToLoad"></param>
    /// <param name="includedFieldNames"></param>
    /// <returns>
    /// Returns an array with exact the same size as the given 'keysToLoad' array.
    /// Non existing keys will be represented by a NULL entry at the corresponding position.
    /// </returns>
    public Dictionary<string, object>[] GetEntityFieldsByKey(
      TKey[] keysToLoad, string[] includedFieldNames
    ) {

      if (keysToLoad.Length == 0) {
        return new Dictionary<string, object>[0];
      }

      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {

        //TODO: BUG!! aktuell werden nur die entities zurückgegeben, die existieren,
        //            -> keine NULL-felder im array, pot. verdrehte Sortierung! 

        //return dbContext.Set<TEntity>().AsNoTracking().Where(
        //  keysToLoad.BuildFilterForKeyValuesExpression<TEntity, TKey>(PrimaryKeySet.ToArray())
        //).Select(
        //  e => new {
        //    e, //TODO is e correct? remove e?
        //    includedFieldNames
        //  }
        //).ToDynamicArray().Select(
        //  e => {
        //    var dict = new Dictionary<string, object>();
        //    foreach (var fieldName in includedFieldNames) {
        //      //HACK: EXTREM TEUER!!
        //      dict[fieldName] = e.e.GetType().GetProperty(fieldName).GetValue(e.e);
        //    }
        //    return dict;
        //  }
        //).ToArray();

        var orderedResult = new ArrayOrderedResult<TKey,Dictionary<string,object>>(keysToLoad);

        Expression<Func<TEntity,bool>> queryByKeys = keysToLoad.BuildInArrayPredicate<TEntity, TKey>(PrimaryKeySet.ToArray());

        string[] keyPropertyNames = PrimaryKeySet.Select(p => p.Name).ToArray();
        string[] fieldNamesToLoad =  includedFieldNames.Union(keyPropertyNames).Distinct().ToArray();

        Expression<Func<TEntity,TEntity>> selector = SelectorMapper.CreateDynamicSelectorExpression<TEntity>(includedFieldNames);

        //full entity-class, but only with selected fields loaded
        TEntity[] partiallyLoadedEntitiesInUnknownOrder = dbContext.Set<TEntity>().AsNoTracking()
          .Where(queryByKeys)
          .Select(selector)
          .ToArray();//MATERIALIZE!

        EntityToDictionaryMapper<TEntity> mapper = new EntityToDictionaryMapper<TEntity>();

        foreach (TEntity partiallyLoadedEntity in partiallyLoadedEntitiesInUnknownOrder) {
          TKey key = _KeyExtractor(partiallyLoadedEntity);
          Dictionary<string, object> resultDict = mapper.MapEntityToDictionary(partiallyLoadedEntity, includedFieldNames);
          orderedResult.SetResultItem(key, resultDict);
        }

        Dictionary<string, object>[] result = orderedResult.GetResultItems(keepEmptyEntriesForMissingResults: true).ToArray();
        return result;
        
      });
    }


    public Dictionary<string, object>[] GetEntityFieldsBySearchExpression(
      string searchExpression,
      string[] includedFieldNames, string[] sortedBy, int limit = 500, int skip = 0
    ) {
      return GetEntitiesBySearchExpression(searchExpression, sortedBy, limit, skip).Select(
        e => {
          var dict = new Dictionary<string, object>();
          foreach (var fieldName in includedFieldNames) {
            dict[fieldName] = e.GetType().GetProperty(fieldName).GetValue(e);
          }
          return dict;
        }
      ).ToArray();
    }

    public EntityRef<TKey>[] GetEntityRefs(
      ExpressionTree filter, string[] sortedBy, int limit = 500, int skip = 0
    ) {
      return GetEntities(filter, sortedBy, limit, skip).Select(
        e => new EntityRef<TKey>(
          e.GetValues(PrimaryKeySet).ToKey<TKey>(),
          ConversionHelper.GetLabel(e, this.GetSchemaRoot())
        )
      ).ToArray();
    }

    /// <summary>
    /// Loads entity references by their keys, in exact order as the given key-array.
    /// WARNING: the returned array will contain NULL entries for non-existing entities!
    /// </summary>
    /// <param name="keysToLoad"></param>
    /// <returns>
    /// Returns an array with exact the same size as the given 'keysToLoad' array.
    /// Non existing keys will be represented by a NULL entry at the corresponding position.
    /// </returns>
    public EntityRef<TKey>[] GetEntityRefsByKey(TKey[] keysToLoad) {

      //HACK: Materialisiert KOMPLETTE Entität und führt diese Methode ad-absodum!
      //      Beim Umbau dran denken: NULL-Einträge für nicht-existierende Keys im Ergebnis-Array!!!
      //      + _ContextInstanceProvider.VisitCurrentDbContext nutzen!!

      return this.GetEntitiesByKey(keysToLoad).Select(
        (e) => (
          e == null ? null : new EntityRef<TKey>(
            e.GetValues(PrimaryKeySet).ToKey<TKey>(),
            ConversionHelper.GetLabel(e, this.GetSchemaRoot())
          )
        )
      ).ToArray();
    }

    public EntityRef<TKey>[] GetEntityRefsBySearchExpression(
      string searchExpression, string[] sortedBy, int limit = 500, int skip = 0
    ) {
      return GetEntitiesBySearchExpression(searchExpression, sortedBy, limit, skip).Select(
        e => new EntityRef<TKey>(e.GetValues(PrimaryKeySet).ToKey<TKey>(),
        ConversionHelper.GetLabel(e, this.GetSchemaRoot())
      )
      ).ToArray();
    }

    public string GetOriginIdentity() {
      return $"EfRepository {typeof(TEntity).Name}";
    }

    /// <summary>
    /// Updates a dedicated subset of fields for all addressed entites and
    /// returns an array containing the keys of affeced entities. 
    /// NOTE: this method can only be used, if the 'SupportsMassupdate'-Capability is given for this repository! 
    /// </summary>
    /// <param name="filter">A filter to adress that entities, which sould be updated.</param>
    /// <param name="fields">A set of fields and value, that should be update.
    /// It MUST NOT contain fields which are part of the Key, otherwise an exception will be thrown!
    /// </param>
    /// <returns></returns>
    public TKey[] Massupdate(ExpressionTree filter, Dictionary<string, object> fields) {
      //TODO: Verwender bitte umbauen auf 'System.Data.Fuse.LinqSupport.ExpressionTreeMapper.BuildLinqExpressionFromTree'
      return MassupdateBySearchExpression(filter.CompileToDynamicLinq(GetSchemaRoot().GetSchema(typeof(TEntity).Name)), fields);
    }

    public TKey[] MassupdateByKeys(TKey[] keysToUpdate, Dictionary<string, object> fields) {
      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {

        // Ensure that the fields to be updated do not include any key fields
        var keyFieldNames = PrimaryKeySet.Select(p => p.Name);
        if (fields.Keys.Intersect(keyFieldNames).Any()) {
          throw new ArgumentException("Update fields must not contain key fields.");
        }

        // Get the entities that match the provided keys
        var entitiesToUpdate = dbContext.Set<TEntity>().Where(
          keysToUpdate.BuildInArrayPredicate<TEntity, TKey>(PrimaryKeySet.ToArray())
        );

        // Update the fields of the entities
        foreach (var entity in entitiesToUpdate) {
          foreach (var field in fields) {
            var propertyInfo = typeof(TEntity).GetProperty(field.Key);
            if (propertyInfo != null) {
              propertyInfo.SetValue(entity, field.Value);
            }
          }
        }

        // Save the changes to the database
        dbContext.SaveChanges();

        // Return the keys of the updated entities
        return entitiesToUpdate.Select(e => e.GetValues(PrimaryKeySet).ToKey<TKey>()).ToArray();
      });
    }

    public TKey[] MassupdateBySearchExpression(string searchExpression, Dictionary<string, object> fields) {
      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {

        // Ensure that the fields to be updated do not include any key fields
        var keyFieldNames = PrimaryKeySet.Select(p => p.Name);
        if (fields.Keys.Intersect(keyFieldNames).Any()) {
          throw new ArgumentException("Update fields must not contain key fields.");
        }

        //HACK: internal usage of System.Data.Fuse.LinqSupport
        var entitiesToUpdate = dbContext.Set<TEntity>().Where(searchExpression);

        // Update the fields of the entities
        foreach (var entity in entitiesToUpdate) {
          foreach (var field in fields) {
            var propertyInfo = typeof(TEntity).GetProperty(field.Key);
            if (propertyInfo != null) {
              propertyInfo.SetValue(entity, field.Value);
            }
          }
        }

        // Save the changes to the database
        dbContext.SaveChanges();

        // Return the keys of the updated entities
        return entitiesToUpdate.Select(e => e.GetValues(PrimaryKeySet).ToKey<TKey>()).ToArray();
      });
    }

    /// <summary>
    /// Adds an new entity and returns its Key on success, otherwise null
    /// (also if the entity is already exisiting).
    /// Depending on the concrete repository implementation the KEY properties
    /// of the entity needs be pre-initialized (see the 'RequiresExternalKeys'-Capability). 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>The entity key on success, otherwise null</returns>
    public TKey TryAddEntity(TEntity entity) {
      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {
        try {
          // Check if the entity already exists
          object[] keySetValues = entity.GetValues(PrimaryKeySet);
          TEntity existingEntity = dbContext.Set<TEntity>().Find(keySetValues);

          // If the entity does not exist, add it
          if (existingEntity == null) {
            this.UnsetIdentityFields(entity);
            dbContext.Set<TEntity>().Add(entity);
            dbContext.SaveChanges();
            return entity.GetValues(PrimaryKeySet).ToKey<TKey>();
          }
        } catch (Exception) {
          // Ignore exceptions and return default(TKey)
        }

        // If the entity already exists or if an error occurs, return default(TKey)
        return default(TKey);
      });
    }


    /// <summary>
    /// Tries to delete entities by the given keys und returns an array containing the keys of only that entities
    /// which were deleted successfully.
    /// NOTE: this method can only be used, if the 'CanDeleteEntities'-Capability is given for this repository! 
    /// </summary>
    /// <param name="keysToDelete"></param>
    /// <returns>keys of deleted entities</returns>
    public TKey[] TryDeleteEntities(TKey[] keysToDelete) {
      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {

        List<TKey> deletedKeys = new List<TKey>();

        foreach (var key in keysToDelete) {
          try {
            // Find the entity by its key
            object[] keySetValues = key.GetKeyFieldValues();
            TEntity entityToDelete = dbContext.Set<TEntity>().Find(keySetValues);

            // If the entity exists, delete it
            if (entityToDelete != null) {
              dbContext.Set<TEntity>().Remove(entityToDelete);
              dbContext.SaveChanges();
              deletedKeys.Add(key);
            }
          } catch (Exception) {
            // Ignore exceptions and continue with the next key
          }
        }

        // Return the keys of the entities that were successfully deleted
        return deletedKeys.ToArray();
      });
    }

    /// <summary>
    /// Updates all updatable fields for an entity and 
    /// returns the entity within its new state or null, if the entity wasn't found.
    /// </summary>
    /// <param name="entity">
    /// NOTE: The target entity which to be updated will be addressed by the key values used from this param!
    /// </param>
    /// <returns>
    /// The entity after it was updated or null, if the entity wasn't found.
    /// NOTE: the returned entity can differ from the given one, because in some cases a field
    /// (1) was not updated,
    /// (2) was updated using normlized (=modified) value that differs from the given one,
    /// (3) was updated implicitely (timestamp's,rowversion's,...) 
    /// </returns>
    public TEntity TryUpdateEntity(TEntity entity) {
      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {
        try {
          // Check if the entity exists
          object[] keySetValues = entity.GetValues(PrimaryKeySet);
          TEntity existingEntity = dbContext.Set<TEntity>().Find(keySetValues);

          // If the entity exists, update it
          if (existingEntity != null) {
            CopyFields(entity, existingEntity);
            dbContext.SaveChanges();
            return existingEntity;
          }
        } catch (Exception) {
          // Ignore exceptions and return null
        }

        // If the entity does not exist or if an error occurs, return null
        return null;
      });
    }

    /// <summary>
    /// Updates the given set of fields for an entity and 
    /// returns all fields that ARE DIFFERENT from the given values or null, if the entity wasn't found.
    /// </summary>
    /// <param name="fields">
    /// NOTE: The target entity to be updated will be addressed by the key values used from this param!
    /// If the given dictionary does not contain the key fiels, an exception will be thrown!
    /// </param>
    /// All fields that ARE DIFFERENT from the given values or null, if the entity wasn't found.
    /// NOTE: the returned entity can differ from the given one, because in some cases a field
    /// (1) was not updated,
    /// (2) was updated using normlized (=modified) value that differs from the given one,
    /// (3) was updated implicitely (timestamp's,rowversion's,...) 
    public Dictionary<string, object> TryUpdateEntityFields(Dictionary<string, object> fields) {

      return _ContextInstanceProvider.VisitCurrentDbContext<Dictionary<string, object>>((dbContext) => {

        // Check for existing entity by primary key
        object[] primaryKeyValues = fields.TryGetValuesByFields(PrimaryKeySet);
        TEntity existingEntity = (primaryKeyValues == null)
          ? null : dbContext.Set<TEntity>().Find(primaryKeyValues);

        // If not found by primary key, check for existing entity by unique keysets
        if (existingEntity == null) {
          foreach (var keyset in Keysets) {
            object[] keysetValues = fields.GetValuesFromDictionary(keyset);
            existingEntity = (keysetValues == null)
              ? null : dbContext.Set<TEntity>().Find(keysetValues);
            if (existingEntity != null) {
              break;
            }
          }
        }

        if (existingEntity != null) {
          // If existing entity found, update it
          CopyFields2(fields, existingEntity);
        } else {
          return null;
        }

        dbContext.SaveChanges();

        // Convert the updated entity back to a dictionary and return it
        Dictionary<string, object> conflictingFields = new Dictionary<string, object>();
        foreach (var propertyInfo in typeof(TEntity).GetProperties()) {
          var updatedValue = propertyInfo.GetValue(existingEntity);
          if (
            !fields.TryGetValue(propertyInfo.Name, out var originalValue) ||
            !Equals(updatedValue, originalValue)
          ) {
            conflictingFields[propertyInfo.Name] = updatedValue;
          }
        }

        return conflictingFields;
      });
    }

    /// <summary>
    /// Changes the KEY for an entity.
    /// NOTE: this method can only be used, if the 'SupportsKeyUpdate'-Capability is given for this repository! 
    /// </summary>
    /// <param name="currentKey"></param>
    /// <param name="newKey"></param>
    /// <returns></returns>
    public bool TryUpdateKey(TKey currentKey, TKey newKey) {
      return _ContextInstanceProvider.VisitCurrentDbContext((dbContext) => {
        try {
          // Find the entity with the current key
          object[] currentKeyValues = currentKey.GetKeyFieldValues();
          TEntity existingEntity = dbContext.Set<TEntity>().Find(currentKeyValues);

          if (existingEntity != null) {
            // Check if the entity with the new key already exists
            object[] newKeyValues = newKey.GetKeyFieldValues();
            TEntity newEntity = dbContext.Set<TEntity>().Find(newKeyValues);

            if (newEntity == null) {
              // Create a copy of the entity and set the new key values
              TEntity updatedEntity = Activator.CreateInstance<TEntity>();
              CopyFields(existingEntity, updatedEntity);

              int i = 0;
              foreach (PropertyInfo propertyInfo in PrimaryKeySet) {
                var newKeyValue = newKeyValues[i++];
                propertyInfo.SetValue(updatedEntity, newKeyValue);
              }

              // Remove the old entity
              dbContext.Set<TEntity>().Remove(existingEntity);
              // Add the new entity
              this.UnsetIdentityFields(updatedEntity);
              dbContext.Set<TEntity>().Add(updatedEntity);

              dbContext.SaveChanges();
              return true;
            }
          }
        } catch (Exception) {
          // Ignore exceptions and return false
        }

        return false;
      });
    }

  }

}
