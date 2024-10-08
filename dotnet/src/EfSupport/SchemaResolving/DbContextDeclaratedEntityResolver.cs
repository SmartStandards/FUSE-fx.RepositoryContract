#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Metadata.Edm;
#endif
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Fuse.Ef;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace System.Data.Fuse.SchemaResolving {

  /// <summary>
  /// Provides functionality to get a concrete Type from a entity name (as used in the Schema/ModelDescription).
  /// The 'DbContextDeclaratedEntityResolver' provides the known types from a internally evaluated list, which
  /// is created via reflection based analysis of the DbContext-Type. It will cover all entites, which have
  /// their own declarated DbSet and all related entites reachable via navigation property.
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  [DebuggerDisplay("DbContextDeclaratedEntityResolver ({_DbContextType.Name})")]
  public class DbContextDeclaratedEntityResolver : IEntityResolver {

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private Type[] _KnownEntityTypes = null;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Type _DbContextType = null;

    /// <summary>
    /// Provides functionality to get a concrete Type from a entity name (as used in the Schema/ModelDescription).
    /// The 'DbContextDeclaratedEntityResolver' provides the known types from a internally evaluated list, which
    /// is created via reflection based analysis of the DbContext-Type. It will cover all entites, which have
    /// their own declarated DbSet and all related entites reachable via navigation property.
    /// </summary>
    public static DbContextDeclaratedEntityResolver CreateFor<TDbContext>(bool lazy)
      where TDbContext: DbContext {
      return new DbContextDeclaratedEntityResolver(typeof(TDbContext), lazy);
    }

    /// <summary>
    /// Provides functionality to get a concrete Type from a entity name (as used in the Schema/ModelDescription).
    /// The 'DbContextDeclaratedEntityResolver' provides the known types from a internally evaluated list, which
    /// is created via reflection based analysis of the DbContext-Type. It will cover all entites, which have
    /// their own declarated DbSet and all related entites reachable via navigation property.
    /// </summary>
    public DbContextDeclaratedEntityResolver(Type dbContextType, bool lazy) {
      _DbContextType = dbContextType;
      if (!lazy) {
        _KnownEntityTypes = GetEntityTypesViaReflection(_DbContextType);
      }
    }

    /// <summary>
    /// Runs a looup over the list of known entity types to find one with the given name.
    /// Otherwiese it will return null.
    /// </summary>
    /// <param name="entityTypeName">Name as provided from the Schema/ModelDescription</param>
    /// <returns></returns>
    public Type TryResolveEntityTypeByName(string entityTypeName) {

      foreach (Type t in this.GetWellknownTypes()) {
        if (t.Name.Equals(entityTypeName, StringComparison.CurrentCultureIgnoreCase)) {
          return t;
        }
      }

      foreach (Type t in this.GetWellknownTypes()) {
        if (t.FullName.Equals(entityTypeName, StringComparison.CurrentCultureIgnoreCase)) {
          return t;
        }
      }

      return null;
    }

    public Type[] GetWellknownTypes() {
      if (_KnownEntityTypes == null) {
        _KnownEntityTypes = GetEntityTypesViaReflection(_DbContextType);
      }
      return _KnownEntityTypes;
    }

    private static Type[] GetEntityTypesViaReflection(Type dbContextTypen) {
      List<Type> entityTypes = new List<Type>();
      var props = dbContextTypen.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
      var dbSetProps = props.Where((p) => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition().Name.StartsWith("DbSet"));
      var rootEntityTypes = dbSetProps.Select((p) => p.PropertyType.GetGenericArguments()[0]).ToList();
      foreach (Type rootEntityType in rootEntityTypes) {
        CrawlNavigationTree(entityTypes, rootEntityType);
      }
      return entityTypes.ToArray();
    }

    private static void CrawlNavigationTree(List<Type> collection, Type current) {
      if (collection.Contains(current)) {
        return;
      }
      collection.Add(current);
      foreach (PropertyInfo propertyInfo in current.GetProperties()) {
        var attribs = propertyInfo.GetCustomAttributes();
        if (
          attribs.OfType<PrincipalAttribute>().Any() ||
          attribs.OfType<LookupAttribute>().Any() ||
          attribs.OfType<DependentAttribute>().Any() ||
          attribs.OfType<ReferrerAttribute>().Any()
        ) {
          CrawlNavigationTree(collection, propertyInfo.PropertyType.GetUnwrappedType());
        }
      }
    }

  }

}
