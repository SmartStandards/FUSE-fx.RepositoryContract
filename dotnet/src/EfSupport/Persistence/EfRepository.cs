using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data.Fuse.Convenience;

#if !NETCOREAPP
using System.Data.Entity;
#else
using System.Text.Json;
#endif
using System.Data.ModelDescription;
using System.Data.ModelDescription.Convenience;
using System.Linq;
using System.Reflection;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace System.Data.Fuse.Ef {

  public class EfRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class {

    private DbContext _DbContext;
    private static SchemaRoot _SchemaRoot;

    protected SchemaRoot SchemaRoot {
      get {
        if (_SchemaRoot == null) {
#if NETCOREAPP
          string[] typenames = _DbContext.Model.GetEntityTypes().Select(t => t.Name).ToArray();
#else
          string[] typenames = _DbContext.GetManagedTypeNames();
#endif
          _SchemaRoot = ModelReader.GetSchema(typeof(TEntity).Assembly, typenames);
        }
        return _SchemaRoot;
      }
    }

    public EfRepository(DbContext context) {
      _DbContext = context;
    }

    public EfRepository(DbContext context, SchemaRoot schemaRoot) {
      _DbContext = context;
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
      TEntity existingEntity = this._DbContext.Set<TEntity>().Find(keySetValues.ToArray());

      //AI
      // If no primary key is found, try to find the entity by unique key
      if (existingEntity == null) {
        foreach (var keyset in Keysets) {
          object[] keysetValues = entity.GetValues(keyset);
          existingEntity = this._DbContext.Set<TEntity>().Find(keysetValues);
          if (existingEntity != null) {
            break;
          }
        }
      }

      if (existingEntity == null) {
        _DbContext.Set<TEntity>().Add(entity);
        _DbContext.SaveChanges();
        return entity;
      } else {
        CopyFields(entity, existingEntity);
        _DbContext.SaveChanges();
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

    //AI - minor adjustments
    public Dictionary<string, object> AddOrUpdateEntityFields(
      Dictionary<string, object> fields
    ) {

      // Check for existing entity by primary key
      object[] primaryKeyValues = fields.TryGetValuesByFields(PrimaryKeySet);
      TEntity existingEntity = (primaryKeyValues == null)
        ? null : _DbContext.Set<TEntity>().Find(primaryKeyValues);

      // If not found by primary key, check for existing entity by unique keysets
      if (existingEntity == null) {
        foreach (var keyset in Keysets) {
          object[] keysetValues = fields.GetValues(keyset);
          existingEntity = (keysetValues == null)
            ? null : _DbContext.Set<TEntity>().Find(keysetValues);
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
        _DbContext.Set<TEntity>().Add(entity);
      }

      _DbContext.SaveChanges();

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
      return _DbContext.Set<TEntity>().Find(key.GetKeyFieldValues()) != null;
    }

    public int Count(ExpressionTree filter) {
      return _DbContext.Set<TEntity>().Count(filter.CompileToDynamicLinq(SchemaRoot.GetSchema(typeof(TEntity).Name)));
    }

    public int CountAll() {
      return _DbContext.Set<TEntity>().Count();
    }

    public int CountBySearchExpression(string searchExpression) {
      return _DbContext.Set<TEntity>().Count(searchExpression);
    }

    public RepositoryCapabilities GetCapabilities() {
      return RepositoryCapabilities.All;
    }

    public TEntity[] GetEntities(
      ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      IQueryable<TEntity> entities;
      if (filter == null) {
        entities = _DbContext.Set<TEntity>();
      } else {
        entities = _DbContext.Set<TEntity>().Where(filter.CompileToDynamicLinq(SchemaRoot.GetSchema(typeof(TEntity).Name)));
      }

      entities = ApplySorting(sortedBy, entities);

      return entities.Skip(skip).Take(limit).ToArray();
    }

    public TEntity[] GetEntitiesByKey(TKey[] keysToLoad) {
      var lambda = keysToLoad.BuildFilterForKeyValuesExpression<TEntity, TKey>(PrimaryKeySet.ToArray());
      return _DbContext.Set<TEntity>().Where(lambda).ToArray();
    }


    public TEntity[] GetEntitiesBySearchExpression(
      string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      var entities = _DbContext.Set<TEntity>().Where(searchExpression);

      entities = ApplySorting(sortedBy, entities);

      return entities.Skip(skip).Take(limit).ToArray();

    }

    //AI
    public Dictionary<string, object>[] GetEntityFields(
      ExpressionTree filter,
      string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      var entities = _DbContext.Set<TEntity>().Where(filter.CompileToDynamicLinq(SchemaRoot.GetSchema(typeof(TEntity).Name)));

      entities = ApplySorting(sortedBy, entities);

      // Build the select expression
      string selectExpression = "new(" + string.Join(", ", includedFieldNames) + ")";

      // Use the select expression to select the fields
      var selectedFields = entities.Select(selectExpression).Skip(skip).Take(limit).ToDynamicArray();

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
      if (sortedBy == null) {
        return entities;
      }
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

    public Dictionary<string, object>[] GetEntityFieldsByKey(
      TKey[] keysToLoad, string[] includedFieldNames
    ) {
      return _DbContext.Set<TEntity>().Where(
        keysToLoad.BuildFilterForKeyValuesExpression<TEntity, TKey>(PrimaryKeySet.ToArray())
      )
        .Select(
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
      return MassupdateBySearchExpression(filter.CompileToDynamicLinq(SchemaRoot.GetSchema(typeof(TEntity).Name)), fields);
    }


    public TKey[] MassupdateByKeys(TKey[] keysToUpdate, Dictionary<string, object> fields) {
      // Ensure that the fields to be updated do not include any key fields
      var keyFieldNames = PrimaryKeySet.Select(p => p.Name);
      if (fields.Keys.Intersect(keyFieldNames).Any()) {
        throw new ArgumentException("Update fields must not contain key fields.");
      }

      // Get the entities that match the provided keys
      var entitiesToUpdate = _DbContext.Set<TEntity>().Where(
        keysToUpdate.BuildFilterForKeyValuesExpression<TEntity, TKey>(PrimaryKeySet.ToArray())
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
      _DbContext.SaveChanges();

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
      var entitiesToUpdate = _DbContext.Set<TEntity>().Where(searchExpression);

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
      _DbContext.SaveChanges();

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
        TEntity existingEntity = this._DbContext.Set<TEntity>().Find(keySetValues);

        // If the entity does not exist, add it
        if (existingEntity == null) {
          _DbContext.Set<TEntity>().Add(entity);
          _DbContext.SaveChanges();
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
          TEntity entityToDelete = _DbContext.Set<TEntity>().Find(keySetValues);

          // If the entity exists, delete it
          if (entityToDelete != null) {
            _DbContext.Set<TEntity>().Remove(entityToDelete);
            _DbContext.SaveChanges();
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
        TEntity existingEntity = this._DbContext.Set<TEntity>().Find(keySetValues);

        // If the entity exists, update it
        if (existingEntity != null) {
          CopyFields(entity, existingEntity);
          _DbContext.SaveChanges();
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
      // Check if the dictionary contains all the key fields
      var keyFieldNames = PrimaryKeySet.Select(p => p.Name);
      if (!keyFieldNames.All(k => fields.ContainsKey(k))) {
        throw new ArgumentException("The given dictionary must contain all the key fields.");
      }

      try {
        // Check if the entity exists
        object[] keySetValues = fields.TryGetValuesByFields(PrimaryKeySet);
        TEntity existingEntity = this._DbContext.Set<TEntity>().Find(keySetValues);

        // If the entity exists, update its fields
        if (existingEntity != null) {
          CopyFields2(fields, existingEntity);
          _DbContext.SaveChanges();

          // Build a dictionary of the fields that are different from the given values
          Dictionary<string, object> conflictingFields = new Dictionary<string, object>();
          foreach (var field in fields) {
            var propertyInfo = typeof(TEntity).GetProperty(field.Key);
            if (propertyInfo != null) {
              var updatedValue = propertyInfo.GetValue(existingEntity);
              if (!Equals(updatedValue, field.Value)) {
                conflictingFields[field.Key] = updatedValue;
              }
            }
          }

          // Return the dictionary of conflicting fields
          return conflictingFields;
        }
      } catch (Exception) {
        // Ignore exceptions and return null
      }

      // If the entity does not exist or if an error occurs, return null
      return null;
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
        TEntity existingEntity = this._DbContext.Set<TEntity>().Find(currentKeyValues);

        // If the entity exists, update its key
        if (existingEntity != null) {
          // Check if the entity with the new key already exists
          object[] newKeyValues = newKey.GetKeyFieldValues();
          TEntity newEntity = this._DbContext.Set<TEntity>().Find(newKeyValues);

          // If the entity with the new key does not exist, update the key
          if (newEntity == null) {
            foreach (var propertyInfo in PrimaryKeySet) {
              var newKeyValue = newKey.GetType().GetProperty(propertyInfo.Name).GetValue(newKey);
              propertyInfo.SetValue(existingEntity, newKeyValue);
            }

            _DbContext.SaveChanges();
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