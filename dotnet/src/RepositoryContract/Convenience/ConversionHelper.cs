using System.Collections;
using System.Collections.Generic;
using System.Data.ModelDescription;
using System.Data.ModelDescription.Convenience;
using System.Linq;
using System.Reflection;
using System.Threading;
#if NETCOREAPP
using System.Text.Json;
using System.Threading;
#endif

namespace System.Data.Fuse.Convenience {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public static class ConversionHelper {

    public static ModelVsEntityRepository<TModel, TEntity, TKey> CreateModelVsEntityRepositry<TModel, TEntity, TKey>(
      IDataStore entityDataStore, IDataStore modelDataStore,
      NavigationRole navigationRoleFlags = NavigationRole.Dependent | NavigationRole.Lookup | NavigationRole.Principal,
      bool loadMultipleNavigations = true
    )
      where TEntity : class
      where TModel : class {

      Func<string, object[], EntityRef[]> getEntityRefsByKey = (string entityName, object[] keys) => {
        return GetEntityRefs(entityName, keys, typeof(TEntity), entityDataStore, entityDataStore.GetSchemaRoot());
      };

      Func<Type, object[], object[]> getModelsByKey = (Type modelType, object[] keys) => {
        return GetEntities(modelType, keys, modelDataStore, modelDataStore.GetSchemaRoot());
      };

      Func<string, ExpressionTree, EntityRef[]> getEntityRefsBySearchExpression = (string entityName, ExpressionTree searchExpression) => {
        return GetEntityRefsBySearchExpression(entityName, searchExpression, typeof(TEntity), entityDataStore, entityDataStore.GetSchemaRoot());
      };

      Func<Type, ExpressionTree, object[]> getModelsBySearchExpression = (Type modelType, ExpressionTree searchExpression) => {
        return GetEntitiesBySearchExpression(modelType, searchExpression, modelDataStore, modelDataStore.GetSchemaRoot());
      };

      return new ModelVsEntityRepository<TModel, TEntity, TKey>(
        entityDataStore.GetRepository<TEntity, TKey>(),
        (x, y) => { }, (x, y) => { },
        ConversionHelper.ResolveNavigations<TEntity>(entityDataStore.GetSchemaRoot()),
        ConversionHelper.LoadNavigations<TEntity, TModel>(
          entityDataStore.GetSchemaRoot(),
          getEntityRefsByKey,
          getModelsByKey,
          getEntityRefsBySearchExpression,
          getModelsBySearchExpression,
          navigationRoleFlags,
          loadMultipleNavigations
        )
      );
    }

    public static DictVsEntityRepository<TEntity, TKey> CreateDictVsEntityRepositry<TEntity, TKey>(
      SchemaRoot schemaRoot, IUniversalRepository universalRepo, IRepository<TEntity, TKey> entityRepository,
      NavigationRole navigationRoleFlags = NavigationRole.Dependent | NavigationRole.Lookup | NavigationRole.Principal,
      bool loadMultipleNavigations = true
    )
      where TEntity : class {

      Func<string, object[], EntityRef[]> getEntityRefsByKey = (string entityName, object[] keys) => {
        return universalRepo.GetEntityRefsByKey(entityName, keys);
      };

      Func<string, ExpressionTree, EntityRef[]> getEntityRefsBySearchExpression = (string entityName, ExpressionTree searchExpression) => {
        return universalRepo.GetEntityRefs(entityName, searchExpression, new string[] { });
      };

      return new DictVsEntityRepository<TEntity, TKey>(
        entityRepository,
        ConversionHelper.ResolveNavigations<TEntity>(schemaRoot),
        ConversionHelper.LoadNavigations<TEntity>(
          schemaRoot,
          getEntityRefsByKey,
          null,
          getEntityRefsBySearchExpression,
          null,
          navigationRoleFlags,
          loadMultipleNavigations,
          null
        )
      );
    }

    #region EntityToModel

    public static Func<PropertyInfo, TEntity, Dictionary<string, object>, bool> LoadNavigations<TEntity, TModel>(
      SchemaRoot schema,
      Func<string, object[], EntityRef[]> getEntityRefsByKey,
      Func<Type, object[], object[]> getModelsByKey,
      Func<string, ExpressionTree, EntityRef[]> getEntityRefsBySearchExpression,
      Func<Type, ExpressionTree, object[]> getModelsBySearchExpression,
      NavigationRole navigationRoleFlags,
      bool loadMultipleNavigations
    ) {
      return LoadNavigations<TEntity>(
        schema,
        getEntityRefsByKey,
        getModelsByKey,
        getEntityRefsBySearchExpression,
        getModelsBySearchExpression,
        navigationRoleFlags,
        loadMultipleNavigations,
        typeof(TModel)
      );
    }

    internal static AsyncLocal<List<string>> _VisitedTypeNames;
    internal static object _VisitedTypeNamesLock = new object();

    public static Func<PropertyInfo, object, Dictionary<string, object>, bool> DismissNavigations(SchemaRoot schema) {
      return (pi, entity, dict) => {
        return schema.Relations.Any((r) => r.PrimaryNavigationName == pi.Name || r.ForeignNavigationName == pi.Name);
      };
    }

    public static Func<PropertyInfo, TEntity, Dictionary<string, object>, bool> LoadNavigations<TEntity>(
      SchemaRoot schema,
      Func<string, object[], EntityRef[]> getEntityRefsByKey,
      Func<Type, object[], object[]> getModelsByKey,
      Func<string, ExpressionTree, EntityRef[]> getEntityRefsBySearchExpression,
      Func<Type, ExpressionTree, object[]> getModelsBySearchExpression,
      NavigationRole navigationRoleFlags,
      bool loadMultipleNavigations,
      Type targetType = null
    ) {

      // TODO prevent circular references (Dependent -> Principal -> Dependent)

      bool includeLookups = (navigationRoleFlags & NavigationRole.Lookup) == NavigationRole.Lookup;
      bool includePrincipals = (navigationRoleFlags & NavigationRole.Principal) == NavigationRole.Principal;
      bool includeDependents = (navigationRoleFlags & NavigationRole.Dependent) == NavigationRole.Dependent;
      bool includeReferrers = (navigationRoleFlags & NavigationRole.Referrer) == NavigationRole.Referrer;

      //bool checkedPrimaryNavigatoinsWithoutProperties = false;

      //List<string> visitedTypeNames = new List<string>();
      //AsyncLocal<List<string>> visitedTypeNames = new AsyncLocal<List<string>>();
      //if (_VisitedTypeNames == null) {
      //  _VisitedTypeNames = new AsyncLocal<List<string>>();
      //}
      //if (_VisitedTypeNames.Value == null) {
      //  _VisitedTypeNames.Value = new List<string>();
      //}

      return (PropertyInfo pi, TEntity entity, Dictionary<string, object> dict) => {

        // TODO support multiple key fields

        if (
          _VisitedTypeNames != null &&
          _VisitedTypeNames.Value != null &&
          _VisitedTypeNames.Value.Count == 0
        ) {
          //_VisitedTypeNames = new AsyncLocal<List<string>>(); // Reset since we are at first property
          //_VisitedTypeNames.Value = new List<string>();
          CheckPrimaryNavigatoinsWithoutProperties(
            schema, entity,
            getEntityRefsBySearchExpression, getModelsBySearchExpression,
            includeDependents, includeReferrers, loadMultipleNavigations, targetType, dict
          );
          //checkedPrimaryNavigatoinsWithoutProperties = true;
        }

        if (
          _VisitedTypeNames != null &&
          _VisitedTypeNames.Value != null &&
          !_VisitedTypeNames.Value.Contains(entity.GetType().Name)
        ) {
          _VisitedTypeNames.Value.Add(entity.GetType().Name);
        }

        // Foreign navigation property
        RelationSchema foreignNavigationPropertyRelation = schema.Relations.FirstOrDefault(
          r => (
            r.ForeignNavigationName == pi.Name &&
            r.ForeignEntityName == typeof(TEntity).Name
          )
        );
        if (foreignNavigationPropertyRelation != null) {
          if (
            _VisitedTypeNames != null &&
            _VisitedTypeNames.Value != null &&
            _VisitedTypeNames.Value.Contains(foreignNavigationPropertyRelation.PrimaryEntityName)
          ) {
            return true;
          }
          bool isCircularReference = dict.Values.Any(
            v => v != null && v.GetType().Name == foreignNavigationPropertyRelation.PrimaryEntityName
          );
          return HandleForeignNavigationProperty(
            schema,
            getEntityRefsByKey, getModelsByKey, targetType,
            pi, entity, dict, includeLookups, includePrincipals, foreignNavigationPropertyRelation
          );
        }

        // Foreign key property
        RelationSchema foreignKeyRelation = schema.Relations.FirstOrDefault(
          r => (
            r.ForeignKeyIndexName == pi.Name && //TODO support multiple key fields
            r.ForeignEntityName == typeof(TEntity).Name
          )
        );

        if (foreignKeyRelation != null) {
          if (
            _VisitedTypeNames != null &&
            _VisitedTypeNames.Value != null &&
            _VisitedTypeNames.Value.Contains(foreignKeyRelation.PrimaryEntityName)) {
            return false;
          }
          return HandleForeigKeyProperty(
            schema,
            getEntityRefsByKey, getModelsByKey, targetType,
            pi, entity, dict, includeLookups, includePrincipals, foreignKeyRelation
          );
        }

        // Primary navigation property
        RelationSchema primaryNavigationPropertyRelation = schema.Relations.FirstOrDefault(
          r => (
            r.PrimaryNavigationName == pi.Name &&
            r.PrimaryEntityName == typeof(TEntity).Name
          )
        );
        if (primaryNavigationPropertyRelation != null) {
          if (
            _VisitedTypeNames != null &&
            _VisitedTypeNames.Value != null &&
            _VisitedTypeNames.Value.Contains(primaryNavigationPropertyRelation.ForeignEntityName)) {
            return true;
          }
          return HandlePrimaryNavigationProperty(
            getEntityRefsBySearchExpression, getModelsBySearchExpression,
            targetType,
            pi.Name, entity, dict, includeDependents, includeReferrers, primaryNavigationPropertyRelation,
            schema, loadMultipleNavigations
          );
        }

        return false;
      };
    }

    private static void CheckPrimaryNavigatoinsWithoutProperties(
      SchemaRoot schema,
      object entity,
      Func<string, ExpressionTree, EntityRef[]> getEntityRefsBySearchExpression,
      Func<Type, ExpressionTree, object[]> getModelsBySearchExpression,
      bool includeDependents,
      bool includeReferrers,
      bool loadMultipleNavigations,
      Type targetType,
      Dictionary<string, object> dict
    ) {
      IEnumerable<RelationSchema> primaryRelations = schema.Relations.Where(
        r => r.PrimaryEntityName == entity.GetType().Name &&
        string.IsNullOrEmpty(r.PrimaryNavigationName)
      );
      foreach (var primaryRelation in primaryRelations) {
        //if (_VisitedTypeNames.Value.Contains(primaryRelation.ForeignEntityName)) {
        //  continue;
        //}
        string targetPropertyName = string.Empty;
        if (targetType != null) {
          if (primaryRelation.ForeignEntityIsMultiple) {
            targetPropertyName = targetType.GetProperties().FirstOrDefault(
              p => p.PropertyType.IsGenericType &&
              typeof(IEnumerable).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()) &&
              p.PropertyType.GenericTypeArguments[0].Name == GetModelName(primaryRelation.ForeignEntityName)
            )?.Name;
          } else {
            targetPropertyName = targetType.GetProperties().FirstOrDefault(
              p => p.PropertyType.Name == primaryRelation.ForeignEntityName
            )?.Name;
          }
        }

        if (string.IsNullOrEmpty(targetPropertyName)) {
          continue;
        }
        HandlePrimaryNavigationProperty(
          getEntityRefsBySearchExpression,
          getModelsBySearchExpression,
          targetType,
          targetPropertyName,
          entity,
          dict,
          includeDependents, includeReferrers, primaryRelation, schema, loadMultipleNavigations
        );
      }
    }

    private static string GetModelName(string foreignEntityName) {
      if (foreignEntityName.EndsWith("Entity")) {
        return foreignEntityName.Substring(0, foreignEntityName.Length - 6);
      } else {
        return foreignEntityName;
      }
    }

    private static bool HandlePrimaryNavigationProperty(
      Func<string, ExpressionTree, EntityRef[]> getEntityRefsBySearchExpression,
      Func<Type, ExpressionTree, object[]> getModelsBySearchExpression,
      Type targetType,
      string propertyName,
      object entity,
      Dictionary<string, object> dict,
      bool includeDependents,
      bool includeReferrers,
      RelationSchema primaryNavigationPropertyRelation,
      SchemaRoot entitySchemaRoot,
      bool includeMultiples
    ) {
      // navigation could already be loaded by finding the fk property first
      if (dict.ContainsKey(propertyName)) {
        return true;
      }

      // check if Navigation should be loaded
      if (primaryNavigationPropertyRelation.IsLookupRelation && !includeReferrers) {
        return true;
      }
      if (!primaryNavigationPropertyRelation.IsLookupRelation && !includeDependents) {
        return true;
      }

      bool isMultiple = primaryNavigationPropertyRelation.ForeignEntityIsMultiple;
      if (isMultiple && !includeMultiples) {
        return true;
      }
      // check if the target model has the property
      Type targetPropertyType = (targetType == null)
        ? (isMultiple ? typeof(List<EntityRef>) : typeof(EntityRef))
        : targetType.GetProperty(propertyName)?.PropertyType;
      if (targetPropertyType == null) { return true; }

      string entityName = primaryNavigationPropertyRelation.ForeignEntityName;
      Type modelType = (isMultiple) ? (
        targetPropertyType.IsArray ?
        targetPropertyType.GetElementType() :
        targetPropertyType.GenericTypeArguments[0]
      ) : targetPropertyType;
      if (string.IsNullOrEmpty(primaryNavigationPropertyRelation.ForeignKeyIndexName)) {
        return true;
      }
      IEnumerable<object> models = GetModelsBySearchExpression(
        GetKey(entity, entitySchemaRoot),
        primaryNavigationPropertyRelation.ForeignKeyIndexName,
        entityName,
        modelType,
        getEntityRefsBySearchExpression,
        getModelsBySearchExpression
      );

      object navigationValueOnModel = models;
      if (!isMultiple) {
        navigationValueOnModel = models.FirstOrDefault();
      }

      dict.Add(propertyName, navigationValueOnModel);
      return true;

    }

    private static bool HandleForeigKeyProperty<TEntity>(
      SchemaRoot schemaRoot,
      Func<string, object[], EntityRef[]> getEntityRefsByKey,
      Func<Type, object[], object[]> getModelsByKey,
      Type targetType,
      PropertyInfo pi,
      TEntity entity,
      Dictionary<string, object> dict,
      bool includeLookups,
      bool includePrincipals,
      RelationSchema foreignKeyRelation
    ) {
      // check if Navigation should be loaded
      if (foreignKeyRelation.IsLookupRelation && !includeLookups) {
        return false;
      }
      if (!foreignKeyRelation.IsLookupRelation && !includePrincipals) {
        return false;
      }

      // find out corresponding navigation property name
      string navigationName = foreignKeyRelation.ForeignNavigationName;
      if (string.IsNullOrEmpty(navigationName)) {
        if (pi.Name.EndsWith("Id")) {
          navigationName = pi.Name.Substring(0, pi.Name.Length - 2);
        } else {
          navigationName = foreignKeyRelation.PrimaryEntityName + "Ref"; ;
        }
      }

      // check if the target model has the property
      Type targetPropertyType = (targetType == null)
        ? typeof(EntityRef) : targetType.GetProperty(navigationName)?.PropertyType;
      if (targetPropertyType == null) {
        return false;
      }

      // navigation could already be loaded by finding the navigation property first
      if (dict.ContainsKey(navigationName)) { return true; }

      object navigationValueOnModel = null;

      PropertyInfo sourceProperty = entity.GetType().GetProperty(navigationName);
      object navigationValueOnEntity = null;
      if (sourceProperty != null) {
        navigationValueOnEntity = sourceProperty.GetValue(entity, null);
      } else {
        return false;
      }
      if (navigationValueOnEntity != null) {
        if (typeof(EntityRef).IsAssignableFrom(targetPropertyType)) {
          navigationValueOnModel = new EntityRef() { Key = pi.GetValue(entity, null), Label = navigationValueOnEntity.ToString() };
        } else {
          Type sourceType = navigationValueOnEntity.GetNonDynamicType();
          navigationValueOnModel = navigationValueOnEntity.ToBusinessModel(
            sourceType, targetType, ConversionHelper.DismissNavigations(schemaRoot)
          );
        }
      } else {
        IEnumerable<object> models = GetModels(
          pi.GetValue(entity),
          foreignKeyRelation.PrimaryEntityName,
          targetPropertyType,
          getEntityRefsByKey,
          getModelsByKey
        );
        navigationValueOnModel = models.FirstOrDefault();
      }
      dict.Add(navigationName, navigationValueOnModel);

      return false; // still set the foreign key property in default behavior
    }

    private static bool HandleForeignNavigationProperty<TEntity>(
      SchemaRoot schemaRoot,
      Func<string, object[], EntityRef[]> getEntityRefsByKey,
      Func<Type, object[], object[]> getModelsByKey,
      Type targetType,
      PropertyInfo pi,
      TEntity entity,
      Dictionary<string, object> dict,
      bool includeLookups,
      bool includePrincipals,
      RelationSchema foreignNavigationPropertyRelation
    ) {
      // navigation could already be loaded by finding the fk property first
      if (dict.ContainsKey(pi.Name)) {
        return true;
      }

      // check if Navigation should be loaded
      if (foreignNavigationPropertyRelation.IsLookupRelation && !includeLookups) {
        return true;
      }
      if (!foreignNavigationPropertyRelation.IsLookupRelation && !includePrincipals) {
        return true;
      }

      // check if the target model has the property
      Type targetPropertyType = (targetType == null)
        ? typeof(EntityRef) : targetType.GetProperty(pi.Name)?.PropertyType;
      if (targetPropertyType == null) {
        return true;
      }

      object navigationValueOnEntity = pi.GetValue(entity);
      // if property on target model is assignable from the lookup value directly, just add it          
      // this should be a rare case, but it is possible
      if (targetPropertyType.IsAssignableFrom(navigationValueOnEntity.GetType())) {
        dict.Add(pi.Name, navigationValueOnEntity);
        return true;
      }
      PropertyInfo foreignKeyProperty = typeof(TEntity).GetProperty(
        foreignNavigationPropertyRelation.ForeignKeyIndexName
      );
      if (foreignKeyProperty == null) {
        return true;
      }

      if (navigationValueOnEntity != null) {
        if (typeof(EntityRef).IsAssignableFrom(targetPropertyType)) {
          dict.Add(pi.Name, new EntityRef() { Key = foreignKeyProperty.GetValue(entity), Label = navigationValueOnEntity.ToString() });
          return true;
        } else {
          Type sourceType = navigationValueOnEntity.GetNonDynamicType();
          dict.Add(
            pi.Name, navigationValueOnEntity.ToBusinessModel(
              sourceType, targetPropertyType, ConversionHelper.DismissNavigations(schemaRoot)
            )
          );
          return true;
        }
      }

      IEnumerable<object> models = GetModels(
        foreignKeyProperty.GetValue(entity),
        foreignNavigationPropertyRelation.PrimaryEntityName,
        targetPropertyType,
        getEntityRefsByKey,
        getModelsByKey
      );
      object navigationValueOnModel = models.FirstOrDefault();

      dict.Add(
        pi.Name,
        (navigationValueOnEntity == null) ? null : navigationValueOnModel
      );
      return true;
    }

    private static IEnumerable<object> GetModels(
      object key,
      string entityTypeName,
      Type modelType,
      Func<string, object[], EntityRef[]> getEntityRefsByKey,
      Func<Type, object[], object[]> getModelsByKey
    ) {
      if (key == null) { return new List<object>(); }
      if (typeof(EntityRef).IsAssignableFrom(modelType)) {
        return getEntityRefsByKey(
          entityTypeName, new object[] { key }
        );
      }
      return getModelsByKey(
        modelType, new object[] { key }
      );
    }
    #endregion

    #region ModelToEntity

    public static Func<PropertyInfo, Dictionary<string, object>, TEntity, bool> ResolveNavigations<TEntity>(
      SchemaRoot entitySchema
    ) {

      return (PropertyInfo pi, Dictionary<string, object> dict, TEntity entity) => {

        // TODO support multiple key fields  

        // Foreign key property
        RelationSchema foreignKeyRelation = entitySchema.Relations.FirstOrDefault(
          r => (
            r.ForeignKeyIndexName == pi.Name && //TODO support multiple key fields
            r.ForeignEntityName == typeof(TEntity).Name
          )
        );

        if (foreignKeyRelation != null) {
          // TODO if the navigation property is already loaded, don't overwrite it otherwise
          // try to resolve it by finding the navigation property in the dict

          // check if foreign key is a key in the dictionary
          if (dict.ContainsKey(pi.Name)) {
            return false; // foreign key will be set by the default behavior
          }

          string navigationName = foreignKeyRelation.ForeignNavigationName;
          if (string.IsNullOrEmpty(navigationName)) {
            if (pi.Name.EndsWith("Id")) {
              navigationName = pi.Name.Substring(0, pi.Name.Length - 2);
            } else {
              navigationName = foreignKeyRelation.PrimaryEntityName + "Ref"; ;
            }
          }
          if (!dict.ContainsKey(navigationName)) {
            return true;
          }
          object navPropValue = dict[navigationName];
          if (typeof(EntityRef).IsAssignableFrom(navPropValue.GetType())) {
            EntityRef entityRef = (EntityRef)dict[navigationName];
            object entityRefKey = entityRef.Key;
#if NETCOREAPP
            if (typeof(JsonElement).IsAssignableFrom(entityRefKey.GetType())) {
              entityRefKey = GetValueFromJsonElement((JsonElement)entityRefKey);
            }
#endif
            pi.SetValue(entity, entityRefKey);
            return true;
          } else {
            string primaryEntityName = foreignKeyRelation.PrimaryEntityName;
            Type primaryEntityType = entity.GetType().Assembly.GetTypes().FirstOrDefault((t) => t.Name == primaryEntityName);
            if (primaryEntityType == null) {
              return true;
            }
            Type targetType = navPropValue.GetType();
            List<PropertyInfo> keyProperties = entitySchema.GetPrimaryKeyProperties(primaryEntityType);
            object[] keyValues;

            if (typeof(IDictionary<string, object>).IsAssignableFrom(navPropValue.GetType())) {
              List<object> keyValuesList = new List<object>();
              IDictionary navPropValueDict = (IDictionary)navPropValue;
              foreach (PropertyInfo keyProperty in keyProperties) {
                if (!navPropValueDict.Contains(keyProperty.Name)) {
                  continue;
                }
                keyValuesList.Add(navPropValueDict[keyProperty.Name]);
              }
              if (keyValuesList.Count == 0) {
                if (navPropValueDict.Contains("Key")) {
                  keyValuesList.Add(navPropValueDict["Key"]);
                }
                if (navPropValueDict.Contains("key")) {
                  keyValuesList.Add(navPropValueDict["key"]);
                }
              }
              keyValues = keyValuesList.ToArray();
            } else {
              List<PropertyInfo> targetProperties = new List<PropertyInfo>();
              foreach (PropertyInfo keyProperty in keyProperties) {
                PropertyInfo targetProperty = targetType.GetProperty(keyProperty.Name);
                if (targetProperty == null) {
                  continue;
                }
                targetProperties.Add(targetProperty);
              }
              keyValues = navPropValue.GetValues(targetProperties);
            }
            object key = GetKey(keyProperties, keyValues);
            pi.SetValue(entity, key);
            return true;
          }

        }

        return false;
      };
    }

    #endregion

    private static IEnumerable<object> GetModelsBySearchExpression(
      object foreignKey,
      string foreignKeyName,
      string entityTypeName,
      Type modelType,
      Func<string, ExpressionTree, EntityRef[]> getEntityRefsBySearchExpression,
      Func<Type, ExpressionTree, object[]> getModelsBySearchExpression
    ) {
      if (typeof(EntityRef).IsAssignableFrom(modelType)) {
        return getEntityRefsBySearchExpression(
          entityTypeName, ExpressionTree.And(FieldPredicate.Equal(foreignKeyName, foreignKey))
        );
      }
      return getModelsBySearchExpression(
        modelType, ExpressionTree.And(FieldPredicate.Equal(foreignKeyName, foreignKey))
      );
    }

    public static object GetKey(object o, SchemaRoot schemaRoot) {
      List<PropertyInfo> keyProperties = schemaRoot.GetPrimaryKeyProperties(o.GetNonDynamicType());
      object[] keyValues = o.GetValues(keyProperties);
      return GetKey(keyProperties, keyValues);
    }

    public static string GetLabel(object o, SchemaRoot schemaRoot) {
      if (o == null) {
        return "";
      }
      Type type = o.GetNonDynamicType();
      EntitySchema entitySchema = schemaRoot.GetSchema(type.Name);
      if (entitySchema == null) {
        return "";
      }
      foreach (FieldSchema fieldSchema in entitySchema.Fields) {
        if (fieldSchema.IdentityLabel) {
          PropertyInfo property = type.GetProperty(fieldSchema.Name);
          if (property != null) {
            return property.GetValue(o).ToString();
          }
        }
      }
      return "";
    }

    public static Type GetNonDynamicType(this object o) {
      if (o == null) return null;
      Type result = o.GetType();
      if (result.Assembly.IsDynamic && result.BaseType != null) {
        result = result.BaseType;
      }
      return result;
    }

    private static object GetKey(List<PropertyInfo> keyProperties, object[] keyValues) {
      if (keyProperties.Count == 0) {
        throw new InvalidOperationException("No key properties found");
      }

      if (keyProperties.Count == 1) {
        return keyValues[0];
      }

      Type keyType = null;
      switch (keyProperties.Count) {
        case 2:
          keyType = typeof(CompositeKey2<,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
          break;
        case 3:
          keyType = typeof(CompositeKey3<,,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
          break;
        case 4:
          keyType = typeof(CompositeKey4<,,,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
          break;
        case 5:
          keyType = typeof(CompositeKey5<,,,,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
          break;
        default:
          throw new InvalidOperationException("Unsupported number of key properties");
      }

      return Activator.CreateInstance(keyType, keyValues);
    }

    public static object GetKeyType(List<PropertyInfo> keyProperties) {
      if (keyProperties.Count == 0) {
        throw new InvalidOperationException("No key properties found");
      }

      if (keyProperties.Count == 1) {
        return keyProperties[0];
      }

      Type keyType = null;
      switch (keyProperties.Count) {
        case 2:
          keyType = typeof(CompositeKey2<,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
          break;
        case 3:
          keyType = typeof(CompositeKey3<,,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
          break;
        case 4:
          keyType = typeof(CompositeKey4<,,,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
          break;
        case 5:
          keyType = typeof(CompositeKey5<,,,,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
          break;
        default:
          throw new InvalidOperationException("Unsupported number of key properties");
      }

      return keyType;
    }

    // gets the key type of the entity. 
    // Uses the SchemaRoot.GetPrimaryKeyProperties method to get the key properties of the entity.
    // if there is only one key property, returns the type of that property.
    // if there are multiple key properties, returns the type of the ComposityKey-class with the matching
    // number of fields.
    public static Type GetKeyType(Type entityType, SchemaRoot schemaRoot) {
      List<PropertyInfo> keyProperties = schemaRoot.GetPrimaryKeyProperties(entityType);

      // If there is only one key property, return its type
      if (keyProperties.Count == 1) {
        return keyProperties[0].PropertyType;
      }

      // If there are multiple key properties, return the type of the CompositeKey class
      // with the matching number of fields
      switch (keyProperties.Count) {
        case 2:
          return typeof(CompositeKey2<,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
        case 3:
          return typeof(CompositeKey3<,,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
        case 4:
          return typeof(CompositeKey4<,,,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
        case 5:
          return typeof(CompositeKey5<,,,,>).MakeGenericType(keyProperties.Select(p => p.PropertyType).ToArray());
        default:
          throw new InvalidOperationException("Unsupported number of key properties");
      }
    }

    public static EntityRef[] GetEntityRefs(
      string entityName,
      object[] keys,
      Type entryType, IDataStore dataStore,
      SchemaRoot schemaRoot
    ) {
      Assembly assembly = Assembly.GetAssembly(entryType);
      Type entityType = assembly.GetTypes().FirstOrDefault((Type t) => t.Name == entityName);
      if (entityType == null) {
        return new EntityRef[] { };
      }
      Type keyType = GetKeyType(entityType, schemaRoot);
      MethodInfo getRepositoryMethod = dataStore.GetType().GetMethod(nameof(IDataStore.GetRepository));
      getRepositoryMethod = getRepositoryMethod.MakeGenericMethod(entityType, keyType);
      object repository = getRepositoryMethod.Invoke(dataStore, new object[] { });
      MethodInfo getEntityRefsByKeyMethod = repository.GetType().GetMethod(
        nameof(IRepository<object, object>.GetEntityRefsByKey)
      );
      object typedKeysArray = Array.CreateInstance(keyType, keys.Length);
      for (int i = 0; i < keys.Length; i++) {
        ((Array)typedKeysArray).SetValue(keys[i], i);
      }
      return (EntityRef[])getEntityRefsByKeyMethod.Invoke(
        repository, new object[] { typedKeysArray }
      );
    }

    public static EntityRef[] GetEntityRefsBySearchExpression(
      string entityName,
      ExpressionTree searchExpression,
      Type entryType, IDataStore dataStore,
      SchemaRoot schemaRoot
    ) {
      Assembly assembly = Assembly.GetAssembly(entryType);
      Type entityType = assembly.GetTypes().FirstOrDefault((Type t) => t.Name == entityName);
      if (entityType == null) {
        return new EntityRef[] { };
      }
      Type keyType = GetKeyType(entityType, schemaRoot);
      MethodInfo getRepositoryMethod = dataStore.GetType().GetMethod(nameof(IDataStore.GetRepository));
      getRepositoryMethod = getRepositoryMethod.MakeGenericMethod(entityType, keyType);
      object repository = getRepositoryMethod.Invoke(dataStore, new object[] { });
      MethodInfo getEntityRefsBySearchExpressionMethod = repository.GetType().GetMethod(
        nameof(IRepository<object, object>.GetEntityRefs)
      );

      return (EntityRef[])getEntityRefsBySearchExpressionMethod.Invoke(
        repository, new object[] { searchExpression, new string[] { }, 0, 0 }
      );
    }

    public static object[] GetEntities(
      Type entityType,
      object[] keys,
      IDataStore dataStore,
      SchemaRoot schemaRoot
    ) {
      Type keyType = GetKeyType(entityType, schemaRoot);
      MethodInfo getRepositoryMethod = dataStore.GetType().GetMethod(nameof(IDataStore.GetRepository));
      getRepositoryMethod = getRepositoryMethod.MakeGenericMethod(entityType, keyType);
      object repository = getRepositoryMethod.Invoke(dataStore, new object[] { });
      MethodInfo getEntitiesByKeyMethod = repository.GetType().GetMethod(
        nameof(IRepository<object, object>.GetEntitiesByKey)
      );
      object typedKeysArray = Array.CreateInstance(keyType, keys.Length);
      for (int i = 0; i < keys.Length; i++) {
        ((Array)typedKeysArray).SetValue(keys[i], i);
      }
      return (object[])getEntitiesByKeyMethod.Invoke(repository, new object[] { typedKeysArray });
    }

    public static object[] GetEntitiesBySearchExpression(
      Type entityType,
      ExpressionTree searchExpression,
      IDataStore dataStore,
      SchemaRoot schemaRoot
    ) {
      Type keyType = GetKeyType(entityType, schemaRoot);
      MethodInfo getRepositoryMethod = dataStore.GetType().GetMethod(nameof(IDataStore.GetRepository));
      getRepositoryMethod = getRepositoryMethod.MakeGenericMethod(entityType, keyType);
      object repository = getRepositoryMethod.Invoke(dataStore, new object[] { });
      MethodInfo getEntitiesBySearchExpressionMethod = repository.GetType().GetMethod(
        nameof(IRepository<object, object>.GetEntities)
      );
      return (object[])getEntitiesBySearchExpressionMethod.Invoke(
        repository, new object[] {
          searchExpression, new string[] { }, 100, 0
        }
      );
    }

    public static void SanitizeDict(ref Dictionary<string, object> dict) {
      Dictionary<string, object> result = new Dictionary<string, object>();
      foreach (KeyValuePair<string, object> keyValuePair in dict) {
        object value = keyValuePair.Value;
#if NETCOREAPP
        if (typeof(JsonElement).IsAssignableFrom(value.GetType())) {
          result.Add(keyValuePair.Key, GetValueFromJsonElement((JsonElement)keyValuePair.Value));
        } else {
          result.Add(keyValuePair.Key, keyValuePair.Value);
        }
#else
        result.Add(keyValuePair.Key, keyValuePair.Value);
#endif
      }
      dict = result;
    }

#if NETCOREAPP
    public static object GetValueFromJsonElement(JsonElement jsonElement) {
      switch (jsonElement.ValueKind) {
        case JsonValueKind.String:
          if (jsonElement.TryGetDateTime(out DateTime dateValue)) {
            return dateValue;
          } else if (Guid.TryParse(jsonElement.GetString(), out Guid guidValue)) {
            return guidValue;
          } else if (jsonElement.GetString().Length == 1) {
            return jsonElement.GetString()[0];
          } else {
            return jsonElement.GetString();
          }
        case JsonValueKind.Number:
          if (jsonElement.TryGetInt32(out int intValue)) {
            return intValue;
          } else if (jsonElement.TryGetInt64(out long longValue)) {
            return longValue;
          } else if (jsonElement.TryGetDouble(out double doubleValue)) {
            return doubleValue;
          }
          break;
        case JsonValueKind.True:
        case JsonValueKind.False:
          return jsonElement.GetBoolean();
        case JsonValueKind.Array:
          return jsonElement.EnumerateArray().Select(GetValueFromJsonElement).ToArray();
        case JsonValueKind.Object:
          var dictionary = new Dictionary<string, object>();
          foreach (var property in jsonElement.EnumerateObject()) {
            dictionary[property.Name] = GetValueFromJsonElement(property.Value);
          }
          return dictionary;
        case JsonValueKind.Null:
          return null;
        default:
          throw new NotSupportedException($"Unsupported JSON value kind: {jsonElement.ValueKind}");
      }

      throw new JsonException($"Unable to parse JSON element: {jsonElement}");
    }

    public static object GetValueFromJsonElementByType(JsonElement jsonElement, Type targetType) {
      object value = GetValueFromJsonElement(jsonElement);
      if (targetType.IsAssignableFrom(value.GetType())) {
        return value;
      }
      string valueAsString;
      if (value.GetType() == typeof(string)) {
        valueAsString = (string)value;
      } else if (value.GetType() == typeof(char)) {
        valueAsString = value.ToString();
      } else {
        return value;
      }

      if (targetType == typeof(Guid)) {
        return Guid.Parse(valueAsString);
      } else if (targetType == typeof(DateTime)) {
        return DateTime.Parse(valueAsString);
      } else if (targetType == typeof(int)) {
        return int.Parse(valueAsString);
      } else if (targetType == typeof(long)) {
        return long.Parse(valueAsString);
      } else if (targetType == typeof(double)) {
        return double.Parse(valueAsString);
      } else if (targetType == typeof(decimal)) {
        return decimal.Parse(valueAsString);
      } else if (targetType == typeof(bool)) {
        return bool.Parse(valueAsString);
      } else if (targetType == typeof(char)) {
        return char.Parse(valueAsString);
      } else {
        return valueAsString;
      }
    }

#endif

    public static object SanitizeObject(object obj, Type targetType) {
#if NETCOREAPP
      if (typeof(JsonElement).IsAssignableFrom(obj.GetType())) {
        return ((JsonElement)obj).Deserialize(targetType);
      }
#endif

      return obj;

    }
  }

}
