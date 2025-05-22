using System.Convenience;
using System.Data.Fuse;

namespace TechDemo.WebApi.Persistence {
  public class TechDemoMveDataStore : ModelVsEntityDataStore {
    public TechDemoMveDataStore(
      Tuple<Type, Type>[] managedTypes
    ) : base(new TechDemoEfDataStore(), managedTypes) {
    }
  }
}
