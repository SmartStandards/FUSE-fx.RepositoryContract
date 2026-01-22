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

  public class TestDbContext2 : DbContext {

    private static bool _IsInitialized = false;

    public DbSet<LeafEntity1> LeafEntities1 { get; set; } = null!;
    public DbSet<RootEntity1> RootEntities1 { get; set; } = null!;
    public DbSet<ChildEntity1> ChildEntities1 { get; set; } = null!;
    public DbSet<LeafEntity2> LeafEntities2 { get; set; } = null!;
    public DbSet<RootEntity2> RootEntities2 { get; set; } = null!;
    public DbSet<ChildEntity2> ChildEntities2 { get; set; } = null!;
    public DbSet<LeafEntityWithCompositeKey> LeafEntityWithCompositeKeys { get; set; } = null!;
    public DbSet<RootEntityWithCompositeKey> RootEntityWithCompositeKeys { get; set; } = null!;
    public DbSet<ChildEntityOfRootEntityWithCompositeKey> ChildEntityOfRootEntityWithCompositeKeys { get; set; } = null!;

    public TestDbContext2() : base() {
      TryEnsureDatabase();
    }

    public TestDbContext2(DbContextOptions<TestDbContext2> options) : base(options) {
      TryEnsureDatabase();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      // Use localdb for testing      
      optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EfRepositoryTestsDb2;Trusted_Connection=True;MultipleActiveResultSets=true");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);
      modelBuilder.Entity<LeafEntityWithCompositeKey>().HasKey(e => new { e.Field1, e.Field2 });
      modelBuilder.Entity<RootEntityWithCompositeKey>().HasKey(e => new { e.KeyField1, e.KeyField2 });
    }

    private void TryEnsureDatabase() {
      if (_IsInitialized) {
        return;
      }
      /*
       * Behavior:
       * - If explicit migrations exist in the project (Database.GetMigrations() returns any),
       *   apply them via Database.Migrate().
       * - Otherwise (no explicit migrations) recreate the database from the current EF model
       *   by dropping and re-creating it (EnsureDeleted + EnsureCreated). This gives a clean,
       *   deterministic schema for tests without requiring generated migration files.
       *
       * Errors are allowed to surface so failing to prepare the test DB will fail the tests.
       */
      try {
        var migrations = this.Database.GetMigrations();
        if (migrations != null && migrations.Any()) {
          // There are explicit migrations available — apply them.
          this.Database.Migrate();
          _IsInitialized = true;
        } else {
          // No explicit migrations — recreate DB to match the current model.
          this.Database.EnsureDeleted();
          this.Database.EnsureCreated();
          _IsInitialized = true;
        }
      } catch (Exception) {
        // Try a fallback recreate if migrate failed, but surface exceptions if recreate fails too.
        try {
          this.Database.EnsureDeleted();
          this.Database.EnsureCreated();
        } catch {
          throw;
        }
      }
    }
  }

  //TODO_RWE: Remove Ignore attribute when tests are ready to run
  [TestClass, Ignore]
  public class EfRepository2Tests : RepositoryTestsBase {

    protected override IRepository<LeafEntity1, int> CreateLeaf1EntityRepository() {
      return new EfRepository<LeafEntity1, int>(
        new ShortLivingDbContextInstanceProvider<TestDbContext2>()
      );
    }

  }
}
