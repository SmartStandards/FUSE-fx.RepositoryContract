#if NETCOREAPP
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Fuse.Ef {

  public static class DbContextExtensions {
    public static string[] GetManagedTypeNames(this DbContext context) {
      var objectContext = ((IObjectContextAdapter)context).ObjectContext;
      var managedTypes = objectContext.MetadataWorkspace
          .GetItemCollection(DataSpace.OSpace)
          .GetItems<EntityType>()
          .ToArray();
      return managedTypes.Select(t => t.Name).ToArray();
      //return managedTypes;
    }
  }

}

#endif