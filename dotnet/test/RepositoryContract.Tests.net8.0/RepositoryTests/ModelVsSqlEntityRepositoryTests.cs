using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Convenience;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.Fuse.Ef;
using System.Data.Fuse.Ef.InstanceManagement;
using System.Data.Fuse.Sql;
using System.Data.Fuse.Sql.InstanceManagement;
using System.Data.ModelDescription;
using System.Data.SqlClient;
using System.Linq;

namespace RepositoryTests {

  [TestClass]
  public class ModelVsSqlEntityRepositoryTests : ModelVsEntityRepositoryTestsBase {

    protected override IRepository<LeafEntity1, int> CreateLeaf1EntityRepository() {
      return new ModelVsEntityRepository<LeafEntity1, LeafEntity1, int>(
        CreateSqlRepository(),
        new ModelVsEntityParams<LeafEntity1, LeafEntity1>()
      );
    }

    private IRepository<LeafEntity1, int> CreateSqlRepository() {
      SchemaRoot schemaRoot = ModelReader.GetSchema(new Type[] {
        typeof(RootEntity1),
        typeof(LeafEntity1),
        typeof(ChildEntity1)
      }, false);
      using (SqlConnection c = new SqlConnection(
          "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RepositoryContractTests;Integrated Security=True;"
      )) {
        SqlSchemaInstaller.EnsureSchemaIsInstalled(c, schemaRoot);
      }
      ;
      return new SqlRepository<LeafEntity1, int>(
        new ShortLivingDbConnectionInstanceProvider(
          () => new SqlConnection(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RepositoryContractTests;Integrated Security=True;"
          )
        ),
        schemaRoot
      );
    }

    protected override IRepository<LeafEntity2, int> CreateLeafEntity2Repository() {
      return new ModelVsEntityRepository<LeafEntity2, LeafEntity2, int>(
        CreateSqlRepository2(),
        new ModelVsEntityParams<LeafEntity2, LeafEntity2>()
      );
    }

    protected IRepository<LeafEntity2, int> CreateSqlRepository2() {
      SchemaRoot schemaRoot = ModelReader.GetSchema(new Type[] {
        typeof(RootEntity2),
        typeof(LeafEntity2),
        typeof(ChildEntity2)
      }, false);
      using (SqlConnection c = new SqlConnection(
          "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RepositoryContractTests;Integrated Security=True;"
      )) {
        SqlSchemaInstaller.EnsureSchemaIsInstalled(c, schemaRoot);
      }
      ;
      return new SqlRepository<LeafEntity2, int>(
        new ShortLivingDbConnectionInstanceProvider(
          () => new SqlConnection(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RepositoryContractTests;Integrated Security=True;"
          )
        ),
        schemaRoot
      );
    }

    protected override IDataStore CreateEntityDatastore() {
      return new SqlDataStore(
        new ShortLivingDbConnectionInstanceProvider(
          () => new SqlConnection(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RepositoryContractTests;Integrated Security=True;"
          )
        ), new Tuple<Type, Type>[] {
          Tuple.Create<Type, Type>(typeof(LeafEntity1), typeof(int)),
          Tuple.Create<Type, Type>(typeof(RootEntity1), typeof(int)),
          Tuple.Create<Type, Type>(typeof(ChildEntity1), typeof(int)),
          Tuple.Create<Type, Type>(typeof(LeafEntity2), typeof(int)),
          Tuple.Create<Type, Type>(typeof(RootEntity2), typeof(int)),
          Tuple.Create<Type, Type>(typeof(ChildEntity2), typeof(int)),
          Tuple.Create<Type, Type>(typeof(LeafEntityWithCompositeKey), typeof(CompositeKey2<int,string>))
        }
      );
    }

  }
}
