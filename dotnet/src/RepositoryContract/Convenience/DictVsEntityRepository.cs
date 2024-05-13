using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if NETCOREAPP
using System.Text.Json;
#endif

namespace System.Data.Fuse.Convenience {

  //AI
  public class DictVsEntityRepository<TEntity, TKey>
    : IRepository<Dictionary<string, object>, object>
    where TEntity : class {

    IRepository<TEntity, TKey> _Repository;

    private readonly Func<PropertyInfo, Dictionary<string, object>, TEntity, bool> _HandlePropertyModelToEntity;
    private readonly Func<PropertyInfo, TEntity, Dictionary<string, object>, bool> _HandlePropertyEntityToModel;

    public DictVsEntityRepository(
      IRepository<TEntity, TKey> repository,
      Func<PropertyInfo, Dictionary<string, object>, TEntity, bool> handlePropertyModelToEntity,
      Func<PropertyInfo, TEntity, Dictionary<string, object>, bool> handlePropertyEntityToModel
    ) {
      _Repository = repository;
      _HandlePropertyModelToEntity = handlePropertyModelToEntity;
      _HandlePropertyEntityToModel = handlePropertyEntityToModel;
    }

    /// <summary>
    /// Returns an string, representing the "Identity" of the current origin.
    /// This can be used to discriminate multiple source repos.
    /// (usually it should be related to a SCOPE like {DbServer}+{DbName/Schema}+{EntityName})
    /// NOTE: This is an technical disciminator and it is not required, that it is an human-readable
    /// "frieldly-name". It can just be an Hash or Uid, so its NOT RECOMMENDED to use it as display label!
    /// </summary>
    public string GetOriginIdentity() {
      return _Repository.GetOriginIdentity();
    }

    /// <summary>
    /// Returns an property bag which holds information about the implemented/supported capabilities of this IRepository.
    /// </summary>
    /// <returns></returns>
    public RepositoryCapabilities GetCapabilities() {
      return _Repository.GetCapabilities();
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
    public EntityRef<object>[] GetEntityRefs(
      ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return _Repository.GetEntityRefs(filter, sortedBy, limit, skip)
        .Select((e) => new EntityRef<object>() { Key = e.Key, Label = e.Label }).ToArray();
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
    public EntityRef<object>[] GetEntityRefsBySearchExpression(
      string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return _Repository.GetCapabilities().SupportsStringBasedSearchExpressions
        ? _Repository.GetEntityRefsBySearchExpression(searchExpression, sortedBy, limit, skip)
        .Select((e) => new EntityRef<object>() { Key = e.Key, Label = e.Label }).ToArray()
        : throw new NotSupportedException("SearchExpressions are not supported by the repository");

    }

    public EntityRef<object>[] GetEntityRefsByKey(
      object[] keysToLoad
    ) {
      return _Repository.GetEntityRefsByKey(keysToLoad.Cast<TKey>().ToArray())
        .Select((e) => new EntityRef<object>() { Key = e.Key, Label = e.Label }).ToArray();
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
    public Dictionary<string, object>[] GetEntities(
      ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return _Repository.GetEntities(filter, sortedBy, limit, skip)
        .Select((e) => e.ConvertToBusinessModelDynamic(_HandlePropertyEntityToModel)).ToArray();
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
    public Dictionary<string, object>[] GetEntitiesBySearchExpression(
      string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return _Repository.GetCapabilities().SupportsStringBasedSearchExpressions
        ? _Repository.GetEntitiesBySearchExpression(searchExpression, sortedBy, limit, skip)
        .Select((e) => e.ConvertToBusinessModelDynamic(_HandlePropertyEntityToModel)).ToArray()
        : throw new NotSupportedException("SearchExpressions are not supported by the repository");
    }

    /// <summary> </summary>
    /// <param name="keysToLoad"></param>
    /// <returns></returns>
    public Dictionary<string, object>[] GetEntitiesByKey(
      object[] keysToLoad
    ) {
      return _Repository.GetEntitiesByKey(keysToLoad.Cast<TKey>().ToArray())
        .Select((e) => e.ConvertToBusinessModelDynamic(_HandlePropertyEntityToModel)).ToArray();
    }

    /// <summary> </summary>
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
    public Dictionary<string, object>[] GetEntityFields(
      ExpressionTree filter, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return _Repository.GetEntityFields(filter, includedFieldNames, sortedBy, limit, skip);
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
    public Dictionary<string, object>[] GetEntityFieldsBySearchExpression(
      string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      return _Repository.GetCapabilities().SupportsStringBasedSearchExpressions
        ? _Repository.GetEntityFieldsBySearchExpression(
          searchExpression, includedFieldNames, sortedBy, limit, skip
        )
        : throw new NotSupportedException("SearchExpressions are not supported by the repository");
    }

    public Dictionary<string, object>[] GetEntityFieldsByKey(
        object[] keysToLoad, string[] includedFieldNames
    ) {
      return _Repository.GetEntityFieldsByKey(keysToLoad.Cast<TKey>().ToArray(), includedFieldNames);
    }

    public int CountAll() {
      return _Repository.CountAll();
    }

    public int Count(ExpressionTree filter) {
      return _Repository.Count(filter);
    }

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="searchExpression"></param>
    /// <returns></returns>
    public int CountBySearchExpression(string searchExpression) {
      return _Repository.GetCapabilities().SupportsStringBasedSearchExpressions
        ? _Repository.CountBySearchExpression(searchExpression)
        : throw new NotSupportedException("SearchExpressions are not supported by the repository");
    }

    public bool ContainsKey(object key) {
      return _Repository.ContainsKey((TKey)key);
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
    public Dictionary<string, object> AddOrUpdateEntityFields(Dictionary<string, object> fields) {
      return _Repository.AddOrUpdateEntityFields(fields);
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
    public Dictionary<string, object> AddOrUpdateEntity(Dictionary<string, object> entity) {
      return _Repository.AddOrUpdateEntity(
        entity.ConvertToEntityDynamic(_HandlePropertyModelToEntity)
      ).ConvertToBusinessModelDynamic(_HandlePropertyEntityToModel);
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
      return _Repository.TryUpdateEntityFields(fields);
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
    public Dictionary<string, object> TryUpdateEntity(Dictionary<string, object> entity) {
      return _Repository.TryUpdateEntity(
        entity.ConvertToEntityDynamic(_HandlePropertyModelToEntity)
      ).ConvertToBusinessModelDynamic(_HandlePropertyEntityToModel);
    }

    /// <summary>
    /// Adds an new entity and returns its Key on success, otherwise null
    /// (also if the entity is already exisiting).
    /// Depending on the concrete repository implementation the KEY properties
    /// of the entity needs be pre-initialized (see the 'RequiresExternalKeys'-Capability). 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>The entity key on success, otherwise null</returns>
    public object TryAddEntity(Dictionary<string, object> entity) {
      return _Repository.TryAddEntity(
        entity.ConvertToEntityDynamic(_HandlePropertyModelToEntity)
      );
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
    public object[] MassupdateByKeys(object[] keysToUpdate, Dictionary<string, object> fields) {
      return _Repository.MassupdateByKeys(
        keysToUpdate.Cast<TKey>().ToArray(), fields
      ).Cast<object>().ToArray();
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
    public object[] Massupdate(ExpressionTree filter, Dictionary<string, object> fields) {
      return _Repository.Massupdate(filter, fields).Cast<object>().ToArray();
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
    public object[] MassupdateBySearchExpression(string searchExpression, Dictionary<string, object> fields) {
      return _Repository.GetCapabilities().SupportsStringBasedSearchExpressions
        ? _Repository.MassupdateBySearchExpression(searchExpression, fields).Cast<object>().ToArray()
        : throw new NotSupportedException("SearchExpressions are not supported by the repository");
    }

    /// <summary>
    /// Tries to delete entities by the given keys und returns an array containing the keys of only that entities
    /// which were deleted successfully.
    /// NOTE: this method can only be used, if the 'CanDeleteEntities'-Capability is given for this repository! 
    /// </summary>
    /// <param name="keysToDelete"></param>
    /// <returns>keys of deleted entities</returns>
    public object[] TryDeleteEntities(object[] keysToDelete) {
      TKey[] keys = keysToDelete.Select((o) => {
#if NETCOREAPP
        if (o.GetType() == typeof(JsonElement)) {
          JsonElement je = (JsonElement)o;
          return je.Deserialize<TKey>();
        }
#endif
        return (TKey)o;
      }).ToArray();
      return _Repository.TryDeleteEntities(keys).Cast<object>().ToArray();
    }

    /// <summary>
    /// Changes the KEY for an entity.
    /// NOTE: this method can only be used, if the 'SupportsKeyUpdate'-Capability is given for this repository! 
    /// </summary>
    /// <param name="currentKey"></param>
    /// <param name="newKey"></param>
    /// <returns></returns>
    public bool TryUpdateKey(object currentKey, object newKey) {
      return _Repository.TryUpdateKey((TKey)currentKey, (TKey)newKey);
    }

  }
}
