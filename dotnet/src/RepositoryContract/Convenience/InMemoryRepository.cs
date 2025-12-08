using System.Collections.Generic;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Data.ModelDescription.Convenience;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace System.Data.Fuse.Convenience {

  public class InMemoryRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class {

    private static SchemaRoot _SchemaRoot;
    private static List<TEntity> _Entities = new List<TEntity>();

    protected SchemaRoot SchemaRoot {
      get {
        return _SchemaRoot;
      }
    }

    public InMemoryRepository(SchemaRoot schemaRoot) {
      _SchemaRoot = schemaRoot;
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
      return SchemaRoot.GetUniqueKeysetsProperties(typeof(TEntity));
    }

    protected List<PropertyInfo> InitPrimaeyKeySet() {
      return SchemaRoot.GetPrimaryKeyProperties(typeof(TEntity)).ToList();
    }

    public TEntity AddOrUpdateEntity(TEntity entity) {

      object[] keySetValues = entity.GetValues(PrimaryKeySet);
      TEntity existingEntity = _Entities.FirstOrDefault(
        entity.GetSearchExpression(PrimaryKeySet.ToArray()).Compile()
      );

      //AI
      // If no primary key is found, try to find the entity by unique key
      if (existingEntity == null) {
        foreach (var keyset in Keysets) {
          existingEntity = _Entities.FirstOrDefault(
            entity.GetSearchExpression(keyset.ToArray()).Compile()
          );
          if (existingEntity != null) {
            break;
          }
        }
      }

      if (existingEntity == null) {
        _Entities.Add(entity);
        return entity;
      } else {
        CopyFields(entity, existingEntity);
        return existingEntity;
      }
    }

    private void CopyFields(TEntity from, TEntity to) {
      EntitySchema schema = SchemaRoot.GetSchema(typeof(TEntity).Name);
      foreach (PropertyInfo propertyInfo in typeof(TEntity).GetProperties()) {
        if (!schema.Fields.Any((f) => f.Name == propertyInfo.Name)) continue;
        propertyInfo.SetValue(to, propertyInfo.GetValue(from, null), null);
      }
    }

    public Dictionary<string, object> AddOrUpdateEntityFields(
      Dictionary<string, object> fields
    ) {

      // Check for existing entity by primary key
      object[] primaryKeyValues = fields.TryGetValuesByFields(PrimaryKeySet);
      TEntity existingEntity = (primaryKeyValues == null)
        ? null : _Entities.FindMatchByValues(primaryKeyValues, PrimaryKeySet.ToArray());

      // If not found by primary key, check for existing entity by unique keysets
      if (existingEntity == null) {
        foreach (var keyset in Keysets) {
          object[] keysetValues = fields.GetValuesFromDictionary(keyset);
          existingEntity = (keysetValues == null)
            ? null : _Entities.FindMatchByValues(keysetValues, keyset.ToArray());
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
        _Entities.Add(entity);
      }

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
    }

    private static void CopyFields2(Dictionary<string, object> fields, TEntity entity) {
      foreach (var field in fields) {
        PropertyInfo propertyInfo = typeof(TEntity).GetProperty(field.Key);
        if (propertyInfo != null) {
          propertyInfo.SetValue(entity, field.Value);
        }
      }
    }

    public bool ContainsKey(TKey key) {
      return _Entities.FindMatchByValues(key.GetKeyFieldValues(), PrimaryKeySet.ToArray()) != null;
    }

    public int Count(ExpressionTree filter) {
      //TODO: Verwender bitte umbauen auf 'System.Data.Fuse.LinqSupport.ExpressionTreeMapper.BuildLinqExpressionFromTree'
      return _Entities.AsQueryable().Count(filter.CompileToDynamicLinq(SchemaRoot.GetSchema(typeof(TEntity).Name)));
    }

    public int CountAll() {
      return _Entities.Count();
    }

    public int CountBySearchExpression(string searchExpression) {
      return _Entities.AsQueryable().Count(searchExpression);
    }

    public RepositoryCapabilities GetCapabilities() {
      return RepositoryCapabilities.All;
    }

    public TEntity[] GetEntities(
      ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    ) {

      //TODO: Verwender bitte umbauen auf 'System.Data.Fuse.LinqSupport.ExpressionTreeMapper.BuildLinqExpressionFromTree'
      string stringbasedDynamicLinqExpression = filter.CompileToDynamicLinq(SchemaRoot.GetSchema(typeof(TEntity).Name));
      IQueryable<TEntity> entities = _Entities.AsQueryable().Where(stringbasedDynamicLinqExpression);

      entities = ApplySorting(sortedBy, entities);
      entities = ApplyPaging(limit, skip, entities);

      return entities.ToArray();
    }

    public TEntity[] GetEntitiesByKey(TKey[] keysToLoad) {
      return _Entities.Where(
        keysToLoad.BuildFilterForKeyValuesExpression<TEntity, TKey>(PrimaryKeySet.ToArray()).Compile()
      ).ToArray();
    }

    public TEntity[] GetEntitiesBySearchExpression(
      string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      var entities = _Entities.AsQueryable().Where(searchExpression);
      entities = ApplySorting(sortedBy, entities);
      entities = ApplyPaging(limit, skip, entities);

      return entities.ToArray();

    }

    //AI
    public Dictionary<string, object>[] GetEntityFields(
      ExpressionTree filter,
      string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      //TODO: Verwender bitte umbauen auf 'System.Data.Fuse.LinqSupport.ExpressionTreeMapper.BuildLinqExpressionFromTree'
      var entities = _Entities.AsQueryable().Where(filter.CompileToDynamicLinq(SchemaRoot.GetSchema(typeof(TEntity).Name)));

      entities = ApplySorting(sortedBy, entities);
      entities = ApplyPaging(limit, skip, entities);

      // Build the select expression
      string selectExpression = "new(" + string.Join(", ", includedFieldNames) + ")";

      // Use the select expression to select the fields
      var selectedFields = entities.Select(selectExpression).ToDynamicArray();

      // Convert the selected fields to dictionaries
      Dictionary<string, object>[] result = selectedFields.Select(sf => {
        var dict = new Dictionary<string, object>();
        foreach (var fieldName in includedFieldNames) {
          dict[fieldName] = sf.GetType().GetProperty(fieldName).GetValue(sf);
        }
        return dict;
      }).ToArray();

      return result;
    }

    private static IQueryable<TEntity> ApplySorting(string[] sortedBy, IQueryable<TEntity> entities) {
      foreach (var sortField in sortedBy) {
        if (sortField.StartsWith("^")) {
          string descSortField = sortField.Substring(1); // remove the "^" prefix
          entities = entities.OrderBy(descSortField + " descending");
        } else {
          entities = entities.OrderBy(sortField);
        }
      }

      return entities;
    }

    private static IQueryable<TEntity> ApplyPaging(int limit, int skip, IQueryable<TEntity> entities) {
      if (skip == 0 && limit == 0) {
        return entities;
      } else if (limit == 0) {
        return entities.Skip(skip);
      } else if (skip == 0) {
        return entities.Take(limit);
      } else {
        return entities.Skip(skip).Take(limit);
      }
    }

    public Dictionary<string, object>[] GetEntityFieldsByKey(
      TKey[] keysToLoad, string[] includedFieldNames
    ) {
      return _Entities.Where(
        keysToLoad.BuildFilterForKeyValuesExpression<TEntity, TKey>(PrimaryKeySet.ToArray()).Compile()
      ).Select(
        e => new {
          e, //TODO is e correct? remove e?
          includedFieldNames
        }
      ).ToDynamicArray().Select(
        e => {
          var dict = new Dictionary<string, object>();
          foreach (var fieldName in includedFieldNames) {
            dict[fieldName] = e.e.GetType().GetProperty(fieldName).GetValue(e.e);
          }
          return dict;
        }
      ).ToArray();
    }

    public Dictionary<string, object>[] GetEntityFieldsBySearchExpression(
      string searchExpression,
      string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0
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
      ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return GetEntities(filter, sortedBy, limit, skip).Select(
        e => new EntityRef<TKey>(e.GetValues(PrimaryKeySet).ToKey<TKey>(), e.ToString())
      ).ToArray();
    }

    public EntityRef<TKey>[] GetEntityRefsByKey(TKey[] keysToLoad) {
      return GetEntitiesByKey(keysToLoad).Select(
        e => new EntityRef<TKey>(e.GetValues(PrimaryKeySet).ToKey<TKey>(), e.ToString())
      ).ToArray();
    }

    public EntityRef<TKey>[] GetEntityRefsBySearchExpression(
      string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return GetEntitiesBySearchExpression(searchExpression, sortedBy, limit, skip).Select(
        e => new EntityRef<TKey>(e.GetValues(PrimaryKeySet).ToKey<TKey>(), e.ToString())
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
      return MassupdateBySearchExpression(filter.CompileToDynamicLinq(SchemaRoot.GetSchema(typeof(TEntity).Name)), fields);
    }


    public TKey[] MassupdateByKeys(TKey[] keysToUpdate, Dictionary<string, object> fields) {
      // Ensure that the fields to be updated do not include any key fields
      var keyFieldNames = PrimaryKeySet.Select(p => p.Name);
      if (fields.Keys.Intersect(keyFieldNames).Any()) {
        throw new ArgumentException("Update fields must not contain key fields.");
      }

      // Get the entities that match the provided keys
      var entitiesToUpdate = _Entities.Where(
        keysToUpdate.BuildFilterForKeyValuesExpression<TEntity, TKey>(PrimaryKeySet.ToArray()).Compile()
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

      // Return the keys of the updated entities
      return entitiesToUpdate.Select(e => e.GetValues(PrimaryKeySet).ToKey<TKey>()).ToArray();
    }

    public TKey[] MassupdateBySearchExpression(string searchExpression, Dictionary<string, object> fields) {
      // Ensure that the fields to be updated do not include any key fields
      var keyFieldNames = PrimaryKeySet.Select(p => p.Name);
      if (fields.Keys.Intersect(keyFieldNames).Any()) {
        throw new ArgumentException("Update fields must not contain key fields.");
      }

      // Get the entities that match the search expression
      var entitiesToUpdate = _Entities.AsQueryable().Where(searchExpression);

      // Update the fields of the entities
      foreach (var entity in entitiesToUpdate) {
        foreach (var field in fields) {
          var propertyInfo = typeof(TEntity).GetProperty(field.Key);
          if (propertyInfo != null) {
            propertyInfo.SetValue(entity, field.Value);
          }
        }
      }

      // Return the keys of the updated entities
      return entitiesToUpdate.Select(e => e.GetValues(PrimaryKeySet).ToKey<TKey>()).ToArray();
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
      try {
        // Check if the entity already exists
        object[] keySetValues = entity.GetValues(PrimaryKeySet);
        TEntity existingEntity = _Entities.FindMatch(entity, PrimaryKeySet.ToArray());

        // If the entity does not exist, add it
        if (existingEntity == null) {
          _Entities.Add(entity);
          return entity.GetValues(PrimaryKeySet).ToKey<TKey>();
        }
      } catch (Exception) {
        // Ignore exceptions and return default(TKey)
      }

      // If the entity already exists or if an error occurs, return default(TKey)
      return default(TKey);
    }


    /// <summary>
    /// Tries to delete entities by the given keys und returns an array containing the keys of only that entities
    /// which were deleted successfully.
    /// NOTE: this method can only be used, if the 'CanDeleteEntities'-Capability is given for this repository! 
    /// </summary>
    /// <param name="keysToDelete"></param>
    /// <returns>keys of deleted entities</returns>
    public TKey[] TryDeleteEntities(TKey[] keysToDelete) {
      List<TKey> deletedKeys = new List<TKey>();

      foreach (var key in keysToDelete) {
        try {
          // Find the entity by its key
          object[] keySetValues = key.GetKeyFieldValues();
          TEntity entityToDelete = _Entities.FindMatchByValues(keySetValues, PrimaryKeySet.ToArray());

          // If the entity exists, delete it
          if (entityToDelete != null) {
            _Entities.Remove(entityToDelete);
            deletedKeys.Add(key);
          }
        } catch (Exception) {
          // Ignore exceptions and continue with the next key
        }
      }

      // Return the keys of the entities that were successfully deleted
      return deletedKeys.ToArray();
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
      try {
        // Check if the entity exists
        object[] keySetValues = entity.GetValues(PrimaryKeySet);
        TEntity existingEntity = _Entities.FindMatchByValues(keySetValues, PrimaryKeySet.ToArray());

        // If the entity exists, update it
        if (existingEntity != null) {
          CopyFields(entity, existingEntity);
          return existingEntity;
        }
      } catch (Exception) {
        // Ignore exceptions and return null
      }

      // If the entity does not exist or if an error occurs, return null
      return null;
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
      // Check for existing entity by primary key
      object[] primaryKeyValues = fields.TryGetValuesByFields(PrimaryKeySet);
      TEntity existingEntity = (primaryKeyValues == null)
        ? null : _Entities.FindMatchByValues(primaryKeyValues, PrimaryKeySet.ToArray());

      // If not found by primary key, check for existing entity by unique keysets
      if (existingEntity == null) {
        foreach (var keyset in Keysets) {
          object[] keysetValues = fields.GetValuesFromDictionary(keyset);
          existingEntity = (keysetValues == null)
            ? null : _Entities.FindMatchByValues(keysetValues, keyset.ToArray());
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
    }

    /// <summary>
    /// Changes the KEY for an entity.
    /// NOTE: this method can only be used, if the 'SupportsKeyUpdate'-Capability is given for this repository! 
    /// </summary>
    /// <param name="currentKey"></param>
    /// <param name="newKey"></param>
    /// <returns></returns>
    public bool TryUpdateKey(TKey currentKey, TKey newKey) {
      try {
        // Check if the entity with the current key exists
        object[] currentKeyValues = currentKey.GetKeyFieldValues();
        TEntity existingEntity = _Entities.FindMatchByValues(currentKeyValues, PrimaryKeySet.ToArray());

        // If the entity exists, update its key
        if (existingEntity != null) {
          // Check if the entity with the new key already exists
          object[] newKeyValues = newKey.GetKeyFieldValues();
          TEntity newEntity = _Entities.FindMatchByValues(newKeyValues, PrimaryKeySet.ToArray());

          // If the entity with the new key does not exist, update the key
          if (newEntity == null) {
            int i = 0;
            foreach (PropertyInfo propertyInfo in PrimaryKeySet) {
              object newKeyValue = newKeyValues[i++];
              propertyInfo.SetValue(existingEntity, newKeyValue);
            }            
            
            return true;
          }
        }
      } catch (Exception) {
        // Ignore exceptions and return false
      }

      // If the entity does not exist, the entity with the new key already exists, or if an error occurs, return false
      return false;
    }

  }

}