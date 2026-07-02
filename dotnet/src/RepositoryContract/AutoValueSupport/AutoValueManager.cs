using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Data.Fuse.AutoValueSupport {
  public class AutoValueManager {

    private const string _DefaultIncrementAlgorithm = "Increment";
    //private const string _DefaultIncrementAlgorithm = "__default_increment__";

    private static readonly object _SyncRoot = new object();
    private static readonly Dictionary<Type, AutoValuePropertyDescriptor[]> _PropertyDescriptorsByEntityType = new Dictionary<Type, AutoValuePropertyDescriptor[]>();
    private static readonly Dictionary<string, Func<AutoValueContext, object>> _AlgorithmHandlers = new Dictionary<string, Func<AutoValueContext, object>>(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, decimal> _HighestAssignedValues = new Dictionary<string, decimal>();
    private static readonly HashSet<string> _InitializedHighestAssignedValueKeys = new HashSet<string>();

    private readonly Type _EntityType;

    static AutoValueManager() {
      RegisterAlgorithm(_DefaultIncrementAlgorithm, StandardIncrementAlgorithm);
    }

    public AutoValueManager(Type entityType) {
      _EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
    }

    public static void RegisterAlgorithm(string algorithm, Func<AutoValueContext, object> handler) {
      if (string.IsNullOrWhiteSpace(algorithm)) {
        throw new ArgumentException("Algorithm name must not be empty.", nameof(algorithm));
      }
      if (handler == null) {
        throw new ArgumentNullException(nameof(handler));
      }

      lock (_SyncRoot) {
        _AlgorithmHandlers[algorithm] = handler;
      }
    }

    public void ApplyValuesOnAdd(
      object entity, Func<PropertyInfo, decimal?> getHighestExistingValue, object scopeKey = null,
      Func<PropertyInfo, bool> skipProperty = null
    ) {
      if (entity == null) { return; }

      foreach (AutoValuePropertyDescriptor descriptor in this.GetAutoValueProperties(skipProperty)) {
        object currentValue = descriptor.PropertyInfo.GetValue(entity, null);
        AutoValueContext context = this.CreateContext(
          descriptor, entity, currentValue, getHighestExistingValue, scopeKey
        );

        object nextValue = this.ResolveHandler(descriptor)(context);
        descriptor.PropertyInfo.SetValue(entity, nextValue, null);

        if (descriptor.IncrementAutoValueAttribute != null && !this.IsDefaultValue(nextValue, descriptor.PropertyInfo.PropertyType)) {
          this.RememberHighestAssignedValue(descriptor, scopeKey, nextValue);
        }
      }
    }

    public bool ShouldPreserveValueOnUpdate(
      PropertyInfo propertyInfo, object incomingValue, Func<PropertyInfo, bool> skipProperty = null
    ) {
      if (propertyInfo == null) {
        return false;
      }

      if (skipProperty != null && skipProperty(propertyInfo)) {
        return false;
      }

      AutoValuePropertyDescriptor descriptor = this.GetAutoValueProperty(propertyInfo);
      if (descriptor == null) {
        return false;
      }

      // Identity fields are immutable after insert — always preserve regardless of the incoming value
      if (descriptor.IdentityAttribute != null) {
        return true;
      }

      return this.IsDefaultValue(incomingValue, propertyInfo.PropertyType);
    }

    private Func<AutoValueContext, object> ResolveHandler(AutoValuePropertyDescriptor descriptor) {
      string algorithm = descriptor.AutoValueAttribute.Algorithm;

      if (string.IsNullOrWhiteSpace(algorithm)) {
        if (descriptor.IncrementAutoValueAttribute != null) {
          algorithm = _DefaultIncrementAlgorithm;
        } else {
          throw new InvalidOperationException($"Auto value property '{_EntityType.Name}.{descriptor.PropertyInfo.Name}' requires an algorithm.");
        }
      }

      lock (_SyncRoot) {
        if (_AlgorithmHandlers.TryGetValue(algorithm, out Func<AutoValueContext, object> handler)) {
          return handler;
        }
      }

      throw new InvalidOperationException($"No auto value algorithm is registered for '{algorithm}'.");
    }

    private AutoValueContext CreateContext(
      AutoValuePropertyDescriptor descriptor, object entity, object currentValue,
      Func<PropertyInfo, decimal?> getHighestExistingValue, object scopeKey
    ) {
      decimal? highestAssignedValue;
      decimal? highestExistingValue;
      this.GetHighestValues(descriptor, getHighestExistingValue, scopeKey, out highestAssignedValue, out highestExistingValue);

      return new AutoValueContext(
        _EntityType,
        entity,
        descriptor.PropertyInfo,
        descriptor.AutoValueAttribute,
        descriptor.IncrementAutoValueAttribute,
        descriptor.IdentityAttribute,
        currentValue,
        !this.IsDefaultValue(currentValue, descriptor.PropertyInfo.PropertyType),
        highestAssignedValue,
        highestExistingValue,
        this.MaxValue(highestAssignedValue, highestExistingValue),
        scopeKey
      );
    }

    private void GetHighestValues(
      AutoValuePropertyDescriptor descriptor,
      Func<PropertyInfo, decimal?> getHighestExistingValue,
      object scopeKey,
      out decimal? highestAssignedValue,
      out decimal? highestExistingValue
    ) {
      string key = this.GetHighestAssignedValueKey(descriptor, scopeKey);

      lock (_SyncRoot) {
        if (_HighestAssignedValues.TryGetValue(key, out decimal rememberedHighestAssignedValue)) {
          highestExistingValue = null;
          highestAssignedValue = rememberedHighestAssignedValue;
          return;
        }

        if (_InitializedHighestAssignedValueKeys.Contains(key)) {
          highestAssignedValue = null;
          highestExistingValue = null;
          return;
        }
      }

      highestExistingValue = this.GetHighestExistingValue(descriptor, getHighestExistingValue);

      lock (_SyncRoot) {
        if (_HighestAssignedValues.TryGetValue(key, out decimal rememberedHighestAssignedValue)) {
          highestAssignedValue = rememberedHighestAssignedValue;
          highestExistingValue = null;
          return;
        }

        _InitializedHighestAssignedValueKeys.Add(key);
        if (highestExistingValue.HasValue) {
          _HighestAssignedValues[key] = highestExistingValue.Value;
        }

        highestAssignedValue = highestExistingValue;
      }
    }

    private decimal? GetHighestExistingValue(AutoValuePropertyDescriptor descriptor, Func<PropertyInfo, decimal?> getHighestExistingValue) {
      if (getHighestExistingValue == null) {
        return null;
      }
      decimal? highestExistingValue = getHighestExistingValue(descriptor.PropertyInfo);  
      return highestExistingValue;
    }

    private void RememberHighestAssignedValue(AutoValuePropertyDescriptor descriptor, object scopeKey, object value) {
      decimal numericValue = Convert.ToDecimal(value);
      string key = this.GetHighestAssignedValueKey(descriptor, scopeKey);

      lock (_SyncRoot) {
        if (!_HighestAssignedValues.TryGetValue(key, out decimal highestAssignedValue) || numericValue > highestAssignedValue) {
          _HighestAssignedValues[key] = numericValue;
        }
      }
    }

    private string GetHighestAssignedValueKey(AutoValuePropertyDescriptor descriptor, object scopeKey) {
      string scopeSegment = this.GetScopeSegment(scopeKey);
      return $"{scopeSegment}|{_EntityType.AssemblyQualifiedName}|{descriptor.PropertyInfo.Name}";
    }

    private string GetScopeSegment(object scopeKey) {
      if (scopeKey == null) {
        return "global";
      }
      if (scopeKey is string scopeString) {
        return $"string:{scopeString}";
      }
      return $"{scopeKey.GetType().FullName}:{RuntimeHelpers.GetHashCode(scopeKey)}";
    }

    private AutoValuePropertyDescriptor[] GetAutoValueProperties(Func<PropertyInfo, bool> skipProperty = null) {
      lock (_SyncRoot) {
        if (!_PropertyDescriptorsByEntityType.TryGetValue(_EntityType, out AutoValuePropertyDescriptor[] descriptors)) {
          descriptors = _EntityType.GetProperties().Select(
            propertyInfo => {
              AutoValueAttribute autoValueAttribute = propertyInfo.GetCustomAttribute<AutoValueAttribute>();
              if (autoValueAttribute == null) {
                return null;
              }

              return new AutoValuePropertyDescriptor(
                propertyInfo,
                autoValueAttribute,
                propertyInfo.GetCustomAttribute<IncrementAutoValueAttribute>(),
                propertyInfo.GetCustomAttribute<IdentityAttribute>()
              );
            }
          ).Where(d => d != null).ToArray();

          if (descriptors.Count(d => d.IdentityAttribute != null) > 1) {
            throw new InvalidOperationException($"Entity '{_EntityType.Name}' must not define more than one identity property.");
          }

          _PropertyDescriptorsByEntityType[_EntityType] = descriptors;
        }

        if (skipProperty == null) {
          return descriptors;
        }

        return descriptors.Where(d => !skipProperty(d.PropertyInfo)).ToArray();
      }
    }

    private AutoValuePropertyDescriptor GetAutoValueProperty(PropertyInfo propertyInfo) {
      return this.GetAutoValueProperties().FirstOrDefault(
        descriptor => descriptor.PropertyInfo.Name == propertyInfo.Name
      );
    }

    private decimal? MaxValue(decimal? left, decimal? right) {
      if (!left.HasValue) {
        return right;
      }
      if (!right.HasValue) {
        return left;
      }
      return Math.Max(left.Value, right.Value);
    }

    private bool IsDefaultValue(object value, Type type) {
      object defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;
      return Equals(value, defaultValue);
    }

    private static object StandardIncrementAlgorithm(AutoValueContext context) {
      if (context.HasExplicitValue) {
        return context.CurrentValue;
      }

      decimal nextValue = context.HighestValue.HasValue
        ? context.HighestValue.Value + context.Increment
        : context.Seed;

      return Convert.ChangeType(nextValue, context.PropertyInfo.PropertyType);
    }

    public class AutoValueContext {

      public Type EntityType { get; }
      public object Entity { get; }
      public PropertyInfo PropertyInfo { get; }
      public AutoValueAttribute AutoValueAttribute { get; }
      public IncrementAutoValueAttribute IncrementAutoValueAttribute { get; }
      public IdentityAttribute IdentityAttribute { get; }
      public object CurrentValue { get; }
      public bool HasExplicitValue { get; }
      public decimal? HighestAssignedValue { get; }
      public decimal? HighestExistingValue { get; }
      public decimal? HighestValue { get; }
      public object ScopeKey { get; }
      public int Seed => IncrementAutoValueAttribute?.Seed ?? 1;
      public int Increment => IncrementAutoValueAttribute?.Increment ?? 1;

      public AutoValueContext(
        Type entityType,
        object entity,
        PropertyInfo propertyInfo,
        AutoValueAttribute autoValueAttribute,
        IncrementAutoValueAttribute incrementAutoValueAttribute,
        IdentityAttribute identityAttribute,
        object currentValue,
        bool hasExplicitValue,
        decimal? highestAssignedValue,
        decimal? highestExistingValue,
        decimal? highestValue,
        object scopeKey
      ) {
        EntityType = entityType;
        Entity = entity;
        PropertyInfo = propertyInfo;
        AutoValueAttribute = autoValueAttribute;
        IncrementAutoValueAttribute = incrementAutoValueAttribute;
        IdentityAttribute = identityAttribute;
        CurrentValue = currentValue;
        HasExplicitValue = hasExplicitValue;
        HighestAssignedValue = highestAssignedValue;
        HighestExistingValue = highestExistingValue;
        HighestValue = highestValue;
        ScopeKey = scopeKey;
      }
    }

    private class AutoValuePropertyDescriptor {

      public PropertyInfo PropertyInfo { get; }
      public AutoValueAttribute AutoValueAttribute { get; }
      public IncrementAutoValueAttribute IncrementAutoValueAttribute { get; }
      public IdentityAttribute IdentityAttribute { get; }

      public AutoValuePropertyDescriptor(
        PropertyInfo propertyInfo,
        AutoValueAttribute autoValueAttribute,
        IncrementAutoValueAttribute incrementAutoValueAttribute,
        IdentityAttribute identityAttribute
      ) {
        PropertyInfo = propertyInfo;
        AutoValueAttribute = autoValueAttribute;
        IncrementAutoValueAttribute = incrementAutoValueAttribute;
        IdentityAttribute = identityAttribute;
      }
    }
  }
}
