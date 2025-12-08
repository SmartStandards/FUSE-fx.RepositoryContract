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
    public DbSet<RootEntity1> RootEntities1 { get; set; } = null!;
    public DbSet<ChildEntity1> ChildEntities1 { get; set; } = null!;
    public DbSet<LeafEntity2> LeafEntities2 { get; set; } = null!;
    public DbSet<RootEntity2> RootEntities2 { get; set; } = null!;
    public DbSet<ChildEntity2> ChildEntities2 { get; set; } = null!;
    public DbSet<LeafEntityWithCompositeKey> LeafEntityWithCompositeKeys { get; set; } = null!;
    public TestDbContext() : base() { }
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      optionsBuilder.UseInMemoryDatabase("TestDb");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);
      modelBuilder.Entity<LeafEntityWithCompositeKey>().HasKey(e => new { e.Field1, e.Field2 });
    }
  }

  [TestClass]
  public class EfRepositoryTests : RepositoryTestsBase {

    protected override IRepository<LeafEntity1, int> CreateLeaf1EntityRepository() {
      return new EfRepository<LeafEntity1, int>(
        new ShortLivingDbContextInstanceProvider<TestDbContext>()
      );
    }

  }
}
