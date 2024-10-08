using System.Collections.Generic;
using System.Linq;
#if NETCOREAPP
using System.Text.Json;
#endif

namespace System.Data.Fuse.Convenience.Internal {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public abstract class RepositoryUntypingFacade {

    /// <summary> </summary>
    /// <param name="filter"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public abstract EntityRef[] GetEntityRefs(
      ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    );

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
    public abstract EntityRef[] GetEntityRefsBySearchExpression(
       string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    );

    public abstract EntityRef[] GetEntityRefsByKey(
      object[] keysToLoad
    );

    /// <summary> </summary>
    /// <param name="filter"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public abstract object[] GetEntities(
      ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    );

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
    public abstract object[] GetEntitiesBySearchExpression(
      string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    );

    /// <summary> </summary>
    /// <param name="keysToLoad"></param>
    /// <returns></returns>
    public abstract object[] GetEntitiesByKey(
      object[] keysToLoad
    );

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
    public abstract Dictionary<string, object>[] GetEntityFields(
      ExpressionTree filter, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0
    );

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
    public abstract Dictionary<string, object>[] GetEntityFieldsBySearchExpression(
      string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0
    );

    public abstract Dictionary<string, object>[] GetEntityFieldsByKey(
      object[] keysToLoad, string[] includedFieldNames
    );

    public abstract int CountAll();

    public abstract int Count(ExpressionTree filter);

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="searchExpression"></param>
    /// <returns></returns>
    public abstract int CountBySearchExpression(string searchExpression);

    public abstract bool ContainsKey(object key);

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
    public abstract Dictionary<string, object> AddOrUpdateEntityFields(Dictionary<string, object> fields);

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
    public abstract object AddOrUpdateEntity(object entity);

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
    public abstract Dictionary<string, object> TryUpdateEntityFields(Dictionary<string, object> fields);

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
    public abstract object TryUpdateEntity(object entity);

    /// <summary>
    /// Adds an new entity and returns its Key on success, otherwise null
    /// (also if the entity is already exisiting).
    /// Depending on the concrete repository implementation the KEY properties
    /// of the entity needs be pre-initialized (see the 'RequiresExternalKeys'-Capability). 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>The entity key on success, otherwise null</returns>
    public abstract object TryAddEntity(object entity);

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
    public abstract object[] MassupdateByKeys(object[] keysToUpdate, Dictionary<string, object> fields);

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
    public abstract object[] Massupdate(ExpressionTree filter, Dictionary<string, object> fields);

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
    public abstract object[] MassupdateBySearchExpression(string searchExpression, Dictionary<string, object> fields);

    /// <summary>
    /// Tries to delete entities by the given keys und returns an array containing the keys of only that entities
    /// which were deleted successfully.
    /// NOTE: this method can only be used, if the 'CanDeleteEntities'-Capability is given for this repository! 
    /// </summary>
    /// <param name="keysToDelete"></param>
    /// <returns>keys of deleted entities</returns>
    public abstract object[] TryDeleteEntities(object[] keysToDelete);

    /// <summary>
    /// Changes the KEY for an entity.
    /// NOTE: this method can only be used, if the 'SupportsKeyUpdate'-Capability is given for this repository! 
    /// </summary>
    /// <param name="currentKey"></param>
    /// <param name="newKey"></param>
    /// <returns></returns>
    public abstract bool TryUpdateKey(object currentKey, object newKey);

  }

  public class DynamicRepositoryFacade<TEntity, TKey> : RepositoryUntypingFacade
    where TEntity : class {
    private IRepository<TEntity, TKey> _InternalRepo;

    public DynamicRepositoryFacade() {
    }

    public DynamicRepositoryFacade(IRepository<TEntity, TKey> internalRepo) {
      this._InternalRepo = internalRepo;
    }

    public override EntityRef[] GetEntityRefs(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      return _InternalRepo.GetEntityRefs(filter, sortedBy, limit, skip).ToArray();
    }

    public override EntityRef[] GetEntityRefsBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      return _InternalRepo.GetEntityRefsBySearchExpression(searchExpression, sortedBy, limit, skip).ToArray();
    }

    public override EntityRef[] GetEntityRefsByKey(object[] keysToLoad) {
      return _InternalRepo.GetEntityRefsByKey(keysToLoad.Cast<TKey>().ToArray()).ToArray();
    }

    public override object[] GetEntities(ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0) {
      return _InternalRepo.GetEntities(filter, sortedBy, limit, skip).ToArray();
    }

    public override object[] GetEntitiesBySearchExpression(string searchExpression, string[] sortedBy, int limit = 100, int skip = 0) {
      return _InternalRepo.GetEntitiesBySearchExpression(searchExpression, sortedBy, limit, skip).ToArray();
    }

    public override object[] GetEntitiesByKey(object[] keysToLoad) {
      return _InternalRepo.GetEntitiesByKey(keysToLoad.Cast<TKey>().ToArray()).ToArray();
    }

    public override Dictionary<string, object>[] GetEntityFields(ExpressionTree filter, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      return _InternalRepo.GetEntityFields(filter, includedFieldNames, sortedBy, limit, skip).ToArray();
    }

    public override Dictionary<string, object>[] GetEntityFieldsBySearchExpression(string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0) {
      return _InternalRepo.GetEntityFieldsBySearchExpression(searchExpression, includedFieldNames, sortedBy, limit, skip).ToArray();
    }

    public override Dictionary<string, object>[] GetEntityFieldsByKey(object[] keysToLoad, string[] includedFieldNames) {
      return _InternalRepo.GetEntityFieldsByKey(keysToLoad.Cast<TKey>().ToArray(), includedFieldNames).ToArray();
    }

    public override int CountAll() {
      return _InternalRepo.CountAll();
    }

    public override int Count(ExpressionTree filter) {
      return _InternalRepo.Count(filter);
    }

    public override int CountBySearchExpression(string searchExpression) {
      return _InternalRepo.CountBySearchExpression(searchExpression);
    }

    public override bool ContainsKey(object key) {
      return _InternalRepo.ContainsKey((TKey)key);
    }

    public override Dictionary<string, object> AddOrUpdateEntityFields(Dictionary<string, object> fields) {
      return _InternalRepo.AddOrUpdateEntityFields(fields);
    }

    public override object AddOrUpdateEntity(object entity) {
      return _InternalRepo.AddOrUpdateEntity((TEntity)ConversionHelper.SanitizeObject(entity, typeof(TEntity)));
    }

    public override Dictionary<string, object> TryUpdateEntityFields(Dictionary<string, object> fields) {
      return _InternalRepo.TryUpdateEntityFields(fields);
    }

    public override object TryUpdateEntity(object entity) {
      return _InternalRepo.TryUpdateEntity((TEntity)entity);
    }

    public override object TryAddEntity(object entity) {
      return _InternalRepo.TryAddEntity((TEntity)entity);
    }
    public override object[] Massupdate(ExpressionTree filter, Dictionary<string, object> fields) {
      return _InternalRepo.Massupdate(filter, fields).Cast<object>().ToArray();
    }

    public override object[] MassupdateByKeys(object[] keysToUpdate, Dictionary<string, object> fields) {
      return _InternalRepo.MassupdateByKeys(keysToUpdate.Cast<TKey>().ToArray(), fields).Cast<object>().ToArray();
    }

    public override object[] MassupdateBySearchExpression(string searchExpression, Dictionary<string, object> fields) {
      return _InternalRepo.MassupdateBySearchExpression(searchExpression, fields).Cast<object>().ToArray();
    }

    public override object[] TryDeleteEntities(object[] keysToDelete) {
      return _InternalRepo.TryDeleteEntities(keysToDelete.Cast<TKey>().ToArray()).Cast<object>().ToArray();
    }

    public override bool TryUpdateKey(object currentKey, object newKey) {
      TKey current = (TKey)currentKey;
      TKey newK = (TKey)newKey;
      return _InternalRepo.TryUpdateKey(current, newK);
    }
  }

}