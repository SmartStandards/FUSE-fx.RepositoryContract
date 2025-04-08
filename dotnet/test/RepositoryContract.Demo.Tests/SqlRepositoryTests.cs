using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryContract.Demo.Model;
using RepositoryContract.Demo.WebApi.Persistence;
using System;
using System.Collections.Generic;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.Fuse.Ef;
using System.Data.Fuse.Ef.InstanceManagement;
using System.Data.Fuse.Sql;
using System.Data.ModelDescription;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContract.Tests {

  [TestClass]
  public class SqlRepositoryTests {

    [TestMethod]
    public void TestMethod1() {
      // Arrange
      SchemaRoot schemaRoot = SchemaCache.GetSchemaRootForContext(typeof(DemoDbContext));
      var contextProvider = new ShortLivingDbConnectionInstanceProvider(
        () => new SqlConnection(@"Server=(localdb)\mssqllocaldb;Database=DemoDbContext")
      );
      var dataStore = new SqlDataStore(contextProvider, schemaRoot, (EntitySchema es) => {
        if (es.Name == "Employee") {
          return "Employees";
        }
        return es.NamePlural;
      });

      // Act
      Employee employee = new Employee() {
        FirstName = "John",
        LastName = "Doe",
        DateOfBirth = DateTime.Now.AddYears(-30)
      };
      employee = dataStore.AddOrUpdate<Employee, int>(employee);

      var result = dataStore.GetEntities<Employee, int>(new ExpressionTree());

      int countAfterFirstUpdate = result.Count();

      Employee employee2 = dataStore.AddOrUpdate<Employee, int>(employee);

      int countAfterSecondAddOrUpdate = result.Count();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(countAfterFirstUpdate, countAfterSecondAddOrUpdate);
      Assert.AreEqual(employee.Id, employee2.Id);
      Assert.IsTrue(result.Count() > 1);
      Assert.IsTrue(result.Any(e => e.FirstName == "John"));

      dataStore.TryDeleteEntities<Employee, int>(employee.Id);
    }

  }
}
