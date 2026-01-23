using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;

namespace System.Data.Fuse.LinqSupport {

  public static partial class RepositoryExpressionExtensions {

    private const char _DescPrefix = '^';

    /// <summary>
    /// Cache for selector lambdas to avoid rebuilding expression trees repeatedly.
    /// Key: "{EntityType.AssemblyQualifiedName}|{PropertyPath}"
    /// Value: LambdaExpression (Expression&lt;Func&lt;TEntity, TKey&gt;&gt;)
    /// </summary>
    private static readonly ConcurrentDictionary<string, LambdaExpression> _SelectorCache = new ConcurrentDictionary<string, LambdaExpression>(StringComparer.Ordinal);

    /// <summary></summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entities"></param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results.
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <returns></returns>
    [Obsolete("use 'ApplySorting' instead!")]
    public static IQueryable<TEntity> ApplySortingViaLinqDynamic<TEntity>(this IQueryable<TEntity> entities, params string[] sortedBy) {
      foreach (var sortField in sortedBy) {
        if (sortField.StartsWith("^")) {
          string descSortField = sortField.Substring(1); // remove the "^" prefix
          //HACK: internal usage of System.Linq.Dynamic.Core
          entities = entities.OrderBy(descSortField + " descending");
        }
        else {
          entities = entities.OrderBy(sortField);
        }
      }

      return entities;
    }

    /// <summary>
    /// Applies sorting to an <see cref="IQueryable{TEntity}"/> based on string field names.
    /// Use '^' as prefix for DESC sorting. Example: ["^Age", "Lastname"].
    /// Supports nested paths, e.g. "Address.City".
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <param name="entities">Source query.</param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results.
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <returns>Sorted query.</returns>
    public static IQueryable<TEntity> ApplySorting<TEntity>(this IQueryable<TEntity> entities, params string[] sortedBy) {
   
      if (entities == null) {
        throw new ArgumentNullException(nameof(entities));
      }
      if (sortedBy == null) {
        throw new ArgumentNullException(nameof(sortedBy));
      }

      IQueryable<TEntity> current = entities;
      bool isFirst = true;

      for (int i = 0; i < sortedBy.Length; i++) {
        string sortFieldRaw = sortedBy[i];

        if (string.IsNullOrWhiteSpace(sortFieldRaw)) {
          continue;
        }

        bool descending = false;
        string propertyPath = sortFieldRaw.Trim();

        if (propertyPath.Length > 0 && propertyPath[0] == _DescPrefix) {
          descending = true;
          propertyPath = propertyPath.Substring(1).Trim();
        }

        if (propertyPath.Length == 0) {
          continue;
        }

        LambdaExpression selector = BuildOrGetSelectorLambda(typeof(TEntity), propertyPath);

        string methodName;
        if (isFirst) {
          methodName = descending ? "OrderByDescending" : "OrderBy";
        }
        else {
          methodName = descending ? "ThenByDescending" : "ThenBy";
        }

        current = ApplyQueryableOrderMethod(current, selector, methodName);
        isFirst = false;
      }

      return current;
    }

    /// <summary>
    /// Applies sorting to an <see cref="IEnumerable{TEntity}"/> based on string field names.
    /// Use '^' as prefix for DESC sorting. Example: ["^Age", "Lastname"].
    /// Supports nested paths, e.g. "Address.City".
    /// If the runtime instance is actually IQueryable, it will route to the IQueryable overload
    /// to avoid unnecessary materialization.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <param name="entities">Source sequence.</param>
    /// <param name="sortedBy">
    /// An array of field names to be used for sorting the results.
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </param>
    /// <returns>Sorted sequence.</returns>
    public static IEnumerable<TEntity> ApplySorting<TEntity>(this IEnumerable<TEntity> entities, params string[] sortedBy) {
    
      if (entities == null) {
        throw new ArgumentNullException(nameof(entities));
      }
      if (sortedBy == null) {
        throw new ArgumentNullException(nameof(sortedBy));
      }

      // IMPORTANT:
      // If the runtime type is IQueryable, keep it queryable and provider-based.
      IQueryable<TEntity> queryable = entities as IQueryable<TEntity>;
      if (queryable != null) {
        return queryable.ApplySorting(sortedBy);
      }

      IEnumerable<TEntity> current = entities;
      IOrderedEnumerable<TEntity> ordered = null;
      bool isFirst = true;

      for (int i = 0; i < sortedBy.Length; i++) {
        string sortFieldRaw = sortedBy[i];

        if (string.IsNullOrWhiteSpace(sortFieldRaw)) {
          continue;
        }

        bool descending = false;
        string propertyPath = sortFieldRaw.Trim();

        if (propertyPath.Length > 0 && propertyPath[0] == _DescPrefix) {
          descending = true;
          propertyPath = propertyPath.Substring(1).Trim();
        }

        if (propertyPath.Length == 0) {
          continue;
        }

        LambdaExpression selector = BuildOrGetSelectorLambda(typeof(TEntity), propertyPath);

        if (isFirst) {
          ordered = ApplyEnumerableOrderMethod(current, selector, descending ? "OrderByDescending" : "OrderBy");
          current = ordered;
          isFirst = false;
        }
        else {
          if (ordered == null) {
            // Defensive: should never happen because once we sort first, ordered is set.
            ordered = current.OrderBy((Func<TEntity, int>)((TEntity _) => 0));
          }

          ordered = ApplyEnumerableThenMethod(ordered, selector, descending ? "ThenByDescending" : "ThenBy");
          current = ordered;
        }
      }

      return current;
    }

    /// <summary>
    /// Builds (or returns cached) key-selector lambda for a given entity type and property path.
    /// Result is a LambdaExpression of type Expression&lt;Func&lt;TEntity, TKey&gt;&gt;.
    /// </summary>
    private static LambdaExpression BuildOrGetSelectorLambda(Type entityType, string propertyPath) {
      if (entityType == null) {
        throw new ArgumentNullException(nameof(entityType));
      }
      if (propertyPath == null) {
        throw new ArgumentNullException(nameof(propertyPath));
      }

      string cacheKey = entityType.AssemblyQualifiedName + "|" + propertyPath;

      LambdaExpression cached;
      if (_SelectorCache.TryGetValue(cacheKey, out cached)) {
        return cached;
      }

      ParameterExpression parameter = Expression.Parameter(entityType, "e");

      Expression body = BuildMemberAccess(parameter, entityType, propertyPath, out Type keyType);

      // Build Expression<Func<TEntity, TKey>>
      Type funcType = typeof(Func<,>).MakeGenericType(entityType, keyType);
      LambdaExpression lambda = Expression.Lambda(funcType, body, parameter);

      // Cache best-effort (race is fine).
      _SelectorCache.TryAdd(cacheKey, lambda);

      return lambda;
    }

    /// <summary>
    /// Creates an expression that accesses a property/field chain, e.g. "Address.City".
    /// Throws an ArgumentException if any segment cannot be resolved.
    /// </summary>
    private static Expression BuildMemberAccess(Expression instance, Type instanceType, string propertyPath, out Type finalType) {
      if (instance == null) {
        throw new ArgumentNullException(nameof(instance));
      }
      if (instanceType == null) {
        throw new ArgumentNullException(nameof(instanceType));
      }
      if (string.IsNullOrWhiteSpace(propertyPath)) {
        throw new ArgumentException("Property path must not be null/empty.", nameof(propertyPath));
      }

      string[] parts = propertyPath.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length == 0) {
        throw new ArgumentException("Property path must contain at least one member name.", nameof(propertyPath));
      }

      Expression current = instance;
      Type currentType = instanceType;

      for (int i = 0; i < parts.Length; i++) {
        string memberName = parts[i].Trim();
        if (memberName.Length == 0) {
          throw new ArgumentException("Property path contains an empty segment.", nameof(propertyPath));
        }

        MemberInfo member = FindPropertyOrField(currentType, memberName);
        if (member == null) {
          throw new ArgumentException(
            "Member '" + memberName + "' was not found on type '" + currentType.FullName + "' for path '" + propertyPath + "'.",
            nameof(propertyPath));
        }

        // Expression.PropertyOrField also works, but we want a controlled resolution and better errors.
        if (member is PropertyInfo) {
          PropertyInfo propertyInfo = (PropertyInfo)member;
          current = Expression.Property(current, propertyInfo);
          currentType = propertyInfo.PropertyType;
        }
        else if (member is FieldInfo) {
          FieldInfo fieldInfo = (FieldInfo)member;
          current = Expression.Field(current, fieldInfo);
          currentType = fieldInfo.FieldType;
        }
        else {
          throw new ArgumentException(
            "Member '" + memberName + "' on type '" + currentType.FullName + "' is not a field or property.",
            nameof(propertyPath));
        }
      }

      finalType = currentType;
      return current;
    }

    /// <summary>
    /// Finds a public instance property or field by name (case-sensitive first, then case-insensitive).
    /// </summary>
    private static MemberInfo FindPropertyOrField(Type type, string name) {
      BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

      // Case-sensitive
      PropertyInfo property = type.GetProperty(name, flags);
      if (property != null) {
        return property;
      }

      FieldInfo field = type.GetField(name, flags);
      if (field != null) {
        return field;
      }

      // Case-insensitive fallback
      PropertyInfo[] properties = type.GetProperties(flags);
      for (int i = 0; i < properties.Length; i++) {
        if (string.Equals(properties[i].Name, name, StringComparison.OrdinalIgnoreCase)) {
          return properties[i];
        }
      }

      FieldInfo[] fields = type.GetFields(flags);
      for (int i = 0; i < fields.Length; i++) {
        if (string.Equals(fields[i].Name, name, StringComparison.OrdinalIgnoreCase)) {
          return fields[i];
        }
      }

      return null;
    }

    /// <summary>
    /// Applies Queryable.OrderBy/ThenBy (or descending variants) using reflection and the provided selector lambda.
    /// </summary>
    private static IQueryable<TEntity> ApplyQueryableOrderMethod<TEntity>(IQueryable<TEntity> source, LambdaExpression selector, string methodName) {
      if (source == null) {
        throw new ArgumentNullException(nameof(source));
      }
      if (selector == null) {
        throw new ArgumentNullException(nameof(selector));
      }
      if (string.IsNullOrWhiteSpace(methodName)) {
        throw new ArgumentException("Method name must not be null/empty.", nameof(methodName));
      }

      // selector.Type is Func<TEntity, TKey>
      Type entityType = typeof(TEntity);
      Type keyType = selector.Body.Type;

      MethodInfo method = GetQueryableGenericOrderMethod(methodName);
      MethodInfo generic = method.MakeGenericMethod(entityType, keyType);

      MethodCallExpression call = Expression.Call(
        null,
        generic,
        new Expression[] { source.Expression, Expression.Quote(selector) });

      IQueryable<TEntity> result = source.Provider.CreateQuery<TEntity>(call);
      return result;
    }

    /// <summary>
    /// Returns the Queryable.OrderBy / OrderByDescending / ThenBy / ThenByDescending method definition (2 parameters).
    /// </summary>
    private static MethodInfo GetQueryableGenericOrderMethod(string name) {
      MethodInfo[] methods = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static);

      for (int i = 0; i < methods.Length; i++) {
        MethodInfo method = methods[i];
        if (!string.Equals(method.Name, name, StringComparison.Ordinal)) {
          continue;
        }
        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length != 2) {
          continue;
        }
        if (!method.IsGenericMethodDefinition) {
          continue;
        }

        // signature: (IQueryable<TSource>, Expression<Func<TSource,TKey>>)
        return method;
      }

      throw new InvalidOperationException("Queryable method not found: " + name);
    }

    /// <summary>
    /// Applies Enumerable.OrderBy/OrderByDescending using reflection and a typed compiled selector.
    /// </summary>
    private static IOrderedEnumerable<TEntity> ApplyEnumerableOrderMethod<TEntity>(IEnumerable<TEntity> source, LambdaExpression selector, string methodName) {
     
      if (source == null) {
        throw new ArgumentNullException(nameof(source));
      }
      if (selector == null) {
        throw new ArgumentNullException(nameof(selector));
      }

      Type entityType = typeof(TEntity);
      Type keyType = selector.Body.Type;

      Delegate compiled = selector.Compile();

      MethodInfo method = GetEnumerableGenericOrderMethod(methodName);
      MethodInfo generic = method.MakeGenericMethod(entityType, keyType);

      object result = generic.Invoke(null, new object[] { source, compiled });
      return (IOrderedEnumerable<TEntity>)result;
    }

    /// <summary>
    /// Applies Enumerable.ThenBy/ThenByDescending using reflection and a typed compiled selector.
    /// </summary>
    private static IOrderedEnumerable<TEntity> ApplyEnumerableThenMethod<TEntity>(IOrderedEnumerable<TEntity> source, LambdaExpression selector, string methodName) {
     
      if (source == null) {
        throw new ArgumentNullException(nameof(source));
      }
      if (selector == null) {
        throw new ArgumentNullException(nameof(selector));
      }

      Type entityType = typeof(TEntity);
      Type keyType = selector.Body.Type;

      Delegate compiled = selector.Compile();

      MethodInfo method = GetEnumerableGenericThenMethod(methodName);
      MethodInfo generic = method.MakeGenericMethod(entityType, keyType);

      object result = generic.Invoke(null, new object[] { source, compiled });
      return (IOrderedEnumerable<TEntity>)result;
    }

    /// <summary>
    /// Returns the Enumerable.OrderBy / OrderByDescending method definition (2 parameters).
    /// </summary>
    private static MethodInfo GetEnumerableGenericOrderMethod(string name) {
      MethodInfo[] methods = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static);

      for (int i = 0; i < methods.Length; i++) {
        MethodInfo method = methods[i];
        if (!string.Equals(method.Name, name, StringComparison.Ordinal)) {
          continue;
        }

        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length != 2) {
          continue;
        }
        if (!method.IsGenericMethodDefinition) {
          continue;
        }

        // signature: (IEnumerable<TSource>, Func<TSource,TKey>)
        return method;
      }

      throw new InvalidOperationException("Enumerable method not found: " + name);
    }

    /// <summary>
    /// Returns the Enumerable.ThenBy / ThenByDescending method definition (2 parameters).
    /// </summary>
    private static MethodInfo GetEnumerableGenericThenMethod(string name) {
      MethodInfo[] methods = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static);

      for (int i = 0; i < methods.Length; i++) {
        MethodInfo method = methods[i];
        if (!string.Equals(method.Name, name, StringComparison.Ordinal)) {
          continue;
        }

        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length != 2) {
          continue;
        }
        if (!method.IsGenericMethodDefinition) {
          continue;
        }

        // signature: (IOrderedEnumerable<TSource>, Func<TSource,TKey>)
        return method;
      }

      throw new InvalidOperationException("Enumerable method not found: " + name);
    }

    public static IQueryable<TEntity> ApplyPaging<TEntity>(this IQueryable<TEntity> entities, int limit, int skip) {
      if (skip == 0 && limit == 0) {
        return entities;
      }
      else if (limit == 0) {
        return entities.Skip(skip);
      }
      else if (skip == 0) {
        return entities.Take(limit);
      }
      else {
        return entities.Skip(skip).Take(limit);
      }
    }

    public static IEnumerable<TEntity> ApplyPaging<TEntity>(this IEnumerable<TEntity> entities, int limit, int skip) {

      if (entities is IQueryable<TEntity>) {
        //avoid materialization if declarative type is ienumerable but runtime type is queryable...
        return ((IQueryable<TEntity>)entities).ApplyPaging(limit, skip);
      }

      if (skip == 0 && limit == 0) {
        return entities;
      }
      else if (limit == 0) {
        return entities.Skip(skip);
      }
      else if (skip == 0) {
        return entities.Take(limit);
      }
      else {
        return entities.Skip(skip).Take(limit);
      }
    }

  }

}
