using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;

namespace System.Data.Fuse.Convenience {

  public abstract class UniversalRepositoryFacade {

    /// <summary>
    /// Returns an string, representing the "Identity" of the current origin.
    /// This can be used to discriminate multiple source repos.
    /// (usually it should be related to a SCOPE like {DbServer}+{DbName/Schema}+{EntityName})
    /// NOTE: This is an technical disciminator and it is not required, that it is an human-readable
    /// "frieldly-name". It can just be an Hash or Uid, so its NOT RECOMMENDED to use it as display label!
    /// </summary>
    public abstract string GetOriginIdentity();

    /// <summary>
    /// Returns an property bag which holds information about the implemented/supported capabilities of this IRepository.
    /// </summary>
    /// <returns></returns>
    public abstract RepositoryCapabilities GetCapabilities();

    public abstract string[] GetEntityNames();

    /// <summary> </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="filter"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public abstract EntityRef[] GetEntityRefs(
      string entityName, ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    );

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="searchExpression"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public abstract EntityRef[] GetEntityRefsBySearchExpression(
      string entityName, string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    );

    public abstract EntityRef[] GetEntityRefsByKey(
      string entityName, object[] keysToLoad
    );

    /// <summary> </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="filter"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public abstract object[] GetEntities(
      string entityName, ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    );

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="searchExpression"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public abstract object[] GetEntitiesBySearchExpression(
      string entityName, string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    );

    /// <summary> </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="keysToLoad"></param>
    /// <returns></returns>
    public abstract object[] GetEntitiesByKey(
      string entityName, object[] keysToLoad
    );

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
    public abstract Dictionary<string, object>[] GetEntityFields(
      string entityName, ExpressionTree filter, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0
    );

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
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
      string entityName, string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0
    );

    public abstract Dictionary<string, object>[] GetEntityFieldsByKey(
      string entityName, object[] keysToLoad, string[] includedFieldNames
    );

    public abstract int CountAll(string entityName);

    public abstract int Count(string entityName, ExpressionTree filter);

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="searchExpression"></param>
    /// <returns></returns>
    public abstract int CountBySearchExpression(string entityName, string searchExpression);

    public abstract bool ContainsKey(string entityName, object key);

    /// <summary>
    /// Creates an new Entity or Updates the given set of fields for an entity and 
    /// returns all fields that ARE DIFFERENT from the given values or null, if the entity wasn't found.
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
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
    public abstract Dictionary<string, object> AddOrUpdateEntityFields(string entityName, Dictionary<string, object> fields);

    /// <summary>
    /// Creates an new Entity or Updates the given set of fields for an entity and 
    /// returns the entity within its new state or null, if the entity wasn't found.
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
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
    public abstract object AddOrUpdateEntity(string entityName, object entity);

    /// <summary>
    /// Updates the given set of fields for an entity and 
    /// returns all fields that ARE DIFFERENT from the given values or null, if the entity wasn't found.
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="fields">
    /// NOTE: The target entity to be updated will be addressed by the key values used from this param!
    /// If the given dictionary does not contain the key fiels, an exception will be thrown!
    /// </param>
    /// All fields that ARE DIFFERENT from the given values or null, if the entity wasn't found.
    /// NOTE: the returned entity can differ from the given one, because in some cases a field
    /// (1) was not updated,
    /// (2) was updated using normlized (=modified) value that differs from the given one,
    /// (3) was updated implicitely (timestamp's,rowversion's,...) 
    public abstract Dictionary<string, object> TryUpdateEntityFields(string entityName, Dictionary<string, object> fields);

    /// <summary>
    /// Updates all updatable fields for an entity and 
    /// returns the entity within its new state or null, if the entity wasn't found.
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
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
    public abstract object TryUpdateEntity(string entityName, object entity);

    /// <summary>
    /// Adds an new entity and returns its Key on success, otherwise null
    /// (also if the entity is already exisiting).
    /// Depending on the concrete repository implementation the KEY properties
    /// of the entity needs be pre-initialized (see the 'RequiresExternalKeys'-Capability). 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="entity"></param>
    /// <returns>The entity key on success, otherwise null</returns>
    public abstract object TryAddEntity(string entityName, object entity);

    /// <summary>
    /// Updates a dedicated subset of fields for all addressed entites and
    /// returns an array containing the keys of affeced entities. 
    /// NOTE: this method can only be used, if the 'SupportsMassupdate'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="keysToUpdate">Keys for that entities, which sould be updated (non exisiting keys will be ignored).</param>
    /// <param name="fields">A set of fields and value, that should be update.
    /// It MUST NOT contain fields which are part of the Key, otherwise an exception will be thrown!
    /// </param>
    /// <returns>An array containing the keys of affeced entities.</returns>
    public abstract object[] Massupdate(string entityName, object[] keysToUpdate, Dictionary<string, object> fields);

    /// <summary>
    /// Updates a dedicated subset of fields for all addressed entites and
    /// returns an array containing the keys of affeced entities. 
    /// NOTE: this method can only be used, if the 'SupportsMassupdate'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="filter">A filter to adress that entities, which sould be updated.</param>
    /// <param name="fields">A set of fields and value, that should be update.
    /// It MUST NOT contain fields which are part of the Key, otherwise an exception will be thrown!
    /// </param>
    /// <returns></returns>
    public abstract object[] Massupdate(string entityName, ExpressionTree filter, Dictionary<string, object> fields);

    /// <summary>
    /// Updates a dedicated subset of fields for all addressed entites and
    /// returns an array containing the keys of affeced entities. 
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability and the
    /// 'SupportsMassupdate'-Capability are given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="searchExpression">A search expression to adress that entities, which sould be updated.</param>
    /// <param name="fields">A set of fields and value, that should be update.
    /// It MUST NOT contain fields which are part of the Key, otherwise an exception will be thrown!
    /// </param>
    /// <returns></returns>
    public abstract object[] Massupdate(string entityName, string searchExpression, Dictionary<string, object> fields);

    /// <summary>
    /// Tries to delete entities by the given keys und returns an array containing the keys of only that entities
    /// which were deleted successfully.
    /// NOTE: this method can only be used, if the 'CanDeleteEntities'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="keysToDelete"></param>
    /// <returns>keys of deleted entities</returns>
    public abstract object[] TryDeleteEntities(string entityName, object[] keysToDelete);

    /// <summary>
    /// Changes the KEY for an entity.
    /// NOTE: this method can only be used, if the 'SupportsKeyUpdate'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="currentKey"></param>
    /// <param name="newKey"></param>
    /// <returns></returns>
    public abstract bool TryUpdateKey(string entityName, object currentKey, object newKey);

  }

  public class DynamicRepositoryFacade<TEntity, TKey>  
    : UniversalRepositoryFacade
    where TEntity : class{

    private KvpModelVsEntityRepository<TEntity, TKey> _InternalRepo;

    public DynamicRepositoryFacade(
      IRepository<TEntity, TKey> internalRepo,
      Func<PropertyInfo, bool> isForeignKey,
      Func<PropertyInfo, bool> isNavigation
    ) {
      _InternalRepo = new KvpModelVsEntityRepository<TEntity, TKey>( internalRepo, isForeignKey, isNavigation);
    }




    //public override Dictionary<string, object> AddOrUpdateEntity(Dictionary<string, object> entity) {
    //  return _InternalRepo.AddOrUpdateEntity(entity);
    //}

    //public override void DeleteEntities(object[][] entityIdsToDelete) {
    //  _InternalRepo.DeleteEntities(entityIdsToDelete);
    //}

    //public override IList<Dictionary<string, object>> GetEntities(
    //  ExpressionTree filter, PagingParams pagingParams, SortingField[] sortingParams
    //) {
    //  return _InternalRepo.GetEntities(filter, pagingParams, sortingParams);
    //}

    //public override IList<Dictionary<string, object>> GetEntities(
    //  string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    //) {
    //  return _InternalRepo.GetEntities(dynamicLinqFilter, pagingParams, sortingParams);
    //}

    //public override IList<EntityRef> GetEntityRefs(
    //  ExpressionTree filter, PagingParams pagingParams, SortingField[] sortingParams
    //) {      
    //  return _InternalRepo.GetEntityRefs(filter, pagingParams, sortingParams).ToList();
    //}

    //public override IList<EntityRef> GetEntityRefs(
    //  string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    //) {
    //  return _InternalRepo.GetEntityRefs(dynamicLinqFilter, pagingParams, sortingParams).ToList();
    //}

    //public override int GetCount(ExpressionTree filter) {
    //  return _InternalRepo.GetCount(filter);
    //}

    //public override int GetCount(string dynamicLinqFilter) {
    //  return _InternalRepo.GetCount(dynamicLinqFilter);
    //}






    /// <summary>
    /// Returns an string, representing the "Identity" of the current origin.
    /// This can be used to discriminate multiple source repos.
    /// (usually it should be related to a SCOPE like {DbServer}+{DbName/Schema}+{EntityName})
    /// NOTE: This is an technical disciminator and it is not required, that it is an human-readable
    /// "frieldly-name". It can just be an Hash or Uid, so its NOT RECOMMENDED to use it as display label!
    /// </summary>
    public override string GetOriginIdentity() {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Returns an property bag which holds information about the implemented/supported capabilities of this IRepository.
    /// </summary>
    /// <returns></returns>
    public override RepositoryCapabilities GetCapabilities() {
      throw new NotImplementedException();
    }

    public override string[] GetEntityNames() {
      throw new NotImplementedException();
    }

    /// <summary> </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="filter"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public override EntityRef[] GetEntityRefs(
      string entityName, ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="searchExpression"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public override EntityRef[] GetEntityRefsBySearchExpression(
      string entityName, string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      throw new NotImplementedException();
    }

    public override EntityRef[] GetEntityRefsByKey(
      string entityName, object[] keysToLoad
    ) {
      throw new NotImplementedException();
    }

    /// <summary> </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="filter"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public override object[] GetEntities(
      string entityName, ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="searchExpression"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    public override object[] GetEntitiesBySearchExpression(
      string entityName, string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      throw new NotImplementedException();
    }

    /// <summary> </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="keysToLoad"></param>
    /// <returns></returns>
    public override object[] GetEntitiesByKey(
      string entityName, object[] keysToLoad
    ) {
      throw new NotImplementedException();
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
    public override Dictionary<string, object>[] GetEntityFields(
      string entityName, ExpressionTree filter, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
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
    public override Dictionary<string, object>[] GetEntityFieldsBySearchExpression(
      string entityName, string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      throw new NotImplementedException();
    }

    public override Dictionary<string, object>[] GetEntityFieldsByKey(
      string entityName, object[] keysToLoad, string[] includedFieldNames
    ) {
      throw new NotImplementedException();
    }

    public override int CountAll(string entityName) {
      throw new NotImplementedException();
    }

    public override int Count(string entityName, ExpressionTree filter) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="searchExpression"></param>
    /// <returns></returns>
    public override int CountBySearchExpression(string entityName, string searchExpression) {
      throw new NotImplementedException();
    }

    public override bool ContainsKey(string entityName, object key) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Creates an new Entity or Updates the given set of fields for an entity and 
    /// returns all fields that ARE DIFFERENT from the given values or null, if the entity wasn't found.
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
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
    public override Dictionary<string, object> AddOrUpdateEntityFields(string entityName, Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Creates an new Entity or Updates the given set of fields for an entity and 
    /// returns the entity within its new state or null, if the entity wasn't found.
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
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
    public override object AddOrUpdateEntity(string entityName, object entity) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Updates the given set of fields for an entity and 
    /// returns all fields that ARE DIFFERENT from the given values or null, if the entity wasn't found.
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="fields">
    /// NOTE: The target entity to be updated will be addressed by the key values used from this param!
    /// If the given dictionary does not contain the key fiels, an exception will be thrown!
    /// </param>
    /// All fields that ARE DIFFERENT from the given values or null, if the entity wasn't found.
    /// NOTE: the returned entity can differ from the given one, because in some cases a field
    /// (1) was not updated,
    /// (2) was updated using normlized (=modified) value that differs from the given one,
    /// (3) was updated implicitely (timestamp's,rowversion's,...) 
    public override Dictionary<string, object> TryUpdateEntityFields(string entityName, Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Updates all updatable fields for an entity and 
    /// returns the entity within its new state or null, if the entity wasn't found.
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
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
    public override object TryUpdateEntity(string entityName, object entity) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Adds an new entity and returns its Key on success, otherwise null
    /// (also if the entity is already exisiting).
    /// Depending on the concrete repository implementation the KEY properties
    /// of the entity needs be pre-initialized (see the 'RequiresExternalKeys'-Capability). 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="entity"></param>
    /// <returns>The entity key on success, otherwise null</returns>
    public override object TryAddEntity(string entityName, object entity) {
      throw new NotImplementedException();
    }

    #region " MASSUPDATE "

    /// <summary>
    /// Updates a dedicated subset of fields for all addressed entites and
    /// returns an array containing the keys of affeced entities. 
    /// NOTE: this method can only be used, if the 'SupportsMassupdate'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="keysToUpdate">Keys for that entities, which sould be updated (non exisiting keys will be ignored).</param>
    /// <param name="fields">A set of fields and value, that should be update.
    /// It MUST NOT contain fields which are part of the Key, otherwise an exception will be thrown!
    /// </param>
    /// <returns>An array containing the keys of affeced entities.</returns>
    public override object[] Massupdate(string entityName, object[] keysToUpdate, Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Updates a dedicated subset of fields for all addressed entites and
    /// returns an array containing the keys of affeced entities. 
    /// NOTE: this method can only be used, if the 'SupportsMassupdate'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="filter">A filter to adress that entities, which sould be updated.</param>
    /// <param name="fields">A set of fields and value, that should be update.
    /// It MUST NOT contain fields which are part of the Key, otherwise an exception will be thrown!
    /// </param>
    /// <returns></returns>
    public override object[] Massupdate(string entityName, ExpressionTree filter, Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Updates a dedicated subset of fields for all addressed entites and
    /// returns an array containing the keys of affeced entities. 
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability and the
    /// 'SupportsMassupdate'-Capability are given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="searchExpression">A search expression to adress that entities, which sould be updated.</param>
    /// <param name="fields">A set of fields and value, that should be update.
    /// It MUST NOT contain fields which are part of the Key, otherwise an exception will be thrown!
    /// </param>
    /// <returns></returns>
    public override object[] Massupdate(string entityName, string searchExpression, Dictionary<string, object> fields) {
      throw new NotImplementedException();
    }

    #endregion

    /// <summary>
    /// Tries to delete entities by the given keys und returns an array containing the keys of only that entities
    /// which were deleted successfully.
    /// NOTE: this method can only be used, if the 'CanDeleteEntities'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="keysToDelete"></param>
    /// <returns>keys of deleted entities</returns>
    public override object[] TryDeleteEntities(string entityName, object[] keysToDelete) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Changes the KEY for an entity.
    /// NOTE: this method can only be used, if the 'SupportsKeyUpdate'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="currentKey"></param>
    /// <param name="newKey"></param>
    /// <returns></returns>
    public override bool TryUpdateKey(string entityName, object currentKey, object newKey) {
      throw new NotImplementedException();
    }

  }

}