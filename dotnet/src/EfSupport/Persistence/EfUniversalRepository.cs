﻿#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif
using System.Reflection;
using System.Linq;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Collections.Generic;
using System.Data.ModelDescription.Convenience;

namespace System.Data.Fuse.Ef {

  /// <summary>
  /// 
  /// </summary>
  public class EfUniversalRepository : UniversalRepository {

    protected readonly DbContext _DbContext;
    protected readonly Assembly _Assembly;

    private static Dictionary<string, SchemaRoot> _SchemaRootByAssemblyName;

    protected static SchemaRoot GetSchemaRoot(Type entityType, DbContext dbContext) {

      if (_SchemaRootByAssemblyName == null) {
        _SchemaRootByAssemblyName = new Dictionary<string, SchemaRoot>();
      }
      if (_SchemaRootByAssemblyName.ContainsKey(entityType.Assembly.FullName)) {
        return _SchemaRootByAssemblyName[entityType.Assembly.FullName];
      }
#if NETCOREAPP
        string[] typenames = dbContext.Model.GetEntityTypes().Select(t => t.Name).ToArray();
#else
          string[] typenames = dbContext.GetManagedTypeNames();
#endif
      SchemaRoot schemaRoot = ModelReader.GetSchema(entityType.Assembly, typenames);
      _SchemaRootByAssemblyName[entityType.Assembly.FullName] = schemaRoot;
      return schemaRoot;

    }

    // gets the key type of the entity. 
    // Uses the SchemaRoot.GetPrimaryKeyProperties method to get the key properties of the entity.
    // if there is only one key property, returns the type of that property.
    // if there are multiple key properties, returns the type of the ComposityKey-class with the matching
    // number of fields.
    private static Type GetKeyType(Type entityType, DbContext dbContext) {
      SchemaRoot schemaRoot = GetSchemaRoot(entityType, dbContext);
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

    public EfUniversalRepository(DbContext dbContext, Assembly assembly) {
      this._DbContext = dbContext;
      this._Assembly = assembly;
    }

    protected override UniversalRepositoryFacade CreateInnerRepo(string entityName) {
      Type[] allTypes = _Assembly.GetTypes();
      Type entityType = allTypes.Where((Type t) => t.Name == entityName).FirstOrDefault();
      if (entityType == null) { return null; }

      Type keyType = GetKeyType(entityType, _DbContext);

      Type repoFacadeType = typeof(DynamicRepositoryFacade<,>);
      repoFacadeType = repoFacadeType.MakeGenericType(entityType, keyType);

      Type repoType = typeof(EfRepository<,>);
      repoType = repoType.MakeGenericType(entityType, keyType);
      object repo = Activator.CreateInstance(repoType, _DbContext, GetSchemaRoot(entityType, _DbContext));

      return (UniversalRepositoryFacade)Activator.CreateInstance(repoFacadeType, repo);
    }

  }

}
