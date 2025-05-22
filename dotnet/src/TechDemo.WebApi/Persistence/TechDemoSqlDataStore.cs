using System.Data.Fuse.Ef;
using System.Data.Fuse.Sql.InstanceManagement;
using System.Data.ModelDescription;
using System.Data.SqlClient;

namespace TechDemo.WebApi.Persistence {
  public class TechDemoSqlDataStore : SqlDataStore {
    public TechDemoSqlDataStore(
      Tuple<Type, Type>[] managedTypes,
      Func<EntitySchema, string> tableNameGetter = null
    ) : base(
      new ShortLivingDbConnectionInstanceProvider(() => {
        return new SqlConnection(@"Server=(localdb)\mssqllocaldb;Database=TechDemoDbContext");
      }
    ), managedTypes, tableNameGetter
    ) {
    }
  }
}
