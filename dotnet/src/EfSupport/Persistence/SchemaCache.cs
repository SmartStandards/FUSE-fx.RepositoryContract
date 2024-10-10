#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif
using System.Collections.Generic;
using System.Data.ModelDescription;
using System.Linq;
using System.Reflection;

namespace System.Data.Fuse.Ef {

  internal static class SchemaCache {

    private static Dictionary<Type, SchemaRoot> _SchemaRootsPerContextType = new Dictionary<Type, SchemaRoot>();

    /// <summary>
    /// Using 'ModelReader.GetSchemaForDbContext' to enumerate the entities base
    /// static reflection analysis (DbSets and crawled Navigation-Properties)
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    /// <returns></returns>
    public static SchemaRoot GetSchemaRootForContext<TDbContext>()
    where TDbContext : DbContext {
      return ModelReader.GetSchemaForDbContext<TDbContext>();
    }

    /// <summary>
    /// Using 'ModelReader.GetSchemaForDbContext' to enumerate the entities base
    /// static reflection analysis (DbSets and crawled Navigation-Properties)
    /// </summary>
    /// <param name="dbContextType"></param>
    /// <returns></returns>
    public static SchemaRoot GetSchemaRootForContext(Type dbContextType) {
      return ModelReader.GetSchemaForDbContext(dbContextType);
    }

    public static SchemaRoot GetSchemaRootForContext<TDbContext>(Assembly entityAssembly)
    where TDbContext : DbContext {
      return GetSchemaRootForContext(typeof(TDbContext), entityAssembly);
    }
    public static SchemaRoot GetSchemaRootForContext(DbContext dbContext, Assembly entityAssembly) {
      lock (_SchemaRootsPerContextType) {
        Type dbContextType = dbContext.GetType();
        SchemaRoot result = null;
        if (!_SchemaRootsPerContextType.TryGetValue(dbContextType, out result)) {
          string[] entityTypeNames = GetEntityTypeNamesForContext(dbContext);
          result = ModelReader.GetSchema(entityAssembly, entityTypeNames);
          _SchemaRootsPerContextType[dbContextType] = result;
        }
        return result;
      }

    }

    public static SchemaRoot GetSchemaRootForContext(Type dbContextType, Assembly entityAssembly) {

      lock (_SchemaRootsPerContextType) {
        SchemaRoot result = null;
        if (!_SchemaRootsPerContextType.TryGetValue(dbContextType, out result)) {
          string[] entityTypeNames = GetEntityTypeNamesForContext(dbContextType);
          result = ModelReader.GetSchema(entityAssembly, entityTypeNames);
          _SchemaRootsPerContextType[dbContextType] = result;
        }
        return result;
      }

    }

    private static Dictionary<Type, string[]> _EntityTypeNamesPerContextType = new Dictionary<Type, string[]>();

    public static string[] GetEntityTypeNamesForContext<TDbContext>()
    where TDbContext : DbContext {
      return GetEntityTypeNamesForContext(typeof(TDbContext));
    } 

    public static string[] GetEntityTypeNamesForContext(Type dbContextType) {
      using (DbContext dbContext = (DbContext)Activator.CreateInstance(dbContextType)) {
        return GetEntityTypeNamesForContext(dbContext);
      }
    }

    public static string[] GetEntityTypeNamesForContext(DbContext dbContext) {
      lock (_EntityTypeNamesPerContextType) {
        Type dbContextType = dbContext.GetType();
        string[] result = null;
        if (!_EntityTypeNamesPerContextType.TryGetValue(dbContextType, out result)) {
#if NETCOREAPP
          result = dbContext.Model.GetEntityTypes().Select(t => t.Name).ToArray();
#else
          result = dbContext.GetManagedTypeNames();
#endif
          _EntityTypeNamesPerContextType[dbContextType] = result;
        }
        return result;
      }

    }

  }

}
