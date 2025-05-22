using System.Data.Fuse.Ef;
using System.Data.Fuse.Ef.InstanceManagement;

namespace TechDemo.WebApi.Persistence {
  public class TechDemoEfDataStore : EfDataStore<TechDemoDbContext> {
    public TechDemoEfDataStore(
    ) : base(new ShortLivingDbContextInstanceProvider<TechDemoDbContext>()) {
    }
  }
}
