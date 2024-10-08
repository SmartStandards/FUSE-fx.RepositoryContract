#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Metadata.Edm;
#endif
using System.Data.Fuse.Ef;
using System.Data.ModelDescription;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Data.Fuse.Ef.InstanceManagement;
using System.Runtime.InteropServices;

namespace System.Data.Fuse.SchemaResolving {

  /// <summary>
  /// Provides functionality to get a concrete Type from a entity name (as used in the Schema/ModelDescription).
  /// The 'DbContextRuntimeEntityResolver' provides the known types from a internally evaluated list, which
  /// is created by requesting the entity framework schema information from a DbContext instance.
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  [DebuggerDisplay("DbContextRuntimeEntityResolver ({_InspectedDbContextType.FullName})")]
  public class DbContextRuntimeEntityResolver : IEntityResolver {

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private Type[] _KnownEntityTypes = null;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Type _InspectedDbContextType = null;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IDbContextInstanceProvider _ContextInstanceProvider;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Func<DbContext> _DbContextFactory = null;

    /// <summary>
    /// Provides functionality to get a concrete Type from a entity name (as used in the Schema/ModelDescription).
    /// The 'DbContextRuntimeEntityResolver' provides the known types from a internally evaluated list, which
    /// is created by requesting the entity framework schema information from a DbContext instance.
    /// To get access to an DbContext instance, the given dbContextVisitingMethod will be invoked.
    /// </summary>
    public DbContextRuntimeEntityResolver(IDbContextInstanceProvider contextInstanceProvider, bool lazy) {
      _ContextInstanceProvider = contextInstanceProvider;
      if (!lazy) {
        Collect();
      }
    }

    /// <summary>
    /// Provides functionality to get a concrete Type from a entity name (as used in the Schema/ModelDescription).
    /// The 'DbContextRuntimeEntityResolver' provides the known types from a internally evaluated list, which
    /// is created by requesting the entity framework schema information from a DbContext instance.
    /// To get access to an DbContext instance, a short-living context will be initialized using
    /// the given factory, and disposed afterwards.
    /// </summary>
    public DbContextRuntimeEntityResolver(Func<DbContext> dbContextFactory, bool lazy) {
      _DbContextFactory = dbContextFactory;
      if (!lazy) {
        Collect();
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
        Collect();
      }
      return _KnownEntityTypes;
    }

    private void Collect() {
      _KnownEntityTypes = new Type[] { };
      if (_ContextInstanceProvider != null) {
        _ContextInstanceProvider.VisitCurrentDbContext((context) => {
          _InspectedDbContextType = context.GetType();
          _KnownEntityTypes = CollectFrom(context);
        });
      }
      else if (_DbContextFactory != null) {
        using (var context = _DbContextFactory.Invoke()) {
          _InspectedDbContextType = context.GetType();
          _KnownEntityTypes = CollectFrom(context);
        }
      }
    }

    private static Type[] CollectFrom(DbContext dbContext) {
#if NETCOREAPP
      return dbContext.Model.GetEntityTypes().Select(t => t.ClrType).ToArray();
#else
      //this line can take a lot of time :-(
      var objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;

      var managedTypes = objectContext.MetadataWorkspace
          .GetItemCollection(DataSpace.OSpace)
          .GetItems<EntityType>()
          .ToArray();

      Assembly lastMatchAssembly = null;
      return managedTypes.Select((t) => {
        Type type;
        if(lastMatchAssembly != null) {
          type = lastMatchAssembly.GetType(t.FullName);
          if (type != null) {
            return type;
          }
        }
        foreach (Assembly loadedAss in AppDomain.CurrentDomain.GetAssemblies()) {
          type = loadedAss.GetType(t.FullName);
          if(type != null) {
            lastMatchAssembly = loadedAss;
            return type;
          }
        }
        return null;
      }).ToArray();
#endif
    }

  }

}
