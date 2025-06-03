using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.SqlServer;

namespace RepositoryContract.Tests.Net4 {

  public class MyDbContext : DbContext {
    public DbSet<Person> People { get; set; }
    public DbSet<Address> Addresses { get; set; }

    public MyDbContext()
      : base(SetInitializer()) {
      Database.SetInitializer<MyDbContext>(null);
    }

    private static string SetInitializer() {
      string connectionString = "Data Source=YOUR_SERVER_NAME;Initial Catalog=RepoContractTestNet4;Integrated Security=True;MultipleActiveResultSets=True";

      Database.SetInitializer(new CreateDatabaseIfNotExists<MyDbContext>());

      var builder = new DbConnectionStringBuilder {
        ConnectionString = connectionString
        
      };

      //DbConfiguration.Loaded += (_, a) =>
      //{
      //  //a.AddDependencyResolver(new SingletonDependencyResolver());
      //  a.ReplaceService<DbProviderServices>((s, x) => SqlProviderServices.Instance);
      //};

      return builder.ConnectionString;
    }
  }

  public class Person {
    public int Id { get; set; }
    public string Name { get; set; }
    public int AddressId { get; set; }
    public virtual Address Address { get; set; }
  }

  public class Address {
    public int Id { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
  }

  [TestClass]
  public class UnitTest1 {
    [TestMethod]
    public void TestMethod1() {
      MyDbContext context = new MyDbContext();
      string[] types = System.Data.Fuse.Ef.DbContextExtensions.GetManagedTypeNames(context);
      Assert.IsTrue(types.Length == 2);
      Assert.IsTrue(types[0] == "Person" || types[1] == "Person");
      Assert.IsTrue(types[0] == "Address" || types[1] == "Address");
    }
  }
}
