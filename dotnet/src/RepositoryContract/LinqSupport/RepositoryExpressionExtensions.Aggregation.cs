using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Fuse.Convenience;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;

namespace System.Data.Fuse.LinqSupport {

  public static partial class RepositoryExpressionExtensions {

    //TODO: das alles nochmal zur perfomance-optimierung durch die KI analysieren lassen - da ist potential!

    #region " AggregateSingle "

    /// <summary>
    /// Performs an in-memory join to a foreign repository, which (as far as in-memory is possible) contains some
    /// optimization, specifically by pre-resoliving the foreign entitiy-keys and loading them (distinct) in a single call.
    /// Note: Creating aggregated entities by the given aggregationSelector will be processed locally after materialization.
    /// </summary>
    /// <typeparam name="TReferringEntity"></typeparam>
    /// <typeparam name="TReferredEntity"></typeparam>
    /// <typeparam name="TForeignKey"></typeparam>
    /// <typeparam name="TAggregatedEntity"></typeparam>
    /// <param name="referringEntities"></param>
    /// <param name="repository"></param>
    /// <param name="foreignKeySelector">The Foreign Key Field(s) on the referring entity (present in the source array)</param>
    /// <param name="aggregationSelector">Method to build-up the aggreagated result...</param>
    /// <param name="brokenIntegrityHandler">
    /// Used to specify the behaviour when a foreign key could not be resolved.
    /// Mostly common is one of the following:
    ///   a) return false to skip the item (will not be present within the result array any more)
    ///   b) return true to keep the item item (may be in addition with assigning dummy values to the 
    ///     'resolvedTarget' paramter in order to avoid exeptions inside of the 'aggregationSelector')
    ///   c) Throw an Exception,  
    /// </param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IEnumerable<TAggregatedEntity> AggregateSingle<TReferringEntity, TReferredEntity, TForeignKey, TAggregatedEntity>(
      this TReferringEntity[] referringEntities,
      IRepository<TReferredEntity, TForeignKey> repository,
      Func<TReferringEntity, TForeignKey> foreignKeySelector,
      Func<TReferringEntity, TReferredEntity, TAggregatedEntity> aggregationSelector,
      BrokenIntegrityHandlingDelegate<TForeignKey, TReferredEntity> brokenIntegrityHandler = null
    ) where TReferredEntity : class {

      Dictionary<TReferringEntity, TForeignKey> foreignKeysByReferringEntity = new Dictionary<TReferringEntity, TForeignKey>();
      foreach (var referringEntity in referringEntities) {
        TForeignKey foreignKey = foreignKeySelector(referringEntity);
        foreignKeysByReferringEntity[referringEntity] = foreignKey;
      }

      TForeignKey[] foreignKeysToLoad = foreignKeysByReferringEntity.Select(
        (kvp) => kvp.Value
      ).Distinct().ToArray();

      TReferredEntity[] foreignEntities = repository.GetEntitiesByKey(foreignKeysToLoad);

      if (foreignEntities.Length != foreignKeysToLoad.Length) {
        throw new InvalidOperationException("The given Repo does not act as expected, the returned Entity-Array has not the same length as the requested keys (Could be a BUG, because also for not-loadable entities, this is a mandatory convention!).");
      }

      return foreignKeysByReferringEntity.SelectOrSkip(
        (KeyValuePair<TReferringEntity, TForeignKey> kvp, ref TAggregatedEntity aggregatedEntity) => {

          TReferringEntity referringEntity = kvp.Key;
          TForeignKey fk = kvp.Value;
          TReferredEntity foreignEntity = null;

          int loadedForeignEntityIndex = Array.FindIndex(
            foreignKeysToLoad, (loadedKey) => EqualityComparer<TForeignKey>.Default.Equals(loadedKey, fk)
          );

          if (loadedForeignEntityIndex >= 0) {
            foreignEntity = foreignEntities[loadedForeignEntityIndex];
            // this relies on the convention, that also for not-loadable entities, a NULL is returned within the result array!
            if (foreignEntity == null) {
              if (!brokenIntegrityHandler.Invoke(fk, ref foreignEntity)) { //Exception possible -> DONT CATCH HERE!
                return false; //skip this item (will not be present within the result array any more)
              }
            }
          }

          aggregatedEntity = aggregationSelector.Invoke(referringEntity, foreignEntity);
          return true;
        }

      );

    }

    /// <summary>
    /// Performs an in-memory join to a foreign repository, which (as far as in-memory is possible) contains some
    /// optimization, specifically by pre-resoliving the foreign entitiy-keys and loading them (distinct) in a single call.
    /// </summary>
    /// <typeparam name="TReferringEntity"></typeparam>
    /// <typeparam name="TReferredEntity"></typeparam>
    /// <typeparam name="TForeignKey"></typeparam>
    /// <param name="referringEntities"></param>
    /// <param name="repository"></param>
    /// <param name="foreignKeySelector">The Foreign Key Field(s) on the referring entity (present in the source array)</param>
    /// <param name="brokenIntegrityHandler">
    /// Used to specify the behaviour when a foreign key could not be resolved.
    /// Mostly common is one of the following:
    ///   a) return false to skip the item (will not be present within the result array any more)
    ///   b) return true to keep the item item (may be in addition with assigning dummy values to the 
    ///     'resolvedTarget' paramter in order to avoid exeptions inside of the 'aggregationSelector')
    ///   c) Throw an Exception,  
    /// </param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IEnumerable<Tuple<TReferringEntity, TReferredEntity>> AggregateSingle<TReferringEntity, TReferredEntity, TForeignKey>(
      this TReferringEntity[] referringEntities,
      IRepository<TReferredEntity, TForeignKey> repository,
      Func<TReferringEntity, TForeignKey> foreignKeySelector,
      BrokenIntegrityHandlingDelegate<TForeignKey, TReferredEntity> brokenIntegrityHandler = null
    ) where TReferredEntity : class {

      return referringEntities.AggregateSingle(
        repository, foreignKeySelector,
        (referringEntity, foreignEntity) => Tuple.Create(referringEntity, foreignEntity),
        brokenIntegrityHandler
      );

    }

    #endregion

    #region " AggregateOptional "

    /// <summary>
    /// Performs an in-memory join to a foreign repository, which (as far as in-memory is possible) contains some
    /// optimization, specifically by pre-resoliving the foreign entitiy-keys and loading them (distinct) in a single call.
    /// Note: Creating aggregated entities by the given aggregationSelector will be processed locally after materialization.
    /// </summary>
    /// <typeparam name="TReferringEntity"></typeparam>
    /// <typeparam name="TReferredEntity"></typeparam>
    /// <typeparam name="TForeignKey"></typeparam>
    /// <typeparam name="TAggregatedEntity"></typeparam>
    /// <param name="referringEntities"></param>
    /// <param name="repository"></param>
    /// <param name="foreignKeySelector">The Foreign Key Field(s) on the referring entity (present in the source array)</param>
    /// <param name="aggregationSelector">Method to build-up the aggreagated result...</param>
    /// <param name="optionalityHandler">
    /// Used to specify the behaviour when an optional foreign key is NULL (as provided by the 'foreignKeySelector).
    /// Mostly common is one of the following:
    ///   a) return false to skip the item (will not be present within the result array any more)
    ///   b) return true to keep the item item (may be in addition with assigning dummy values to the 
    ///     'resolvedTarget' paramter in order to avoid exeptions inside of the 'aggregationSelector')
    /// </param>
    /// <param name="brokenIntegrityHandler">
    /// Used to specify the behaviour when a foreign key could not be resolved.
    /// Mostly common is one of the following:
    ///   a) return false to skip the item (will not be present within the result array any more)
    ///   b) return true to keep the item item (may be in addition with assigning dummy values to the 
    ///     'resolvedTarget' paramter in order to avoid exeptions inside of the 'aggregationSelector')
    ///   c) Throw an Exception,  
    /// </param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IEnumerable<TAggregatedEntity> AggregateOptional<TReferringEntity, TReferredEntity, TForeignKey, TAggregatedEntity>(
      this TReferringEntity[] referringEntities,
      IRepository<TReferredEntity, TForeignKey> repository,
      Func<TReferringEntity, TForeignKey> foreignKeySelector,
      Func<TReferringEntity, TReferredEntity, TAggregatedEntity> aggregationSelector,
      OptionalityHandlingDelegate<TReferredEntity> optionalityHandler,
      BrokenIntegrityHandlingDelegate<TForeignKey, TReferredEntity> brokenIntegrityHandler = null
    ) where TReferredEntity : class {

      Dictionary<TReferringEntity, TForeignKey> foreignKeysByReferringEntity = new Dictionary<TReferringEntity, TForeignKey>();
      foreach (var referringEntity in referringEntities) {
        TForeignKey foreignKey = foreignKeySelector(referringEntity);
        foreignKeysByReferringEntity[referringEntity] = foreignKey;
      }

      TForeignKey[] foreignKeysToLoad = foreignKeysByReferringEntity.Select(
        (kvp) => kvp.Value
      ).Where(
        (v) => KeyNotNull(v)
      ).Distinct().ToArray();

      TReferredEntity[] foreignEntities = repository.GetEntitiesByKey(foreignKeysToLoad);

      if (foreignEntities.Length != foreignKeysToLoad.Length) {
        throw new InvalidOperationException("The given Repo does not act as expected, the returned Entity-Array has not the same length as the requested keys (Could be a BUG, because also for not-loadable entities, this is a mandatory convention!).");
      }

      return foreignKeysByReferringEntity.SelectOrSkip(
        (KeyValuePair<TReferringEntity, TForeignKey> kvp, ref TAggregatedEntity aggregatedEntity) => {

          TReferringEntity referringEntity = kvp.Key;
          TForeignKey fk = kvp.Value;
          TReferredEntity foreignEntity = null;

          int loadedForeignEntityIndex = -1;
          if (KeyNotNull(fk)) {

            loadedForeignEntityIndex = Array.FindIndex(
              foreignKeysToLoad, (loadedKey) => EqualityComparer<TForeignKey>.Default.Equals(loadedKey, fk)
            );

            if (loadedForeignEntityIndex >= 0) {
              foreignEntity = foreignEntities[loadedForeignEntityIndex];
              // this relies on the convention, that also for not-loadable entities, a NULL is returned within the result array!
              if (foreignEntity == null) {
                if (!brokenIntegrityHandler.Invoke(fk, ref foreignEntity)) { //Exception possible -> DONT CATCH HERE!
                  return false; //skip this item (will not be present within the result array any more)
                }
              }
            }

          }
          else {
            if (!optionalityHandler.Invoke(ref foreignEntity)) {
              return false; //skip this item (will not be present within the result array any more)
            }
          }

          aggregatedEntity = aggregationSelector.Invoke(referringEntity, foreignEntity);
          return true;
        }
      );

    }

    /// <summary>
    /// Performs an in-memory join to a foreign repository, which (as far as in-memory is possible) contains some
    /// optimization, specifically by pre-resoliving the foreign entitiy-keys and loading them (distinct) in a single call.
    /// </summary>
    /// <typeparam name="TReferringEntity"></typeparam>
    /// <typeparam name="TReferredEntity"></typeparam>
    /// <typeparam name="TForeignKey"></typeparam>
    /// <param name="referringEntities"></param>
    /// <param name="repository"></param>
    /// <param name="foreignKeySelector">The Foreign Key Field(s) on the referring entity (present in the source array)</param>
    /// <param name="optionalityHandler">
    /// Used to specify the behaviour when an optional foreign key is NULL (as provided by the 'foreignKeySelector).
    /// Mostly common is one of the following:
    ///   a) return false to skip the item (will not be present within the result array any more)
    ///   b) return true to keep the item item (may be in addition with assigning dummy values to the 
    ///     'resolvedTarget' paramter in order to avoid exeptions inside of the 'aggregationSelector')
    /// </param>
    /// <param name="brokenIntegrityHandler">
    /// Used to specify the behaviour when a foreign key could not be resolved.
    /// Mostly common is one of the following:
    ///   a) return false to skip the item (will not be present within the result array any more)
    ///   b) return true to keep the item item (may be in addition with assigning dummy values to the 
    ///     'resolvedTarget' paramter in order to avoid exeptions inside of the 'aggregationSelector')
    ///   c) Throw an Exception,  
    /// </param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IEnumerable<Tuple<TReferringEntity, TReferredEntity>> AggregateOptional<TReferringEntity, TReferredEntity, TForeignKey>(
      this TReferringEntity[] referringEntities,
      IRepository<TReferredEntity, TForeignKey> repository,
      Func<TReferringEntity, TForeignKey> foreignKeySelector,
      OptionalityHandlingDelegate<TReferredEntity> optionalityHandler,
      BrokenIntegrityHandlingDelegate<TForeignKey, TReferredEntity> brokenIntegrityHandler = null
    ) where TReferredEntity : class {

      return referringEntities.AggregateOptional(
        repository, foreignKeySelector,
        (referringEntity, foreignEntity) => Tuple.Create(referringEntity, foreignEntity),
        optionalityHandler,
        brokenIntegrityHandler
      );

    }

    #endregion

    #region " AggregateMany "

    /// <summary>
    /// WARNING: This method is intentionally designed for resolving a few referenced entities as 'attachments' to the
    /// respective main entities in the source array. Everything is loaded simultaneously and merged in-memory.
    /// For reverse navigation over large amounts of data records, this method is NOT suitable -
    /// in this case: use clean iterator concepts (possibly with chunks) for mass processing or restructure your data access
    /// for forward navigation (see 'AggregateSingle' or 'AggregateOptional').
    /// </summary>
    /// <typeparam name="TReferredEntity"></typeparam>
    /// <typeparam name="TReferringEntity"></typeparam>
    /// <typeparam name="TReferringEntityKey"></typeparam>
    /// <typeparam name="TForeignKey"></typeparam>
    /// <typeparam name="TAggregatedEntity"></typeparam>
    /// <param name="referredEntities"></param>
    /// <param name="repository"></param>
    /// <param name="primaryKeySelector">The Primay Key Field(s) on the referred entity (present in the source array).</param>
    /// <param name="foreignKeySelector">The Foreign Key Field(s) on the referring entity (which will be loaded from the given repository).</param>
    /// <param name="aggregationSelector">Method to build-up the aggreagated result...</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IEnumerable<TAggregatedEntity> AggregateMany<TReferredEntity, TReferringEntity, TReferringEntityKey, TForeignKey, TAggregatedEntity>(
      this TReferredEntity[] referredEntities,
      IRepository<TReferringEntity, TReferringEntityKey> repository,
      Expression<Func<TReferringEntity, TForeignKey>> foreignKeySelector,
      Func<TReferredEntity, TForeignKey> primaryKeySelector,
      Func<TReferredEntity, IEnumerable<TReferringEntity>, TAggregatedEntity> aggregationSelector
    ) where TReferringEntity : class {

      TForeignKey[] foreignKeysToLoad = referredEntities.Select((e) => primaryKeySelector(e)).Distinct().ToArray();

      Expression<Func<TReferringEntity, bool>> containsExpressionWithSemanticOfIn = QueryExtensions.BuildInArrayPredicate(
        foreignKeySelector, foreignKeysToLoad
      );

      //load everthing with one single call
      TReferringEntity[] referringEntities = repository.GetEntitiesWhere(containsExpressionWithSemanticOfIn, limit: 2000);

      Func<TReferringEntity, TForeignKey> compiledforeignKeySelector = foreignKeySelector.Compile();

      foreach (TReferredEntity referredEntity in referredEntities) {

        TForeignKey key = primaryKeySelector(referredEntity);

        IEnumerable<TReferringEntity> foreignEntity = referringEntities.Where(
          (fe) => EqualityComparer<TForeignKey>.Default.Equals(compiledforeignKeySelector(fe), key)
        );

        yield return aggregationSelector.Invoke(referredEntity, foreignEntity);

      }

    }

    #endregion

    #region " Handling of NULL-/Broken-Integrity"

    /// <summary>
    /// Used to specify the behaviour when a foreign key could not be resolved.
    /// Mostly common is one of the following:
    ///   a) return false to skip the item (will not be present within the result array any more)
    ///   b) return true to keep the item item (may be in addition with assigning dummy values to the 
    ///     'resolvedTarget' paramter in order to avoid exeptions inside of the 'aggregationSelector')
    ///   c) Throw an Exception,   
    /// </summary>
    /// <typeparam name="TReferredEntity"></typeparam>
    /// <typeparam name="TForeignKey"></typeparam>
    /// <param name="nonExistingFkValue">
    ///  The given FK value for which no entity was present within the target repo.
    /// </param>
    /// <param name="dummyTargetToProvide">
    ///  Can be used to assign dummy values in order to avoid exeptions inside of the 'aggregationSelector'
    /// </param>
    /// <returns></returns>
    public delegate bool BrokenIntegrityHandlingDelegate<TForeignKey, TReferredEntity>(TForeignKey nonExistingFkValue, ref TReferredEntity dummyTargetToProvide);

    private static bool DefaultBrokenIntegrityHandler<TForeignKey, TReferredEntity>(TForeignKey fk, ref TReferredEntity dummyTargetToProvide) {
      throw new KeyNotFoundException($"Could not find a {typeof(TReferredEntity).Name} addressed by foreign key {fk}.");
    }

    /// <summary>
    /// Used to specify the behaviour when an optional foreign key is NULL (as provided by the 'foreignKeySelector).
    /// Mostly common is one of the following:
    ///   a) return false to skip the item (will not be present within the result array any more)
    ///   b) return true to keep the item item (may be in addition with assigning dummy values to the 
    ///     'resolvedTarget' paramter in order to avoid exeptions inside of the 'aggregationSelector')
    /// </summary>
    /// <typeparam name="TReferredEntity"></typeparam>
    /// <param name="dummyTargetToProvide">
    ///  Can be used to assign dummy values in order to avoid exeptions inside of the 'aggregationSelector'
    /// </param>
    /// <returns></returns>
    public delegate bool OptionalityHandlingDelegate<TReferredEntity>(ref TReferredEntity dummyTargetToProvide);

    private static bool DefaultOptionalityHandler<TReferredEntity>(ref TReferredEntity dummyTargetToProvide) {
      return true; // Keep the item with NULL foreign entity
    }

    #endregion

    #region " Helpers "

    internal delegate bool SelectorDelegate<TIn, TOut>(TIn item, ref TOut output);
    internal static IEnumerable<TOut> SelectOrSkip<TIn, TOut>(this IEnumerable<TIn> source, SelectorDelegate<TIn, TOut> selector) {
      foreach (TIn item in source) {
        TOut output = default(TOut);
        if (selector(item, ref output)) {
          yield return output;
        }
      }
    }

    internal static bool KeyNotNull<TKey>(TKey key) {

      if (key == null) {
        return false;
      }
      if (typeof(TKey).IsPrimitive) {
        return true;
      }

      //TODO: performance-optimierung und property-cahcing - für die komplexen fälle ab hier:

      Type keyType = typeof(TKey);
      if (keyType.IsGenericType && keyType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
        var hasValueProp = keyType.GetProperty("HasValue");
        if (hasValueProp != null) {
          bool hasValue = (bool)hasValueProp.GetValue(key);
          if (!hasValue) {
            return false;
          }
          else {
            return true;
          }
        }
      }

      var properties = keyType.GetProperties();
      foreach (var prop in properties) {
        var propValue = prop.GetValue(key);
        if (!KeyNotNull(propValue)) {
          return false;
        }
      }

      return true;
    }

    #endregion

    #region " !!! EXPERIMENTAL !!! "

    /// <summary>
    /// Performs an in-memory join to a foreign repository.
    /// Optimizations:
    /// - Pre-resolves distinct foreign keys
    /// - Loads foreign entities in a single repository call
    /// - Builds a dictionary lookup for O(1) mapping per referring entity
    /// </summary>
    /// <typeparam name="TReferringEntity">Type of the referring entity.</typeparam>
    /// <typeparam name="TReferredEntity">Type of the foreign entity.</typeparam>
    /// <typeparam name="TForeignKey">Type of the foreign key.</typeparam>
    /// <param name="referringEntities">Entities that hold a foreign key.</param>
    /// <param name="repository">Repository to load foreign entities by key.</param>
    /// <param name="foreignKeySelector">Selects the foreign key from a referring entity.</param>
    /// <returns>Array of pairs (referringEntity, foreignEntity). foreignEntity may be null if not found.</returns>
    /// <exception cref="ArgumentNullException">If any parameter is null.</exception>
    /// <exception cref="InvalidOperationException">If repository returns an unexpected number of entities.</exception>
    internal static Tuple<TReferringEntity, TReferredEntity>[] AggregateSingle_OptimizedExperimental<TReferringEntity, TReferredEntity, TForeignKey>(
      this TReferringEntity[] referringEntities,
      IRepository<TReferredEntity, TForeignKey> repository,
      Func<TReferringEntity, TForeignKey> foreignKeySelector
    ) where TReferredEntity : class {

      if (referringEntities == null) {
        throw new ArgumentNullException(nameof(referringEntities));
      }
      if (repository == null) {
        throw new ArgumentNullException(nameof(repository));
      }
      if (foreignKeySelector == null) {
        throw new ArgumentNullException(nameof(foreignKeySelector));
      }

      if (referringEntities.Length == 0) {
        return Array.Empty<Tuple<TReferringEntity, TReferredEntity>>();
      }

      // 1) Collect foreign keys per referring entity (parallel arrays, no dictionary needed)
      TForeignKey[] foreignKeysByIndex = new TForeignKey[referringEntities.Length];

      // 2) Build distinct key set
      HashSet<TForeignKey> distinctKeys = new HashSet<TForeignKey>(EqualityComparer<TForeignKey>.Default);

      for (int i = 0; i < referringEntities.Length; i++) {
        TReferringEntity referringEntity = referringEntities[i];

        // If you need to guard against null referring entities, uncomment:
        // if (referringEntity == null) { continue; }

        TForeignKey foreignKey = foreignKeySelector(referringEntity);
        foreignKeysByIndex[i] = foreignKey;
        distinctKeys.Add(foreignKey);
      }

      // 3) Materialize distinct keys to array
      TForeignKey[] foreignKeysToLoad = new TForeignKey[distinctKeys.Count];
      distinctKeys.CopyTo(foreignKeysToLoad, 0);

      // 4) Load in one call
      TReferredEntity[] foreignEntities = repository.GetEntitiesByKey(foreignKeysToLoad);

      // NOTE: This length check is only valid if the repository contract guarantees
      // "one result for each key in the same order".
      if (foreignEntities == null) {
        throw new InvalidOperationException("Repository returned null foreign entity array.");
      }
      if (foreignEntities.Length != foreignKeysToLoad.Length) {
        throw new InvalidOperationException("Not all foreign entities could be loaded!");
      }

      // 5) Build O(1) lookup: key -> entity
      Dictionary<TForeignKey, TReferredEntity> foreignByKey =
        new Dictionary<TForeignKey, TReferredEntity>(foreignKeysToLoad.Length, EqualityComparer<TForeignKey>.Default);

      for (int i = 0; i < foreignKeysToLoad.Length; i++) {
        TForeignKey key = foreignKeysToLoad[i];
        TReferredEntity entity = foreignEntities[i];

        // If duplicate keys somehow exist, last one wins.
        foreignByKey[key] = entity;
      }

      // 6) Project results in original order
      Tuple<TReferringEntity, TReferredEntity>[] result = new Tuple<TReferringEntity, TReferredEntity>[referringEntities.Length];

      for (int i = 0; i < referringEntities.Length; i++) {
        TReferringEntity referringEntity = referringEntities[i];
        TForeignKey fk = foreignKeysByIndex[i];

        TReferredEntity foreignEntity;
        bool found = foreignByKey.TryGetValue(fk, out foreignEntity);

        if (!found) {
          foreignEntity = null;
        }

        result[i] = Tuple.Create(referringEntity, foreignEntity);
      }

      return result;

    }

    //    /// <summary> </summary>
    //    /// <param name="filter"></param>
    //    /// <param name="sortedBy">
    //    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    //    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    //    /// </param>
    //    /// <param name="limit"></param>
    //    /// <param name="skip"></param>
    //    /// <returns></returns>
    //    public static EntityRef[] GetEntityRefs<TEntity>(
    //      this IUniversalRepository repo, ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    //    ) {
    //      return repo.GetEntityRefs(typeof(TEntity).Name, filter, sortedBy, limit, skip);
    //    }

    //    /// <summary>
    //    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    //    /// </summary>
    //    /// <param name="searchExpression"></param>
    //    /// <param name="sortedBy">
    //    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    //    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    //    /// </param>
    //    /// <param name="limit"></param>
    //    /// <param name="skip"></param>
    //    /// <returns></returns>
    //    public static EntityRef[] GetEntityRefsBySearchExpression<TEntity>(
    //      this IUniversalRepository repo, string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    //    ) {
    //      return repo.GetEntityRefsBySearchExpression(
    //        typeof(TEntity).Name, searchExpression, sortedBy, limit, skip
    //      );
    //    }

    //    public static EntityRef[] GetEntityRefsByKey<TEntity>(
    //      this IUniversalRepository repo, object[] keysToLoad
    //    ) {
    //      return repo.GetEntityRefsByKey(typeof(TEntity).Name, keysToLoad);
    //    }

    //    /// <summary> </summary>
    //    /// <param name="filter"></param>
    //    /// <param name="sortedBy">
    //    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    //    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    //    /// </param>
    //    /// <param name="limit"></param>
    //    /// <param name="skip"></param>
    //    /// <returns></returns>
    //    public static TEntity[] GetEntities<TEntity>(
    //      this IUniversalRepository repo, ExpressionTree filter, string[] sortedBy, int limit = 100, int skip = 0
    //    ) {
    //      return repo.GetEntities(
    //        typeof(TEntity).Name, filter, sortedBy, limit, skip
    //      ).OfType<TEntity>().ToArray();
    //    }

    //    /// <summary>
    //    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    //    /// </summary>
    //    /// <param name="searchExpression"></param>
    //    /// <param name="sortedBy">
    //    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    //    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    //    /// </param>
    //    /// <param name="limit"></param>
    //    /// <param name="skip"></param>
    //    /// <returns></returns>
    //    public static TEntity[] GetEntitiesBySearchExpression<TEntity>(
    //      this IUniversalRepository repo, string searchExpression, string[] sortedBy, int limit = 100, int skip = 0
    //    ) {
    //      return repo.GetEntitiesBySearchExpression(
    //        typeof(TEntity).Name, searchExpression, sortedBy, limit, skip
    //      ).OfType<TEntity>().ToArray(); ;
    //    }

    //    /// <summary> </summary>
    //    /// <param name="keysToLoad"></param>
    //    /// <returns></returns>
    //    public static TEntity[] GetEntitiesByKey<TEntity>(
    //      this IUniversalRepository repo, object[] keysToLoad
    //    ) {
    //      return repo.GetEntitiesByKey(typeof(TEntity).Name, keysToLoad).OfType<TEntity>().ToArray();
    //    }

    //    /// <summary> </summary>
    //    /// <param name="entityName"> The name of the Entity-CLASS (selects the concrete store, to work on) </param>
    //    /// <param name="filter"></param>
    //    /// <param name="includedFieldNames">
    //    /// An array of field names to be loaded
    //    /// </param>
    //    /// <param name="sortedBy">
    //    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    //    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    //    /// </param>
    //    /// <param name="limit"></param>
    //    /// <param name="skip"></param>
    //    /// <returns></returns>
    //    public static Dictionary<string, object>[] GetEntityFields<TEntity>(
    //      this IUniversalRepository repo, string entityName, ExpressionTree filter,
    //      string[] includedFieldNames, string[] sortedBy, int limit = 100, int skip = 0
    //    ) {
    //      return repo.GetEntityFields(typeof(TEntity).Name, filter, includedFieldNames, sortedBy, limit, skip);
    //    }

    //    /// <summary>
    //    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    //    /// </summary>
    //    /// <param name="searchExpression"></param>
    //    /// <param name="includedFieldNames">
    //    /// An array of field names to be loaded
    //    /// </param>
    //    /// <param name="sortedBy">
    //    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    //    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    //    /// </param>
    //    /// <param name="limit"></param>
    //    /// <param name="skip"></param>
    //    /// <returns></returns>
    //    public static Dictionary<string, object>[] GetEntityFieldsBySearchExpression<TEntity>(
    //      this IUniversalRepository repo, string searchExpression, string[] includedFieldNames,
    //      string[] sortedBy, int limit = 100, int skip = 0
    //    ) {
    //      return repo.GetEntityFieldsBySearchExpression(
    //        typeof(TEntity).Name, searchExpression, includedFieldNames, sortedBy, limit, skip
    //      );
    //    }

    //    public static Dictionary<string, object>[] GetEntityFieldsByKey<TEntity>(
    //      this IUniversalRepository repo, object[] keysToLoad, string[] includedFieldNames
    //    ) {
    //      return repo.GetEntityFieldsByKey(typeof(TEntity).Name, keysToLoad, includedFieldNames);
    //    }

    //    public static int CountAll<TEntity>(this IUniversalRepository repo) {
    //      return repo.CountAll(typeof(TEntity).Name);
    //    }

    //    public static int Count<TEntity>(this IUniversalRepository repo, ExpressionTree filter) {
    //      return repo.Count(typeof(TEntity).Name, filter);
    //    }

    //    /// <summary>
    //    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability is given for this repository! 
    //    /// </summary>
    //    /// <param name="searchExpression"></param>
    //    /// <returns></returns>
    //    public static int CountBySearchExpression<TEntity>(
    //      this IUniversalRepository repo, string searchExpression
    //    ) {
    //      return repo.CountBySearchExpression(typeof(TEntity).Name, searchExpression);
    //    }

    //    public static bool ContainsKey<TEntity>(this IUniversalRepository repo, object key) {
    //      return repo.ContainsKey(typeof(TEntity).Name, key);
    //    }

    //    /// <summary>
    //    /// Creates an new Entity or Updates the given set of fields for an entity and 
    //    /// returns all fields that ARE DIFFERENT from the given values or null, if the entity wasn't found.
    //    /// </summary>
    //    /// <param name="fields">
    //    /// NOTE: The target entity to be updated will be addressed by the key values used from this param,
    //    /// so if the given dictionary contains key fields which are addressing an exisiting record, then it will be updated.
    //    /// (CASE): 
    //    /// If the given dictionary contains NO key fields, then the result is depending on the concrete repository
    //    /// implementation (see the 'RequiresExternalKeys'-Capability):
    //    /// If external keys are required, this method will skip crating a record and return null,
    //    /// otherwise it will create a new record (with an new key) and return it.
    //    /// (CASE): 
    //    /// If the given dictionary contains key fields, which are addressing an NOT exisiting entity then the
    //    /// result is again depending on the concrete repository implementation (see the 'RequiresExternalKeys'-Capability):
    //    /// If external keys are required, this method will crate a record (using the given key) and return it,
    //    /// otherwise it will skip create a new record (with an new key) and return it.
    //    /// (CASE): 
    //    /// If the given dictionary contains key fields, which are addressing an NOT exisiting entity then
    //    /// it will add it and return it again. Depending on the concrete repository implementation
    //    /// (see the 'RequiresExternalKeys'-Capability) it will either use the given key oder create a assign
    //    /// a new key that will be present within the returned fields). For that reason it is IMPORTANT,
    //    /// that the call needs to evaluate the returned key!
    //    /// </param>
    //    /// All fields that ARE DIFFERENT from the given values or null, if the entity wasn't found.
    //    /// NOTE: the returned entity can differ from the given one, because in some cases a field
    //    /// (1) was not updated,
    //    /// (2) was updated using normlized (=modified) value that differs from the given one,
    //    /// (3) was updated implicitely (timestamp's,rowversion's,...) 
    //    public static Dictionary<string, object> AddOrUpdateEntityFields<TEntity>(
    //      this IUniversalRepository repo, Dictionary<string, object> fields
    //    ) {
    //      return repo.AddOrUpdateEntityFields(typeof(TEntity).Name, fields);
    //    }

    //    /// <summary>
    //    /// Creates an new Entity or Updates the given set of fields for an entity and 
    //    /// returns the entity within its new state or null, if the entity wasn't found.
    //    /// </summary>
    //    /// <param name="entity">
    //    /// NOTE: The target entity to be updated will be addressed by the key values used from this param,
    //    /// so if the given dictionary contains key fields which are addressing an exisiting record, then it will be updated.
    //    /// (CASE): 
    //    /// If the given dictionary contains NO key fields, then the result is depending on the concrete repository
    //    /// implementation (see the 'RequiresExternalKeys'-Capability):
    //    /// If external keys are required, this method will skip crating a record and return null,
    //    /// otherwise it will create a new record (with an new key) and return it.
    //    /// (CASE): 
    //    /// If the given dictionary contains key fields, which are addressing an NOT exisiting entity then the
    //    /// result is again depending on the concrete repository implementation (see the 'RequiresExternalKeys'-Capability):
    //    /// If external keys are required, this method will crate a record (using the given key) and return it,
    //    /// otherwise it will skip create a new record (with an new key) and return it.
    //    /// (CASE): 
    //    /// If the given dictionary contains key fields, which are addressing an NOT exisiting entity then
    //    /// it will add it and return it again. Depending on the concrete repository implementation
    //    /// (see the 'RequiresExternalKeys'-Capability) it will either use the given key oder create a assign
    //    /// a new key that will be present within the returned fields). For that reason it is IMPORTANT,
    //    /// that the call needs to evaluate the returned key!
    //    /// </param>
    //    /// returns the entity within its new state or null, if the entity wasn't found.
    //    /// NOTE: the returned entity can differ from the given one, because in some cases a field
    //    /// (1) was not updated,
    //    /// (2) was updated using normlized (=modified) value that differs from the given one,
    //    /// (3) was updated implicitely (timestamp's,rowversion's,...) 
    //    public static TEntity AddOrUpdateEntity<TEntity>(
    //      this IUniversalRepository repo, TEntity entity
    //    ) {
    //      return (TEntity)repo.AddOrUpdateEntity(typeof(TEntity).Name, entity);
    //    }

    //    /// <summary>
    //    /// Updates the given set of fields for an entity and 
    //    /// returns all fields that ARE DIFFERENT from the given values or null, if the entity wasn't found.
    //    /// </summary>
    //    /// <param name="fields">
    //    /// NOTE: The target entity to be updated will be addressed by the key values used from this param!
    //    /// If the given dictionary does not contain the key fiels, an exception will be thrown!
    //    /// </param>
    //    /// All fields that ARE DIFFERENT from the given values or null, if the entity wasn't found.
    //    /// NOTE: the returned entity can differ from the given one, because in some cases a field
    //    /// (1) was not updated,
    //    /// (2) was updated using normlized (=modified) value that differs from the given one,
    //    /// (3) was updated implicitely (timestamp's,rowversion's,...) 
    //    public static Dictionary<string, object> TryUpdateEntityFields<TEntity>(
    //      this IUniversalRepository repo, Dictionary<string, object> fields
    //    ) {
    //      return repo.TryUpdateEntityFields(typeof(TEntity).Name, fields);
    //    }

    //    /// <summary>
    //    /// Updates all updatable fields for an entity and 
    //    /// returns the entity within its new state or null, if the entity wasn't found.
    //    /// </summary>
    //    /// <param name="entity">
    //    /// NOTE: The target entity which to be updated will be addressed by the key values used from this param!
    //    /// </param>
    //    /// <returns>
    //    /// The entity after it was updated or null, if the entity wasn't found.
    //    /// NOTE: the returned entity can differ from the given one, because in some cases a field
    //    /// (1) was not updated,
    //    /// (2) was updated using normlized (=modified) value that differs from the given one,
    //    /// (3) was updated implicitely (timestamp's,rowversion's,...) 
    //    /// </returns>
    //    public static TEntity TryUpdateEntity<TEntity>(
    //      this IUniversalRepository repo, TEntity entity
    //    ) {
    //      return (TEntity)repo.TryUpdateEntity(typeof(TEntity).Name, entity);
    //    }

    //    /// <summary>
    //    /// Adds an new entity and returns its Key on success, otherwise null
    //    /// (also if the entity is already exisiting).
    //    /// Depending on the concrete repository implementation the KEY properties
    //    /// of the entity needs be pre-initialized (see the 'RequiresExternalKeys'-Capability). 
    //    /// </summary>
    //    /// <param name="entity"></param>
    //    /// <returns>The entity key on success, otherwise null</returns>
    //    public static TEntity TryAddEntity<TEntity>(
    //      this IUniversalRepository repo, TEntity entity
    //    ) {
    //      return (TEntity)repo.TryAddEntity(typeof(TEntity).Name, entity);
    //    }

    //    /// <summary>
    //    /// Updates a dedicated subset of fields for all addressed entites and
    //    /// returns an array containing the keys of affeced entities. 
    //    /// NOTE: this method can only be used, if the 'SupportsMassupdate'-Capability is given for this repository! 
    //    /// </summary>
    //    /// <param name="keysToUpdate">Keys for that entities, which sould be updated (non exisiting keys will be ignored).</param>
    //    /// <param name="fields">A set of fields and value, that should be update.
    //    /// It MUST NOT contain fields which are part of the Key, otherwise an exception will be thrown!
    //    /// </param>
    //    /// <returns>An array containing the keys of affeced entities.</returns>
    //    public static object[] MassupdateByKeys<TEntity>(
    //      this IUniversalRepository repo, object[] keysToUpdate, Dictionary<string, object> fields
    //    ) {
    //      return repo.MassupdateByKeys(typeof(TEntity).Name, keysToUpdate, fields);
    //    }

    //    /// <summary>
    //    /// Updates a dedicated subset of fields for all addressed entites and
    //    /// returns an array containing the keys of affeced entities. 
    //    /// NOTE: this method can only be used, if the 'SupportsMassupdate'-Capability is given for this repository! 
    //    /// </summary>
    //    /// <param name="filter">A filter to adress that entities, which sould be updated.</param>
    //    /// <param name="fields">A set of fields and value, that should be update.
    //    /// It MUST NOT contain fields which are part of the Key, otherwise an exception will be thrown!
    //    /// </param>
    //    /// <returns></returns>
    //    public static object[] Massupdate<TEntity>(
    //      this IUniversalRepository repo, ExpressionTree filter, Dictionary<string, object> fields
    //    ) {
    //      return repo.Massupdate(typeof(TEntity).Name, filter, fields);
    //    }

    //    /// <summary>
    //    /// Updates a dedicated subset of fields for all addressed entites and
    //    /// returns an array containing the keys of affeced entities. 
    //    /// NOTE: this method can only be used, if the 'SupportsStringBaseSearchExpressions'-Capability and the
    //    /// 'SupportsMassupdate'-Capability are given for this repository! 
    //    /// </summary>
    //    /// <param name="searchExpression">A search expression to adress that entities, which sould be updated.</param>
    //    /// <param name="fields">A set of fields and value, that should be update.
    //    /// It MUST NOT contain fields which are part of the Key, otherwise an exception will be thrown!
    //    /// </param>
    //    /// <returns></returns>
    //    public static object[] MassupdateBySearchExpression<TEntity>(
    //      this IUniversalRepository repo, string searchExpression, Dictionary<string, object> fields
    //    ) {
    //      return repo.MassupdateBySearchExpression(typeof(TEntity).Name, searchExpression, fields);
    //    }

    //    /// <summary>
    //    /// Tries to delete entities by the given keys und returns an array containing the keys of only that entities
    //    /// which were deleted successfully.
    //    /// NOTE: this method can only be used, if the 'CanDeleteEntities'-Capability is given for this repository! 
    //    /// </summary>
    //    /// <param name="keysToDelete"></param>
    //    /// <returns>keys of deleted entities</returns>
    //    public static object[] TryDeleteEntities<TEntity>(
    //      this IUniversalRepository repo, object[] keysToDelete
    //    ) {
    //      return repo.TryDeleteEntities(typeof(TEntity).Name, keysToDelete);
    //    }

    //    /// <summary>
    //    /// Changes the KEY for an entity.
    //    /// NOTE: this method can only be used, if the 'SupportsKeyUpdate'-Capability is given for this repository! 
    //    /// </summary>
    //    /// <param name="currentKey"></param>
    //    /// <param name="newKey"></param>
    //    /// <returns></returns>
    //    public static bool TryUpdateKey<TEntity>(
    //      this IUniversalRepository repo, object currentKey, object newKey
    //    ) {
    //      return repo.TryUpdateKey(typeof(TEntity).Name, currentKey, newKey);
    //    }

    //    public static void CopyValuesToEntity<TEntity>(
    //      this Dictionary<string, object> fields, TEntity targetEntity
    //    ) {
    //      foreach (string fieldName in fields.Keys) {
    //        var prop = typeof(TEntity).GetProperty(fieldName);
    //        if (prop != null) {
    //          prop.SetValue(targetEntity, fields[fieldName], null);
    //        }
    //      }
    //    }

    #endregion

  }

}
