using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Fuse.Convenience {

  /// <summary>
  /// Creates Key-Value-Pairs (for Dictionaries) from entity instances
  /// </summary>
  /// <typeparam name="TEntity"></typeparam>
  internal sealed class EntityToDictionaryMapper<TEntity> where TEntity : class {

    /// <summary>
    /// Caches compiled property getters per property name for the given TEntity.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, Func<TEntity, object>> _GetterByPropertyName =
        BuildGetterCache();

    /// <summary>
    /// Caches resolved getter arrays for a specific property-name selection.
    /// Key is a normalized string representation of the requested propertyNames.
    /// </summary>
    private static readonly ConcurrentDictionary<string, CachedSelection> _SelectionCache =
        new ConcurrentDictionary<string, CachedSelection>(StringComparer.Ordinal);

    /// <summary>
    /// Maps the given entity into a dictionary (property name -> property value),
    /// restricted to the provided propertyNames.
    /// </summary>
    /// <param name="entity">The entity instance to read values from.</param>
    /// <param name="propertyNames">The property names to transfer.</param>
    /// <returns>
    /// Dictionary containing the selected property names as keys and their corresponding values.
    /// </returns>
    public Dictionary<string, object> MapEntityToDictionary(TEntity entity, string[] propertyNames) {
      if (entity is null) {
        throw new ArgumentNullException(nameof(entity));
      }

      if (propertyNames is null) {
        throw new ArgumentNullException(nameof(propertyNames));
      }

      if (propertyNames.Length == 0) {
        return new Dictionary<string, object>(0, StringComparer.Ordinal);
      }

      CachedSelection selection = this.GetOrCreateSelection(propertyNames);

      Dictionary<string, object> result =
          new Dictionary<string, object>(selection.PropertyNames.Length, StringComparer.Ordinal);

      Int32 index = 0;
      while (index < selection.PropertyNames.Length) {
        String propertyName = selection.PropertyNames[index];
        Func<TEntity, object> getter = selection.Getters[index];

        Object value = getter(entity);
        result[propertyName] = value;

        index++;
      }

      return result;
    }


    /// <summary>
    /// Resolves and caches a selection (propertyNames -> getters) for fast repeated mapping calls.
    /// </summary>
    /// <param name="propertyNames">Property name selection.</param>
    /// <returns>Cached selection containing validated property names and getters.</returns>
    private CachedSelection GetOrCreateSelection(string[] propertyNames) {
      String selectionKey = BuildSelectionKey(propertyNames);

      CachedSelection cachedSelection;
      if (EntityToDictionaryMapper<TEntity>._SelectionCache.TryGetValue(selectionKey, out cachedSelection)) {
        return cachedSelection;
      }

      // Validate and resolve getters once, then cache.
      String[] normalizedNames = new String[propertyNames.Length];
      Func<TEntity, object>[] getters = new Func<TEntity, object>[propertyNames.Length];

      Int32 index = 0;
      while (index < propertyNames.Length) {
        String propertyName = propertyNames[index];

        if (String.IsNullOrWhiteSpace(propertyName)) {
          throw new ArgumentException("Property names must not be null/empty/whitespace.", nameof(propertyNames));
        }

        Func<TEntity, object> getter;
        if (!EntityToDictionaryMapper<TEntity>._GetterByPropertyName.TryGetValue(propertyName, out getter)) {
          throw new ArgumentException(
              $"Property '{propertyName}' was not found on entity type '{typeof(TEntity).FullName}'.",
              nameof(propertyNames)
          );
        }

        normalizedNames[index] = propertyName;
        getters[index] = getter;

        index++;
      }

      CachedSelection newSelection = new CachedSelection(normalizedNames, getters);

      // Race-safe add: if another thread added concurrently, use the existing one.
      return EntityToDictionaryMapper<TEntity>._SelectionCache.GetOrAdd(selectionKey, newSelection);
    }

    /// <summary>
    /// Builds a stable cache key for a given selection, preserving order.
    /// </summary>
    /// <param name="propertyNames">Property name selection.</param>
    /// <returns>Stable key string.</returns>
    private static string BuildSelectionKey(string[] propertyNames) {
      // Using a rarely-used separator to reduce collision risk.
      // Order matters by design: ["A","B"] != ["B","A"].
      return String.Join("\u001F", propertyNames);
    }

    /// <summary>
    /// Builds a per-entity-type cache of compiled property getters.
    /// </summary>
    /// <returns>Dictionary mapping property name to compiled getter.</returns>
    private static IReadOnlyDictionary<string, Func<TEntity, object>> BuildGetterCache() {
      Dictionary<string, Func<TEntity, object>> getterByName =
          new Dictionary<string, Func<TEntity, object>>(StringComparer.Ordinal);

      PropertyInfo[] properties = typeof(TEntity).GetProperties(BindingFlags.Instance | BindingFlags.Public);

      Int32 index = 0;
      while (index < properties.Length) {
        PropertyInfo propertyInfo = properties[index];

        // Skip indexers and unreadable properties.
        if (!propertyInfo.CanRead) {
          index++;
          continue;
        }

        if (propertyInfo.GetIndexParameters().Length != 0) {
          index++;
          continue;
        }

        Func<TEntity, object> getter = CompileGetter(propertyInfo);
        getterByName[propertyInfo.Name] = getter;

        index++;
      }

      return getterByName;
    }

    /// <summary>
    /// Compiles a strongly-typed getter (TEntity -> object) for the given property.
    /// </summary>
    /// <param name="propertyInfo">Property to compile access for.</param>
    /// <returns>Compiled getter delegate.</returns>
    private static Func<TEntity, object> CompileGetter(PropertyInfo propertyInfo) {
      // (TEntity e) => (object)e.Property
      ParameterExpression entityParameter = Expression.Parameter(typeof(TEntity), "e");
      MemberExpression propertyAccess = Expression.Property(entityParameter, propertyInfo);

      UnaryExpression boxedValue = Expression.Convert(propertyAccess, typeof(object));
      Expression<Func<TEntity, object>> lambda = Expression.Lambda<Func<TEntity, object>>(boxedValue, entityParameter);

      return lambda.Compile();
    }

    /// <summary>
    /// Holds a cached mapping from a selection of property names to their compiled getters.
    /// </summary>
    private sealed class CachedSelection {
      private readonly String[] _PropertyNames;
      private readonly Func<TEntity, object>[] _Getters;

      public CachedSelection(String[] propertyNames, Func<TEntity, object>[] getters) {
        if (propertyNames is null) {
          throw new ArgumentNullException(nameof(propertyNames));
        }

        if (getters is null) {
          throw new ArgumentNullException(nameof(getters));
        }

        if (propertyNames.Length != getters.Length) {
          throw new ArgumentException("propertyNames and getters must have the same length.");
        }

        this._PropertyNames = propertyNames;
        this._Getters = getters;
      }

      public String[] PropertyNames {
        get {
          return this._PropertyNames;
        }
      }

      public Func<TEntity, object>[] Getters {
        get {
          return this._Getters;
        }
      }
    }

  }

}