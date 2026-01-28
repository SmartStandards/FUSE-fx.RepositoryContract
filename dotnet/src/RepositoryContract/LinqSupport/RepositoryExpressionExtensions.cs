using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.Convenience;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;

namespace System.Data.Fuse.LinqSupport {

  public static partial class RepositoryExpressionExtensions {

    /// <summary>
    /// Executes a query against the repository using a LINQ-style predicate that is translated
    /// into a FUSE ExpressionTree and passed to IRepository.GetEntityRefs(...).
    /// </summary>
    /// <typeparam name="TEntity">The entity type of the repository.</typeparam>
    /// <typeparam name="TKey">The key type of the repository.</typeparam>
    /// <param name="repository">The repository instance to query.</param>
    /// <param name="predicate">
    /// A predicate describing the filter condition. It is not executed directly but translated
    /// into a FUSE ExpressionTree structure.
    /// </param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns>An array of entities that match the given predicate.</returns>
    /// <exception cref="ArgumentNullException">Thrown if repository or predicate is null.</exception>
    /// <exception cref="NotSupportedException">
    /// Thrown if the repository does not support string based search expressions or if
    /// the predicate contains unsupported expression constructs.
    /// </exception>
    public static EntityRef<TKey>[] GetEntityRefsWhere<TEntity, TKey>(
        this IRepository<TEntity, TKey> repository,
        Expression<Func<TEntity, bool>> predicate,
        string sortedBy, int limit = 500, int skip = 0
    ) where TEntity : class {

      //THIS IS ONLY AN OVERLOAD TO GIVE JUST A SINGLE SORT FIELD NAME INSTEAD OF ARRAY...
      return GetEntityRefsWhere<TEntity, TKey>(repository, predicate, new string[] { sortedBy }, limit, skip);
    }

    /// <summary>
    /// Executes a query against the repository using a LINQ-style predicate that is translated
    /// into a FUSE ExpressionTree and passed to IRepository.GetEntityRefs(...).
    /// </summary>
    /// <typeparam name="TEntity">The entity type of the repository.</typeparam>
    /// <typeparam name="TKey">The key type of the repository.</typeparam>
    /// <param name="repository">The repository instance to query.</param>
    /// <param name="predicate">
    /// A predicate describing the filter condition. It is not executed directly but translated
    /// into a FUSE ExpressionTree structure.
    /// </param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns>An array of entities that match the given predicate.</returns>
    /// <exception cref="ArgumentNullException">Thrown if repository or predicate is null.</exception>
    /// <exception cref="NotSupportedException">
    /// Thrown if the repository does not support string based search expressions or if
    /// the predicate contains unsupported expression constructs.
    /// </exception>
    public static EntityRef<TKey>[] GetEntityRefsWhere<TEntity, TKey>(
        this IRepository<TEntity, TKey> repository,
        Expression<Func<TEntity, bool>> predicate,
        string[] sortedBy = null, int limit = 500, int skip = 0
    ) where TEntity : class {

      if (repository == null) {
        throw new ArgumentNullException("repository");
      }

      if (predicate == null) {
        throw new ArgumentNullException("predicate");
      }

      ExpressionTree filter = ExpressionTreeMapper.BuildTreeFromLinqExpression<TEntity>(predicate);
      if (sortedBy == null) {
        sortedBy = new string[0];
      }

      EntityRef<TKey>[] result = repository.GetEntityRefs(filter, sortedBy, limit, skip);

      return result;
    }
    /// <summary>
    /// Executes a query against the repository using a LINQ-style predicate that is translated
    /// into a FUSE ExpressionTree and passed to IRepository.GetEntities(...).
    /// </summary>
    /// <typeparam name="TEntity">The entity type of the repository.</typeparam>
    /// <typeparam name="TKey">The key type of the repository.</typeparam>
    /// <param name="repository">The repository instance to query.</param>
    /// <param name="predicate">
    /// A predicate describing the filter condition. It is not executed directly but translated
    /// into a FUSE ExpressionTree structure.
    /// </param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns>An array of entities that match the given predicate.</returns>
    /// <exception cref="ArgumentNullException">Thrown if repository or predicate is null.</exception>
    /// <exception cref="NotSupportedException">
    /// Thrown if the repository does not support string based search expressions or if
    /// the predicate contains unsupported expression constructs.
    /// </exception>
    public static TEntity[] GetEntitiesWhere<TEntity, TKey>(
        this IRepository<TEntity, TKey> repository,
        Expression<Func<TEntity, bool>> predicate,
        string sortedBy, int limit = 500, int skip = 0
    ) where TEntity : class {

      //THIS IS ONLY AN OVERLOAD TO GIVE JUST A SINGLE SORT FIELD NAME INSTEAD OF ARRAY...
      return GetEntitiesWhere(repository, predicate, new string[] { sortedBy }, limit, skip);
    }


    /// <summary>
    /// Executes a query against the repository using a LINQ-style predicate that is translated
    /// into a FUSE ExpressionTree and passed to IRepository.GetEntities(...).
    /// </summary>
    /// <typeparam name="TEntity">The entity type of the repository.</typeparam>
    /// <typeparam name="TKey">The key type of the repository.</typeparam>
    /// <param name="repository">The repository instance to query.</param>
    /// <param name="predicate">
    /// A predicate describing the filter condition. It is not executed directly but translated
    /// into a FUSE ExpressionTree structure.
    /// </param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <returns>An array of entities that match the given predicate.</returns>
    /// <exception cref="ArgumentNullException">Thrown if repository or predicate is null.</exception>
    /// <exception cref="NotSupportedException">
    /// Thrown if the repository does not support string based search expressions or if
    /// the predicate contains unsupported expression constructs.
    /// </exception>
    public static TEntity[] GetEntitiesWhere<TEntity, TKey>(
        this IRepository<TEntity, TKey> repository,
        Expression<Func<TEntity, bool>> predicate, 
        string[] sortedBy = null, int limit = 500, int skip = 0
    ) where TEntity : class {

      if (repository == null) {
        throw new ArgumentNullException("repository");
      }

      if (predicate == null) {
        throw new ArgumentNullException("predicate");
      }

      ExpressionTree filter = ExpressionTreeMapper.BuildTreeFromLinqExpression<TEntity>(predicate);
      if(sortedBy == null) {
        sortedBy = new string[0];
      }

      TEntity[] result = repository.GetEntities(filter, sortedBy, limit, skip);

      return result;
    }

    /// <summary>
    /// Executes a projection query using a FUSE ExpressionTree filter and a LINQ-style selector expression.
    /// The repository only returns field dictionaries. This method maps them back into the desired selected type.
    /// Supports anonymous type projections.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TKey">Primary key type.</typeparam>
    /// <typeparam name="TSelectedFields">Projection type, including anonymous types.</typeparam>
    /// <param name="repository">The repository instance.</param>
    /// <param name="fieldsSelector">
    /// A projection expression defining the return object structure. Can be an anonymous type.
    /// </param>
    /// <param name="where">
    /// A boolean expression describing filter logic, translated into a FUSE ExpressionTree.
    /// </param>
    /// <param name="sortedBy">Sorting field names.</param>
    /// <param name="limit">Limit of items (default 100).</param>
    /// <param name="skip">Skip count.</param>
    /// <returns>An array of projected objects of type TSelectedFields.</returns>
    /// <exception cref="ArgumentNullException">Thrown if repository or selector is null.</exception>
    /// <exception cref="NotSupportedException">Thrown for unsupported expressions or capabilities.</exception>
    public static TSelectedFields[] GetEntityFieldsWhere<TEntity, TKey, TSelectedFields>(
      this IRepository<TEntity, TKey> repository,
      Expression<Func<TEntity, bool>> where,
      Expression<Func<TEntity, TSelectedFields>> fieldsSelector,
      string sortedBy, int limit = 500, int skip = 0
    ) where TEntity : class {

      //THIS IS ONLY AN OVERLOAD TO GIVE JUST A SINGLE SORT FIELD NAME INSTEAD OF ARRAY...
      return GetEntityFieldsWhere(repository, where, fieldsSelector, new string[] { sortedBy }, limit, skip);
    }

    /// <summary>
    /// Executes a projection query using a FUSE ExpressionTree filter and a LINQ-style selector expression.
    /// The repository only returns field dictionaries. This method maps them back into the desired selected type.
    /// Supports anonymous type projections.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TKey">Primary key type.</typeparam>
    /// <typeparam name="TSelectedFields">Projection type, including anonymous types.</typeparam>
    /// <param name="repository">The repository instance.</param>
    /// <param name="fieldsSelector">
    /// A projection expression defining the return object structure. Can be an anonymous type.
    /// </param>
    /// <param name="where">
    /// A boolean expression describing filter logic, translated into a FUSE ExpressionTree.
    /// </param>
    /// <param name="sortedBy">Sorting field names.</param>
    /// <param name="limit">Limit of items (default 100).</param>
    /// <param name="skip">Skip count.</param>
    /// <returns>An array of projected objects of type TSelectedFields.</returns>
    /// <exception cref="ArgumentNullException">Thrown if repository or selector is null.</exception>
    /// <exception cref="NotSupportedException">Thrown for unsupported expressions or capabilities.</exception>
    public static TSelectedFields[] GetEntityFieldsWhere<TEntity, TKey, TSelectedFields>(
      this IRepository<TEntity, TKey> repository,
      Expression<Func<TEntity, bool>> where,
      Expression<Func<TEntity, TSelectedFields>> fieldsSelector,
      string[] sortedBy = null, int limit = 500, int skip = 0
    ) where TEntity : class {

      if (repository == null) {
        throw new ArgumentNullException("repository");
      }

      if (fieldsSelector == null) {
        throw new ArgumentNullException("fieldsSelector");
      }

      // Build filter using provided expression
      ExpressionTree filter = ExpressionTree.Empty();
      if (where != null) {
        filter = ExpressionTreeMapper.BuildTreeFromLinqExpression<TEntity>(where);
      }

      if (sortedBy == null) {
        sortedBy = new string[0];
      }

      // Extract list of selected fields
      string[] selectedFieldNames = SelectorMapper.ExtractSelectorFieldNames(fieldsSelector);

      // Get raw field dictionaries
      Dictionary<string, object>[] rows =
          repository.GetEntityFields(filter, selectedFieldNames, sortedBy, limit, skip);

      // Map dictionary rows into TSelectedFields[]
      TSelectedFields[] mapped = SelectorMapper.Map<TEntity, TSelectedFields>(rows, fieldsSelector);

      return mapped;
    }

    /// <summary>
    /// Executes a query against the repository using a LINQ-style predicate that is translated
    /// into a FUSE ExpressionTree and passed to IRepository.Count(...).
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="repository">The repository instance to query.</param>
    /// <param name="predicate">
    /// A predicate describing the filter condition. It is not executed directly but translated
    /// into a FUSE ExpressionTree structure.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static int CountWhere<TEntity, TKey>(
      this IRepository<TEntity, TKey> repository, Expression<Func<TEntity, bool>> predicate
    ) where TEntity : class {

      if (repository == null) {
        throw new ArgumentNullException("repository");
      }

      if (predicate == null) {
        throw new ArgumentNullException("predicate");
      }

      ExpressionTree filter = ExpressionTreeMapper.BuildTreeFromLinqExpression<TEntity>(predicate);

      int result = repository.Count(filter);

      return result;
    }

    /// <summary>
    /// Executes a call against the repository using a LINQ-style predicate that is translated
    /// into a FUSE ExpressionTree and passed to IRepository.Massupdate(...).
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TSelectedFields"></typeparam>
    /// <param name="repository">The repository instance to query.</param>
    /// <param name="predicate">
    /// A predicate describing the filter condition. It is not executed directly but translated
    /// into a FUSE ExpressionTree structure.
    /// </param>
    /// <param name="entity">The entity-Instance, containing the values (to select a subset from, usig the 'fieldsSelector').</param>
    /// <param name="fieldsSelector"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TKey[] MassupdateWhere<TEntity, TKey, TSelectedFields>(
      this IRepository<TEntity, TKey> repository,
      Expression<Func<TEntity, bool>> predicate, TEntity entity,
      Func<TEntity, TSelectedFields> fieldsSelector
     ) where TEntity : class {

      if (repository == null) {
        throw new ArgumentNullException("repository");
      }

      if (predicate == null) {
        throw new ArgumentNullException("predicate");
      }

      ExpressionTree filter = ExpressionTreeMapper.BuildTreeFromLinqExpression<TEntity>(predicate);

      TSelectedFields subsetOfEntity = fieldsSelector.Invoke(entity);

      Dictionary<string, object> fieldsToUpdate = SelectorMapper.MapToDict<TSelectedFields>(subsetOfEntity);

      TKey[] affectedKeys = repository.Massupdate(filter, fieldsToUpdate);

      return affectedKeys;
    }

    /// <summary>
    /// Executes a call against the repository using a LINQ-style predicate that is translated
    /// into a FUSE ExpressionTree and passed to IRepository.TryDeleteEntities(...).
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="repository">The repository instance to query.</param>
    /// <param name="predicate">
    /// A predicate describing the filter condition. It is not executed directly but translated
    /// into a FUSE ExpressionTree structure.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TKey[] TryDeleteEntitiesWhere<TEntity, TKey>(
      this IRepository<TEntity, TKey> repository, Expression<Func<TEntity, bool>> predicate
    ) where TEntity : class {

      if (repository == null) {
        throw new ArgumentNullException("repository");
      }

      if (predicate == null) {
        throw new ArgumentNullException("predicate");
      }

      TKey[] keysToDelete = repository.GetEntityRefsWhere<TEntity, TKey>(predicate).Select((e)=> e.Key).ToArray();

      TKey[] deletedKeys = repository.TryDeleteEntities(keysToDelete);

      return deletedKeys;
    }

  }

}
