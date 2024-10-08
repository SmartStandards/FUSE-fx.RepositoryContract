using System.Collections.Generic;

namespace System.Data.Fuse {

  public static class UniversalRepoExtensions {

    /// <summary> </summary>
    /// <param name="filter"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public static EntityRef[] GetEntityRefs<TEntity>(
      this IUniversalRepository repo, ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return repo.GetEntityRefs(typeof(TEntity).Name, filter, sortedBy, limit, skip);
    }

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="searchExpression"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public static EntityRef[] GetEntityRefsBySearchExpression<TEntity>(
      this IUniversalRepository repo, string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return repo.GetEntityRefsBySearchExpression(
        typeof(TEntity).Name, searchExpression, sortedBy, limit, skip
      );
    }

    public static EntityRef[] GetEntityRefsByKey<TEntity>(
      this IUniversalRepository repo, object[] keysToLoad
    ) {
      return repo.GetEntityRefsByKey(typeof(TEntity).Name, keysToLoad);
    }

    /// <summary> </summary>
    /// <param name="filter"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public static object[] GetEntities<TEntity>(
      this IUniversalRepository repo, ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return repo.GetEntities(
        typeof(TEntity).Name, filter, sortedBy, limit, skip
      );
    }

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="searchExpression"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public static object[] GetEntitiesBySearchExpression<TEntity>(
      this IUniversalRepository repo, string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return repo.GetEntitiesBySearchExpression(
        typeof(TEntity).Name, searchExpression, sortedBy, limit, skip
      );
    }

    /// <summary> </summary>
    /// <param name="keysToLoad"></param>
    /// <returns></returns>
    public static object[] GetEntitiesByKey<TEntity>(
      this IUniversalRepository repo, object[] keysToLoad
    ) {
      return repo.GetEntitiesByKey(typeof(TEntity).Name, keysToLoad);
    }

    /// <summary> </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="filter"></param>
    /// <param name="includedFieldNames">
    /// An array of field names to be loaded
    /// </param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public static Dictionary<string, object>[] GetEntityFields<TEntity>(
      this IUniversalRepository repo, string entityName, ExpressionTree filter,
      string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return repo.GetEntityFields(typeof(TEntity).Name, filter, includedFieldNames, sortedBy, limit, skip);
    }

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="searchExpression"></param>
    /// <param name="includedFieldNames">
    /// An array of field names to be loaded
    /// </param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public static Dictionary<string, object>[] GetEntityFieldsBySearchExpression<TEntity>(
      this IUniversalRepository repo, string searchExpression, string[] includedFieldNames,
      string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return repo.GetEntityFieldsBySearchExpression(
        typeof(TEntity).Name, searchExpression, includedFieldNames, sortedBy, limit, skip
      );
    }

    public static Dictionary<string, object>[] GetEntityFieldsByKey<TEntity>(
      this IUniversalRepository repo, object[] keysToLoad, string[] includedFieldNames
    ) {
      return repo.GetEntityFieldsByKey(typeof(TEntity).Name, keysToLoad, includedFieldNames);
    }

    public static int CountAll<TEntity>(this IUniversalRepository repo) {
      return repo.CountAll(typeof(TEntity).Name);
    }

    public static int Count<TEntity>(this IUniversalRepository repo, ExpressionTree filter) {
      return repo.Count(typeof(TEntity).Name, filter);
    }

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="searchExpression"></param>
    /// <returns></returns>
    public static int CountBySearchExpression<TEntity>(
      this IUniversalRepository repo, string searchExpression
    ) {
      return repo.CountBySearchExpression(typeof(TEntity).Name, searchExpression);
    }

    public static bool ContainsKey<TEntity>(this IUniversalRepository repo, object key) {
      return repo.ContainsKey(typeof(TEntity).Name, key);
    }

    /// <summary>
    /// Creates an new Entity or Updates the given set of fields for an entity and 
    /// returns all fields that ARE DIFFERENT from the given values or null, if the entity wasn't found.
    /// </summary>
    /// <param name="fields">
    /// NOTE: The target entity to be updated will be addressed by the key values used from this param,
    /// so if the given dictionary contains key fields which are addressing an exisiting record, then it will be updated.
    /// (CASE): 
    /// If the given dictionary contains NO key fields, then the result is depending on the concrete repository
    /// implementation (see the 'RequiresExternalKeys'-Capability):
    /// If external keys are required, this method will skip crating a record and return null,
    /// otherwise it will create a new record (with an new key) and return it.
    /// (CASE): 
    /// If the given dictionary contains key fields, which are addressing an NOT exisiting entity then the
    /// result is again depending on the concrete repository implementation (see the 'RequiresExternalKeys'-Capability):
    /// If external keys are required, this method will crate a record (using the given key) and return it,
    /// otherwise it will skip create a new record (with an new key) and return it.
    /// (CASE): 
    /// If the given dictionary contains key fields, which are addressing an NOT exisiting entity then
    /// it will add it and return it again. Depending on the concrete repository implementation
    /// (see the 'RequiresExternalKeys'-Capability) it will either use the given key oder create a assign
    /// a new key that will be present within the returned fields). For that reason it is IMPORTANT,
    /// that the call needs to evaluate the returned key!
    /// </param>
    /// All fields that ARE DIFFERENT from the given values or null, if the entity wasn't found.
    /// NOTE: the returned entity can differ from the given one, because in some cases a field
    /// (1) was not updated,
    /// (2) was updated using normlized (=modified) value that differs from the given one,
    /// (3) was updated implicitely (timestamp's,rowversion's,...) 
    public static Dictionary<string, object> AddOrUpdateEntityFields<TEntity>(
      this IUniversalRepository repo, Dictionary<string, object> fields
    ) {
      return repo.AddOrUpdateEntityFields(typeof(TEntity).Name, fields);
    }

    /// <summary>
    /// Creates an new Entity or Updates the given set of fields for an entity and 
    /// returns the entity within its new state or null, if the entity wasn't found.
    /// </summary>
    /// <param name="entity">
    /// NOTE: The target entity to be updated will be addressed by the key values used from this param,
    /// so if the given dictionary contains key fields which are addressing an exisiting record, then it will be updated.
    /// (CASE): 
    /// If the given dictionary contains NO key fields, then the result is depending on the concrete repository
    /// implementation (see the 'RequiresExternalKeys'-Capability):
    /// If external keys are required, this method will skip crating a record and return null,
    /// otherwise it will create a new record (with an new key) and return it.
    /// (CASE): 
    /// If the given dictionary contains key fields, which are addressing an NOT exisiting entity then the
    /// result is again depending on the concrete repository implementation (see the 'RequiresExternalKeys'-Capability):
    /// If external keys are required, this method will crate a record (using the given key) and return it,
    /// otherwise it will skip create a new record (with an new key) and return it.
    /// (CASE): 
    /// If the given dictionary contains key fields, which are addressing an NOT exisiting entity then
    /// it will add it and return it again. Depending on the concrete repository implementation
    /// (see the 'RequiresExternalKeys'-Capability) it will either use the given key oder create a assign
    /// a new key that will be present within the returned fields). For that reason it is IMPORTANT,
    /// that the call needs to evaluate the returned key!
    /// </param>
    /// returns the entity within its new state or null, if the entity wasn't found.
    /// NOTE: the returned entity can differ from the given one, because in some cases a field
    /// (1) was not updated,
    /// (2) was updated using normlized (=modified) value that differs from the given one,
    /// (3) was updated implicitely (timestamp's,rowversion's,...) 
    public static TEntity AddOrUpdateEntity<TEntity>(
      this IUniversalRepository repo, TEntity entity
    ) {
      return (TEntity) repo.AddOrUpdateEntity(typeof(TEntity).Name, entity);
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
    public static Dictionary<string, object> TryUpdateEntityFields<TEntity>(
      this IUniversalRepository repo, Dictionary<string, object> fields
    ) {
      return repo.TryUpdateEntityFields(typeof(TEntity).Name, fields);
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
    public static TEntity TryUpdateEntity<TEntity>(
      this IUniversalRepository repo, TEntity entity
    ) {
      return (TEntity) repo.TryUpdateEntity(typeof(TEntity).Name, entity);
    }

    /// <summary>
    /// Adds an new entity and returns its Key on success, otherwise null
    /// (also if the entity is already exisiting).
    /// Depending on the concrete repository implementation the KEY properties
    /// of the entity needs be pre-initialized (see the 'RequiresExternalKeys'-Capability). 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>The entity key on success, otherwise null</returns>
    public static TEntity TryAddEntity<TEntity>(
      this IUniversalRepository repo, TEntity entity
    ) {
      return (TEntity) repo.TryAddEntity(typeof(TEntity).Name, entity);
    }

    /// <summary>
    /// Updates a dedicated subset of fields for all addressed entites and
    /// returns an array containing the keys of affeced entities. 
    /// NOTE: this method can only be used, if the 'SupportsMassupdate'-Capability is given for this repository! 
    /// </summary>
    /// <param name="keysToUpdate">Keys for that entities, which sould be updated (non exisiting keys will be ignored).</param>
    /// <param name="fields">A set of fields and value, that should be update.
    /// It MUST NOT contain fields which are part of the Key, otherwise an exception will be thrown!
    /// </param>
    /// <returns>An array containing the keys of affeced entities.</returns>
    public static object[] MassupdateByKeys<TEntity>(
      this IUniversalRepository repo, object[] keysToUpdate, Dictionary<string, object> fields
    ) {
      return repo.MassupdateByKeys(typeof(TEntity).Name, keysToUpdate, fields);
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
    public static object[] Massupdate<TEntity>(
      this IUniversalRepository repo, ExpressionTree filter, Dictionary<string, object> fields
    ) {
      return repo.Massupdate(typeof(TEntity).Name, filter, fields);
    }

    /// <summary>
    /// Updates a dedicated subset of fields for all addressed entites and
    /// returns an array containing the keys of affeced entities. 
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability and the
    /// 'SupportsMassupdate'-Capability are given for this repository! 
    /// </summary>
    /// <param name="searchExpression">A search expression to adress that entities, which sould be updated.</param>
    /// <param name="fields">A set of fields and value, that should be update.
    /// It MUST NOT contain fields which are part of the Key, otherwise an exception will be thrown!
    /// </param>
    /// <returns></returns>
    public static object[] MassupdateBySearchExpression<TEntity>(
      this IUniversalRepository repo, string searchExpression, Dictionary<string, object> fields
    ) {
      return repo.MassupdateBySearchExpression(typeof(TEntity).Name, searchExpression, fields);
    }

    /// <summary>
    /// Tries to delete entities by the given keys und returns an array containing the keys of only that entities
    /// which were deleted successfully.
    /// NOTE: this method can only be used, if the 'CanDeleteEntities'-Capability is given for this repository! 
    /// </summary>
    /// <param name="keysToDelete"></param>
    /// <returns>keys of deleted entities</returns>
    public static object[] TryDeleteEntities<TEntity>(
      this IUniversalRepository repo, object[] keysToDelete
    ) {
      return repo.TryDeleteEntities(typeof(TEntity).Name, keysToDelete);
    }

    /// <summary>
    /// Changes the KEY for an entity.
    /// NOTE: this method can only be used, if the 'SupportsKeyUpdate'-Capability is given for this repository! 
    /// </summary>
    /// <param name="currentKey"></param>
    /// <param name="newKey"></param>
    /// <returns></returns>
    public static bool TryUpdateKey<TEntity>(
      this IUniversalRepository repo, object currentKey, object newKey
    ) {
      return repo.TryUpdateKey(typeof(TEntity).Name, currentKey, newKey);
    }

    public static void CopyValuesToEntity<TEntity>(
      this Dictionary<string, object> fields, TEntity targetEntity
    ) {
      foreach (string fieldName in fields.Keys) {
        var prop = typeof(TEntity).GetProperty(fieldName);
        if (prop != null) {
          prop.SetValue(targetEntity, fields[fieldName], null);
        }
      }
    }

  }

}
