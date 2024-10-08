#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Metadata.Edm;
#endif
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace System.Data.Fuse.Ef {


  public static class DbContextExtensions {

#if NETCOREAPP
#else
    public static string[] GetManagedTypeNames(this DbContext dbContext) {
      var objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;
      var managedTypes = objectContext.MetadataWorkspace
          .GetItemCollection(DataSpace.OSpace)
          .GetItems<EntityType>()
          .ToArray();
      return managedTypes.Select(t => t.Name).ToArray();
    }
#endif

    public static string GetGeneratedOriginName(this DbContext dbContext) {
      using (MD5 md5 = MD5.Create()) {
#if NETCOREAPP
        byte[] inputBytes = Encoding.UTF8.GetBytes(dbContext.Database.GetDbConnection().ConnectionString);
#else  
        byte[] inputBytes = Encoding.UTF8.GetBytes(dbContext.Database.Connection.DataSource);
#endif
        byte[] hashBytes = md5.ComputeHash(inputBytes);
        StringBuilder sb = new StringBuilder();
        foreach (byte b in hashBytes) {
          sb.Append(b.ToString("x2"));
        }
        return "EF-" + sb.ToString();
      }
    }

    //private static Dictionary<Type, Type[]> _AvailableTypesPerContextType = new Dictionary<Type, Type[]>();
    //public static Type ResolveEntityTypeViaReflection(this DbContext dbContext, string entityName) {
    //  Type contextType = dbContext.GetType();
    //  Type[] availableTypes;
    //  lock (_AvailableTypesPerContextType) {
    //    if (_AvailableTypesPerContextType.ContainsKey(contextType)) {
    //      availableTypes = _AvailableTypesPerContextType[contextType];
    //    }
    //    else {
    //      availableTypes = dbContext.GetEntityTypesViaReflection();
    //      _AvailableTypesPerContextType[contextType] = availableTypes;
    //    }
    //  }
    //  return availableTypes.Where((t) => t.Name.Equals(entityName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
    //}

    //public static Type[] GetEntityTypesViaReflection(this DbContext dbContext) {
    //  List<Type> entityTypes = new List<Type>();
    //  var props = dbContext.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
    //  var dbSetProps = props.Where((p) => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition().Name.StartsWith("DbSet"));
    //  var rootEntityTypes = dbSetProps.Select((p) => p.PropertyType.GetGenericArguments()[0]).ToList();
    //  foreach (Type rootEntityType in rootEntityTypes) {
    //    CrawlNavigationTree(entityTypes, rootEntityType);
    //  }
    //  return entityTypes.ToArray();
    //}
    //private static void CrawlNavigationTree(List<Type> collection, Type current) {
    //  if (collection.Contains(current)) {
    //    return;
    //  }
    //  collection.Add(current);
    //  foreach (PropertyInfo propertyInfo in current.GetProperties()) {
    //    var attribs = propertyInfo.GetCustomAttributes();
    //    if (
    //      attribs.OfType<PrincipalAttribute>().Any() ||
    //      attribs.OfType<LookupAttribute>().Any() ||
    //      attribs.OfType<DependentAttribute>().Any() ||
    //      attribs.OfType<ReferrerAttribute>().Any()
    //    ) {
    //      CrawlNavigationTree(collection, propertyInfo.PropertyType.GetUnwrappedType());
    //    }
    //  }
    //}

    internal static Type GetUnwrappedType(this Type extendee) {
      if (extendee == null || extendee.FullName == "System.Void") {
        return null;
      }
      if (extendee.IsByRef) {
        extendee = extendee.GetElementType();
      }
      if (extendee.IsArray) {
        extendee = extendee.GetElementType();
      }
      else if (extendee.IsGenericType) {
        var genBase = extendee.GetGenericTypeDefinition();
        var genArg1 = extendee.GetGenericArguments()[0];
        if (typeof(List<>).MakeGenericType(genArg1).IsAssignableFrom(extendee)) {
          extendee = genArg1;
        }
        else if (typeof(Collection<>).MakeGenericType(genArg1).IsAssignableFrom(extendee)) {
          extendee = genArg1;
        }
        if (genBase == typeof(Nullable<>)) {
          extendee = genArg1;
        }
      }
      return extendee;
    }

  }

}
