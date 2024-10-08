using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryContract.Tests;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Fuse;
using System.Data.Fuse.FileSupport;
using System.Data.ModelDescription;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Fuse.SchemaResolving {

  [TestClass]
  public class SchemaResolvingTests {

    [TestMethod]
    public void TestEFCoreSchemaResolving() {

      IEntityResolver resolver = new AssemblySearchEntityResolver(
        typeof(PersonEntity).Assembly, typeof(PersonEntity).Namespace
      );

      Type[] entityTypes = resolver.GetWellknownTypes();
      Assert.AreEqual(17, entityTypes.Length);
   
    }

    [TestMethod]
    public void TestEF4SchemaResolving2() {
      IEntityResolver resolver = new DbContextDeclaratedEntityResolver(
        typeof(MyDbContext), false
      );

      Type[] entityTypes = resolver.GetWellknownTypes();
      Assert.AreEqual(2, entityTypes.Length);

    }

    [TestMethod]
    public void TestEF4SchemaResolving3() {
      IEntityResolver resolver = new DbContextRuntimeEntityResolver(
       () => new MyDbContext(), false
      );

      Type[] entityTypes = resolver.GetWellknownTypes();
      Assert.AreEqual(2, entityTypes.Length);

    }

  }

  public class MyDbContext : DbContext {

    public DbSet<Religion> Religions { get; set; }
    public DbSet<Address> Addresses { get; set; }

    public MyDbContext() {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      base.OnConfiguring(optionsBuilder);
      optionsBuilder.UseInMemoryDatabase(@"UnitTest");
    }

  }

}
