using System.Collections.Generic;
using System.Data.Fuse.Convenience;

namespace System.Data.Fuse {

  public abstract class UniversalRepository : IUniversalRepository {

    private Dictionary<string, UniversalRepositoryFacade> _InnerRepos = new Dictionary<string, UniversalRepositoryFacade>();

    protected UniversalRepositoryFacade GetInnerRepo(string entityName) {
      if (!_InnerRepos.ContainsKey(entityName)) {
        _InnerRepos.Add(entityName, CreateInnerRepo(entityName));
      }
      return _InnerRepos[entityName];
    }

    protected abstract UniversalRepositoryFacade CreateInnerRepo(string entityName);

    /// <summary>
    /// Returns an string, representing the "Identity" of the current origin.
    /// This can be used to discriminate multiple source repos.
    /// (usually it should be related to a SCOPE like {DbServer}+{DbName/Schema}+{EntityName})
    /// NOTE: This is an technical disciminator and it is not required, that it is an human-readable
    /// "frieldly-name". It can just be an Hash or Uid, so its NOT RECOMMENDED to use it as display label!
    /// </summary>
    public string GetOriginIdentity() { 
      throw new NotImplementedException();
    }

    /// <summary>
    /// Returns an property bag which holds information about the implemented/supported capabilities of this IRepository.
    /// </summary>
    /// <returns></returns>
    public RepositoryCapabilities GetCapabilities() {
      throw new NotImplementedException();
    }

    public string[] GetEntityNames() {
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
    public EntityRef[] GetEntityRefs(
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
    public EntityRef[] GetEntityRefsBySearchExpression(
      string entityName, string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      throw new NotImplementedException();
    }

    public EntityRef[] GetEntityRefsByKey(
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
    public object[] GetEntities(
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
    public object[] GetEntitiesBySearchExpression(
      string entityName, string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      throw new NotImplementedException();
    }

    /// <summary> </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="keysToLoad"></param>
    /// <returns></returns>
    public object[] GetEntitiesByKey(
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
    public Dictionary<string, object>[] GetEntityFields(
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
    public Dictionary<string, object>[] GetEntityFieldsBySearchExpression(
      string entityName, string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0
    ) {
      throw new NotImplementedException();
    }

    public Dictionary<string, object>[] GetEntityFieldsByKey(
      string entityName, object[] keysToLoad, string[] includedFieldNames
    ) {
      throw new NotImplementedException();
    }

    public int CountAll(string entityName) {
      throw new NotImplementedException();
    }

    public int Count(string entityName, ExpressionTree filter) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    /// </summary>
    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    /// <param name="searchExpression"></param>
    /// <returns></returns>
    public int CountBySearchExpression(string entityName, string searchExpression) {
      throw new NotImplementedException();
    }

    public bool ContainsKey(string entityName, object key) {
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
    public Dictionary<string, object> AddOrUpdateEntityFields(string entityName, Dictionary<string, object> fields) {
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
    public object AddOrUpdateEntity(string entityName, object entity) {
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
    public Dictionary<string, object> TryUpdateEntityFields(string entityName, Dictionary<string, object> fields) {
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
    public object TryUpdateEntity(string entityName, object entity) {
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
    public object TryAddEntity(string entityName, object entity) {
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
    public object[] Massupdate(string entityName, object[] keysToUpdate, Dictionary<string, object> fields) {
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
    public object[] Massupdate(string entityName, ExpressionTree filter, Dictionary<string, object> fields) {
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
    public object[] Massupdate(string entityName, string searchExpression, Dictionary<string, object> fields) {
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
    public object[] TryDeleteEntities(string entityName, object[] keysToDelete) {
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
    public bool TryUpdateKey(string entityName, object currentKey, object newKey) {
      throw new NotImplementedException();
    }

  }

}
