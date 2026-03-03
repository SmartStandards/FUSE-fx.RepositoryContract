using System;
using System.Data.Fuse.Ef;
using System.Data.Fuse.Sql.InstanceManagement;
using System.Data.ModelDescription;
using System.Data.SqlClient;

namespace RepositoryContract.Demo.WebApi.DynamicSqlRepoDemo {
  public class DemoSqlDataStore : SqlDataStore {
    public DemoSqlDataStore() : base(
      new ShortLivingDbConnectionInstanceProvider(
        () => new SqlConnection(
          "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RepositoryContractTests;Integrated Security=True;"
        )
      ),
      new Tuple<Type, Type>[] {
            new Tuple<Type, Type>(typeof(BavPerson), typeof(int))
      }
    ) {
    }
  }
}
