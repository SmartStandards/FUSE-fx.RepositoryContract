using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryContract.Tests;
using System;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.Fuse.Ef;
using System.Data.Fuse.Ef.InstanceManagement;
using System.Data.ModelDescription;
using System.Linq;

namespace RepositoryTests {

  public class TestDbContext : DbContext {
    public DbSet<LeafEntity1> LeafEntities1 { get; set; } = null!;
    public TestDbContext() : base() { }
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      optionsBuilder.UseInMemoryDatabase("TestDb");
    }
  }

  [TestClass]
  public class EfRepositoryTests : RepositoryTestsBase {

    protected override IRepository<LeafEntity1, int> CreateRepository() {
      var options = new DbContextOptionsBuilder<TestDbContext>()
                      .UseInMemoryDatabase(databaseName: "TestDb")
                      .Options;

      using var context = new TestDbContext(options);

      return new EfRepository<LeafEntity1, int>(new ShortLivingDbContextInstanceProvider<TestDbContext>());
    }

  }
}
