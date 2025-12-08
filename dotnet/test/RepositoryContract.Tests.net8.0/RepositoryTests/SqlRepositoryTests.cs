using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryContract.Tests;
using System;
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
  public class SqlRepositoryTests : RepositoryTestsBase {

    protected override IRepository<LeafEntity1, int> CreateLeaf1EntityRepository() {
      SchemaRoot schemaRoot = ModelReader.GetSchema(new Type[] {
        typeof(RootEntity1),
        typeof(LeafEntity1),
        typeof(ChildEntity1),
        typeof(LeafEntityWithCompositeKey)
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

  }
}
