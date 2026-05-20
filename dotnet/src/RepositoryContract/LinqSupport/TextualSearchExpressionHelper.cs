using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Fuse.LinqSupport {

  public static class TextualSearchExpressionHelper {

    private static readonly ConcurrentDictionary<Type, FilterabilityInfo[]> _AnalysisCache = new ConcurrentDictionary<Type, FilterabilityInfo[]>();
    private static readonly ConcurrentDictionary<string, object> _ExpressionCache = new ConcurrentDictionary<string, object>();

    /// <summary>
    /// Analyses all public instance properties of a type, including inherited properties,
    /// and returns all properties that should participate in generic filtering.
    /// </summary>
    public static FilterabilityInfo[] GetFilterabilityInfos(this Type entityType) {
      if (entityType == null) {
        throw new ArgumentNullException(nameof(entityType));
      }

      return _AnalysisCache.GetOrAdd(entityType, TextualSearchExpressionHelper.AnalyzeFilterabilityInfos);
    }

    /// <summary>
    /// Dynamically filters entities by evaluating all filterable properties using OR semantics.
    /// Using the 'FilterableAttribute' controls explicit behavior
    /// (Note: Substring only applies to strings - if used on non-string properties,
    /// ir will result in full equality-matching).
    /// Using the 'SystemInternalAttribute' excludes properties entirely.
    /// Inherited properties are included.
    /// Unsupported parse attempts are ignored per-property.
    /// Special characters/operators are treated as literal search text without wildcard or expression semantics.
    /// </summary>
    public static IQueryable<T> WhereContentContains<T>(this IQueryable<T> source, string searchString) {

      if (source == null) {
        throw new ArgumentNullException(nameof(source));
      }

      Expression<Func<T, bool>> expression = BuildSearchExpression<T>(searchString);

      return source.Where(expression);
    }

    /// <summary>
    /// Dynamically filters entities by evaluating all filterable properties using OR semantics.
    /// Using the 'FilterableAttribute' controls explicit behavior
    /// (Note: Substring only applies to strings - if used on non-string properties,
    /// ir will result in full equality-matching).
    /// Using the 'SystemInternalAttribute' excludes properties entirely.
    /// Inherited properties are included.
    /// Unsupported parse attempts are ignored per-property.
    /// Special characters/operators are treated as literal search text without wildcard or expression semantics.
    /// </summary>
    public static IEnumerable<T> WhereContentContains<T>(this IEnumerable<T> source, string searchString) {

      if (source == null) {
        throw new ArgumentNullException(nameof(source));
      }

      if(source is IQueryable<T>) {
        return ((IQueryable<T>)source).WhereContentContains(searchString);
      }

      Expression<Func<T, bool>> expression = BuildSearchExpression<T>(searchString);

      Func<T, bool> compiledExpression = expression.Compile();

      return source.Where(compiledExpression);
    }

    /// <summary>
    /// Builds a cached LINQ expression for searching across all filterable properties of T.
    /// Wildcard-like characters and operators are treated as normal search text.
    /// </summary>
    public static Expression<Func<T, bool>> BuildSearchExpression<T>(string searchString) {
      string normalizedSearchString = searchString;

      if (normalizedSearchString == null) {
        normalizedSearchString = string.Empty;
      }

      string cacheKey = typeof(T).AssemblyQualifiedName + "|" + normalizedSearchString;

      object cachedExpression;

      if (_ExpressionCache.TryGetValue(cacheKey, out cachedExpression)) {
        return (Expression<Func<T, bool>>)cachedExpression;
      }

      Expression<Func<T, bool>> expression = TextualSearchExpressionHelper.BuildSearchExpressionInternal<T>(normalizedSearchString);

      _ExpressionCache[cacheKey] = expression;

      return expression;
    }

    /// <summary>
    /// Performs the type analysis without reading the analysis cache.
    /// </summary>
    private static FilterabilityInfo[] AnalyzeFilterabilityInfos(Type entityType) {
      PropertyInfo[] properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
      List<FilterabilityInfo> result = new List<FilterabilityInfo>();

      bool hasAnyContentOrFilterableAttribute = TextualSearchExpressionHelper.HasAnyContentOrFilterableAttribute(properties);

      for (int propertyIndex = 0; propertyIndex < properties.Length; propertyIndex++) {
        PropertyInfo property = properties[propertyIndex];

        if (!TextualSearchExpressionHelper.IsSupportedProperty(property)) {
          continue;
        }

        if (TextualSearchExpressionHelper.HasAttribute(property, typeof(SystemInternalAttribute))) {
          continue;
        }

        FilterableAttribute filterableAttribute = (FilterableAttribute) TextualSearchExpressionHelper.GetAttribute(property, typeof(FilterableAttribute));

        if (filterableAttribute != null) {

          if (filterableAttribute.Filterability != Filterability.None) {
            result.Add(new FilterabilityInfo(property, filterableAttribute.Filterability));
          }

          continue;
        }

        if (TextualSearchExpressionHelper.HasAttribute(property, typeof(ContentAttribute))) {

          if(property.PropertyType == typeof(string)) {
            result.Add(new FilterabilityInfo(property, Filterability.Substring));
          }
          else {
            result.Add(new FilterabilityInfo(property, Filterability.ExactMatch));
          }
 
          continue;
        }

        if (!hasAnyContentOrFilterableAttribute) {
          if (property.PropertyType == typeof(string)) {
            result.Add(new FilterabilityInfo(property, Filterability.Substring));
          }
          else {
            result.Add(new FilterabilityInfo(property, Filterability.ExactMatch));
          }
        }
      }

      return result.ToArray();
    }

    /// <summary>
    /// Builds the actual expression tree for the given concrete search string.
    /// </summary>
    private static Expression<Func<T, bool>> BuildSearchExpressionInternal<T>(string searchString) {

      ParameterExpression parameter = Expression.Parameter(typeof(T), "e");
      FilterabilityInfo[] filterabilityInfos = typeof(T).GetFilterabilityInfos();

      if (string.IsNullOrWhiteSpace(searchString)) {
        return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), parameter);
      }

      Expression combinedExpression = null;

      for (int infoIndex = 0; infoIndex < filterabilityInfos.Length; infoIndex++) {

        FilterabilityInfo info = filterabilityInfos[infoIndex];
        Expression propertyExpression = Expression.Property(parameter, info.Property);
        Type propertyType = TextualSearchExpressionHelper.GetNonNullableType(info.Property.PropertyType);

        Expression currentExpression = TextualSearchExpressionHelper.TryBuildPropertyExpression(
          propertyExpression,
          propertyType,
          info.EnumValue,
          searchString
         );

        if (currentExpression == null) {
          continue;
        }

        if (combinedExpression == null) {
          combinedExpression = currentExpression;
        }
        else {
          combinedExpression = Expression.OrElse(combinedExpression, currentExpression);
        }
      }

      if (combinedExpression == null) {
        combinedExpression = Expression.Constant(false);
      }

      return Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
    }

    /// <summary>
    /// Builds one property-specific expression, or returns null when the search string cannot be parsed for that property.
    /// </summary>
    private static Expression TryBuildPropertyExpression(
        Expression propertyExpression,
        Type propertyType,
        Filterability filterability,
        string searchString) {
      if (propertyType == typeof(string)) {
        if (filterability == Filterability.Substring) {
          return TextualSearchExpressionHelper.BuildStringContainsExpression(propertyExpression, searchString);
        }

        if (filterability == Filterability.ExactMatch) {
          return TextualSearchExpressionHelper.BuildStringEqualsExpression(propertyExpression, searchString);
        }

        return null;
      }

      object parsedValue;

      if (!TextualSearchExpressionHelper.TryParseValue(searchString, propertyType, out parsedValue)) {
        return null;
      }

      Expression convertedPropertyExpression = propertyExpression;

      if (TextualSearchExpressionHelper.IsNullableType(propertyExpression.Type)) {
        convertedPropertyExpression = Expression.Property(propertyExpression, "Value");

        Expression hasValueExpression = Expression.Property(propertyExpression, "HasValue");
        Expression equalsExpression = Expression.Equal(
            convertedPropertyExpression,
            Expression.Constant(parsedValue, propertyType));

        return Expression.AndAlso(hasValueExpression, equalsExpression);
      }

      return Expression.Equal(
          convertedPropertyExpression,
          Expression.Constant(parsedValue, propertyType));
    }

    /// <summary>
    /// Builds a null-safe string equality expression.
    /// </summary>
    private static Expression BuildStringEqualsExpression(Expression propertyExpression, string searchString) {
      Expression notNullExpression = Expression.NotEqual(
          propertyExpression,
          Expression.Constant(null, typeof(string)));

      Expression equalsExpression = Expression.Equal(
          propertyExpression,
          Expression.Constant(searchString, typeof(string)));

      return Expression.AndAlso(notNullExpression, equalsExpression);
    }

    /// <summary>
    /// Builds a null-safe string Contains expression.
    /// </summary>
    private static Expression BuildStringContainsExpression(Expression propertyExpression, string searchString) {
      MethodInfo containsMethod = typeof(string).GetMethod(
          nameof(string.Contains),
          new Type[] { typeof(string) });

      if (containsMethod == null) {
        throw new MissingMethodException(typeof(string).FullName, nameof(string.Contains));
      }

      Expression notNullExpression = Expression.NotEqual(
          propertyExpression,
          Expression.Constant(null, typeof(string)));

      Expression containsExpression = Expression.Call(
          propertyExpression,
          containsMethod,
          Expression.Constant(searchString, typeof(string)));

      return Expression.AndAlso(notNullExpression, containsExpression);
    }

    /// <summary>
    /// Tries to parse supported scalar values from the search string.
    /// </summary>
    private static bool TryParseValue(string searchString, Type targetType, out object parsedValue) {
      parsedValue = null;

      if (targetType == typeof(Guid)) {
        Guid guidValue;

        if (Guid.TryParse(searchString, out guidValue)) {
          parsedValue = guidValue;
          return true;
        }

        return false;
      }

      if (targetType == typeof(int)) {
        int value;

        if (int.TryParse(searchString, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) {
          parsedValue = value;
          return true;
        }

        return false;
      }

      if (targetType == typeof(long)) {
        long value;

        if (long.TryParse(searchString, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) {
          parsedValue = value;
          return true;
        }

        return false;
      }

      if (targetType == typeof(decimal)) {
        decimal value;

        if (decimal.TryParse(searchString, NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
          parsedValue = value;
          return true;
        }

        return false;
      }

      if (targetType == typeof(double)) {
        double value;

        if (double.TryParse(searchString, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) {
          parsedValue = value;
          return true;
        }

        return false;
      }

      if (targetType == typeof(float)) {
        float value;

        if (float.TryParse(searchString, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) {
          parsedValue = value;
          return true;
        }

        return false;
      }

      if (targetType == typeof(short)) {
        short value;

        if (short.TryParse(searchString, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) {
          parsedValue = value;
          return true;
        }

        return false;
      }

      if (targetType == typeof(byte)) {
        byte value;

        if (byte.TryParse(searchString, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) {
          parsedValue = value;
          return true;
        }

        return false;
      }

      return false;
    }

    /// <summary>
    /// Checks whether at least one property has ContentAttribute or FilterableAttribute.
    /// </summary>
    private static bool HasAnyContentOrFilterableAttribute(PropertyInfo[] properties) {
      for (int propertyIndex = 0; propertyIndex < properties.Length; propertyIndex++) {
        PropertyInfo property = properties[propertyIndex];

        if (TextualSearchExpressionHelper.HasAttribute(property, typeof(ContentAttribute))) {
          return true;
        }

        if (TextualSearchExpressionHelper.HasAttribute(property, typeof(FilterableAttribute))) {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Checks whether a property type is supported for generic filtering.
    /// </summary>
    private static bool IsSupportedProperty(PropertyInfo property) {
      if (property == null) {
        return false;
      }

      if (!property.CanRead) {
        return false;
      }

      if (property.GetIndexParameters().Length > 0) {
        return false;
      }

      Type propertyType = TextualSearchExpressionHelper.GetNonNullableType(property.PropertyType);

      if (propertyType == typeof(string)) {
        return true;
      }

      if (propertyType == typeof(Guid)) {
        return true;
      }

      if (propertyType == typeof(int)) {
        return true;
      }

      if (propertyType == typeof(long)) {
        return true;
      }

      if (propertyType == typeof(decimal)) {
        return true;
      }

      if (propertyType == typeof(double)) {
        return true;
      }

      if (propertyType == typeof(float)) {
        return true;
      }

      if (propertyType == typeof(short)) {
        return true;
      }

      if (propertyType == typeof(byte)) {
        return true;
      }

      return false;
    }

    /// <summary>
    /// Returns the underlying type for Nullable&lt;T&gt;, otherwise the original type.
    /// </summary>
    private static Type GetNonNullableType(Type type) {
      Type nullableType = Nullable.GetUnderlyingType(type);

      if (nullableType != null) {
        return nullableType;
      }

      return type;
    }

    /// <summary>
    /// Checks whether a type is Nullable&lt;T&gt;.
    /// </summary>
    private static bool IsNullableType(Type type) {
      return Nullable.GetUnderlyingType(type) != null;
    }

    /// <summary>
    /// Gets the first attribute matching the given full type name.
    /// </summary>
    private static object GetAttribute(PropertyInfo property, Type attributeType) {
      object[] attributes = property.GetCustomAttributes(true);

      for (int attributeIndex = 0; attributeIndex < attributes.Length; attributeIndex++) {
        object attribute = attributes[attributeIndex];

        if (attribute.GetType() == attributeType) {
          return attribute;
        }
      }

      return null;
    }

    /// <summary>
    /// Checks whether the property has an attribute matching the given full type name.
    /// </summary>
    private static bool HasAttribute(PropertyInfo property, Type attributeType) {
      return TextualSearchExpressionHelper.GetAttribute(property, attributeType) != null;
    }
  }

  [DebuggerDisplay("{Property.PropertyType.Name} {Property.Name} ({EnumValue})")]
  public sealed class FilterabilityInfo {

    private readonly PropertyInfo _Property;
    private readonly Filterability _EnumValue;

    public FilterabilityInfo(PropertyInfo property, Filterability enumValue) {
      if (property == null) {
        throw new ArgumentNullException(nameof(property));
      }

      _Property = property;
      _EnumValue = enumValue;
    }

    public PropertyInfo Property {
      get {
        return _Property;
      }
    }

    public Filterability EnumValue {
      get {
        return _EnumValue;
      }
    }

  }

}
