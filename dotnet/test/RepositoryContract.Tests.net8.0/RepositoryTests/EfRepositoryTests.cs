using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryContract.Tests;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data.Fuse;
using System.Data.Fuse.AutoValueSupport;
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
    public DbSet<RootEntityWithCompositeKey> RootEntityWithCompositeKeys { get; set; } = null!;
    public DbSet<ChildEntityOfRootEntityWithCompositeKey> ChildEntityOfRootEntityWithCompositeKeys { get; set; } = null!;
    public TestDbContext() : base() { }
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      optionsBuilder.UseInMemoryDatabase("TestDb");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);
      modelBuilder.Entity<LeafEntityWithCompositeKey>().HasKey(e => new { e.Field1, e.Field2 });
      modelBuilder.Entity<RootEntityWithCompositeKey>().HasKey(e => new { e.KeyField1, e.KeyField2 });
    }
  }

  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  public class AutoValueLeafEntity {
    public int Id { get; set; }

    [IncrementAutoValue]
    public int SequenceNumber { get; set; }

    public string Name { get; set; } = string.Empty;
  }

  public class AutoValueEfTestDbContext : DbContext {
    private readonly DbConnection _connection;
    private readonly DbCommandInterceptor _commandInterceptor;

    public AutoValueEfTestDbContext(DbConnection connection, DbCommandInterceptor commandInterceptor) {
      _connection = connection;
      _commandInterceptor = commandInterceptor;
    }

    public DbSet<AutoValueLeafEntity> AutoValueLeafEntities { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      optionsBuilder
        .UseSqlite(_connection)
        .AddInterceptors(_commandInterceptor);
    }
  }

  internal sealed class MaxQueryCommandInterceptor : DbCommandInterceptor {
    public List<string> CommandTexts { get; } = new List<string>();

    public override InterceptionResult<DbDataReader> ReaderExecuting(
      DbCommand command,
      CommandEventData eventData,
      InterceptionResult<DbDataReader> result
    ) {
      CommandTexts.Add(command.CommandText);
      return base.ReaderExecuting(command, eventData, result);
    }

    public override InterceptionResult<object> ScalarExecuting(
      DbCommand command,
      CommandEventData eventData,
      InterceptionResult<object> result
    ) {
      CommandTexts.Add(command.CommandText);
      return base.ScalarExecuting(command, eventData, result);
    }

    public int CountCommandsContaining(string snippet) {
      return CommandTexts.Count(commandText => commandText.Contains(snippet, StringComparison.OrdinalIgnoreCase));
    }
  }

  [TestClass]
  public class EfRepositoryTests : RepositoryTestsBase {

    protected override IRepository<LeafEntity1, int> CreateLeaf1EntityRepository() {
      return new EfRepository<LeafEntity1, int>(
        new ShortLivingDbContextInstanceProvider<TestDbContext>()
      );
    }

    [TestMethod]
    public void ApplyValuesOnAdd_UsesSingleMaxQueryAndCachesHighestValuePerScope() {
      SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
      connection.Open();

      try {
        MaxQueryCommandInterceptor interceptor = new MaxQueryCommandInterceptor();

        using (AutoValueEfTestDbContext setupContext = new AutoValueEfTestDbContext(connection, interceptor)) {
          setupContext.Database.EnsureCreated();
          setupContext.AutoValueLeafEntities.AddRange(
            new AutoValueLeafEntity { Id = 1, SequenceNumber = 3, Name = "existing-1" },
            new AutoValueLeafEntity { Id = 2, SequenceNumber = 7, Name = "existing-2" }
          );
          setupContext.SaveChanges();
        }

        AutoValueManager.RegisterAlgorithm(
          "TestEfMaxQueryCaching_" + Guid.NewGuid().ToString("N"),
          context => {
            decimal nextValue = context.HighestValue.HasValue
              ? context.HighestValue.Value + context.Increment
              : context.Seed;
            return Convert.ChangeType(nextValue, context.PropertyInfo.PropertyType);
          }
        );

        string scopeKey = Guid.NewGuid().ToString();
        EfRepository<AutoValueLeafEntity, int> repository = new EfRepository<AutoValueLeafEntity, int>(
          new ShortLivingDbContextInstanceProvider<AutoValueEfTestDbContext>(
            () => new AutoValueEfTestDbContext(connection, interceptor)
          )
        );

        AutoValueLeafEntity firstEntity = repository.AddOrUpdateEntity(
          new AutoValueLeafEntity { Id = 10, Name = "first" }
        );
        AutoValueLeafEntity secondEntity = repository.AddOrUpdateEntity(
          new AutoValueLeafEntity { Id = 11, Name = "second" }
        );

        Assert.AreEqual(8, firstEntity.SequenceNumber);
        Assert.AreEqual(9, secondEntity.SequenceNumber);
        Assert.AreEqual(1, interceptor.CountCommandsContaining("MAX"));
      } finally {
        connection.Dispose();
      }
    }

  }
}
