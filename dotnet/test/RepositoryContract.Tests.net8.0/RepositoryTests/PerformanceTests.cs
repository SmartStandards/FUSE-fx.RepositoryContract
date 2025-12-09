using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryTests;
using System;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.Fuse.Ef;
using System.Data.Fuse.Ef.InstanceManagement;
using System.Data.Fuse.Sql;
using System.Data.Fuse.Sql.InstanceManagement;
using System.Data.ModelDescription;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace RepositoryContract.Tests {
  [TestClass, Ignore]
  public class PerformanceTests {

    protected IRepository<LeafEntity1, int> CreateInMemoryRepository() {
      SchemaRoot schemaRoot = ModelReader.GetSchema(
          new Type[] { typeof(LeafEntity1) }, false
        );
      return new InMemoryRepository<LeafEntity1, int>(schemaRoot);
    }

    protected IRepository<LeafEntity1, int> CreateEfInMemoryRepository() {
      return new EfRepository<LeafEntity1, int>(
        new ShortLivingDbContextInstanceProvider<TestDbContext>()
      );
    }

    protected IRepository<LeafEntity1, int> CreateSqlRepository() {
      SchemaRoot schemaRoot = ModelReader.GetSchema(new Type[] {
        typeof(RootEntity1),
        typeof(LeafEntity1),
        typeof(ChildEntity1),
        typeof(LeafEntityWithCompositeKey)
      }, false);
      using (SqlConnection c = new SqlConnection(
          "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SqlRepositoryPerformanceTests;Integrated Security=True;"
      )) {
        SqlSchemaInstaller.EnsureSchemaIsInstalled(c, schemaRoot);
      }
      ;
      IRepository<LeafEntity1, int> result = new SqlRepository<LeafEntity1, int>(
        new ShortLivingDbConnectionInstanceProvider(
          () => new SqlConnection(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SqlRepositoryPerformanceTests;Integrated Security=True;"
          )
        ),
        schemaRoot
      );
      //Delete all entities
      var allEntities = result.GetEntities(ExpressionTree.Empty(), new string[] { }, 0, 0);
      result.TryDeleteEntities(allEntities.Select(e => e.Id).ToArray());
      return result;
    }

    protected IRepository<LeafEntity1, int> CreateEfLocalDbRepository() {
      IRepository<LeafEntity1, int> result = new EfRepository<LeafEntity1, int>(
        new ShortLivingDbContextInstanceProvider<TestDbContext2>()
      );
      //Delete all entities
      var allEntities = result.GetEntities(ExpressionTree.Empty(), new string[] { }, 0, 0);
      result.TryDeleteEntities(allEntities.Select(e => e.Id).ToArray());
      return result;
    }

    protected IRepository<LeafEntity1, int> CreateModelVsEntityRepository(IRepository<LeafEntity1, int> repo) {
      return new ModelVsEntityRepository<LeafEntity1, LeafEntity1, int>(
        repo,
        new ModelVsEntityParams<LeafEntity1, LeafEntity1>()
      );
    }

    [TestMethod]
    public void TestPerformance() {
      IRepository<LeafEntity1, int> inMemoryRepo = CreateInMemoryRepository();
      IRepository<LeafEntity1, int> efInMemoryRepo = CreateEfInMemoryRepository();
      IRepository<LeafEntity1, int> sqlRepo = CreateSqlRepository();
      IRepository<LeafEntity1, int> efLocalDbRepo = CreateEfLocalDbRepository();

      IRepository<LeafEntity1, int> modelVsInMemoryRepo = CreateModelVsEntityRepository(inMemoryRepo);

      StringBuilder logBuilder = new StringBuilder();
      Action<string> logAction = (msg) => {
        logBuilder.AppendLine(msg);
      };
      TestPerformance(inMemoryRepo, null);
      TestPerformance(inMemoryRepo, (msg) => logBuilder.AppendLine($"InMemory: {msg}"));

      TestPerformance(efInMemoryRepo, null);
      TestPerformance(efInMemoryRepo, (msg) => logBuilder.AppendLine($"EfInMemory: {msg}"));

      TestPerformance(sqlRepo, null);
      TestPerformance(sqlRepo, (msg) => logBuilder.AppendLine($"Sql: {msg}"));

      TestPerformance(efLocalDbRepo, null);
      TestPerformance(efLocalDbRepo, (msg) => logBuilder.AppendLine($"EfLocalDb: {msg}"));

      TestPerformance(modelVsInMemoryRepo, null);
      TestPerformance(modelVsInMemoryRepo, (msg) => logBuilder.AppendLine($"MvE|EfLocalDb: {msg}"));

      string log = logBuilder.ToString();
    }

    public void TestPerformance(IRepository<LeafEntity1, int> repo, Action<string>? logAction) {

      // Delete all existing entities
      var existingEntities = repo.GetEntities(ExpressionTree.Empty(), new string[] { }, 0, 0);
      repo.TryDeleteEntities(existingEntities.Select(e => e.Id).ToArray());

      // Insert 1000 entities
      DateTime now = DateTime.Now;
      for (int i = 0; i < 1000; i++) {
        var entity = new LeafEntity1() {
          Id = i,
          LongValue = i * 10,
          StringValue = "Entity " + i,
          DateValue = DateTime.Now.AddDays(-i),
          GuidValue = Guid.NewGuid(),
          BoolValue = (i % 2 == 0),
          FloatValue = i * 1.1f,
          DoubleValue = i * 2.2,
          DecimalValue = i * 3.3m
        };
        repo.AddOrUpdateEntity(entity);
      }
      DateTime afterInsert = DateTime.Now;
      logAction?.Invoke($"Inserted 1000 entities in {(afterInsert - now).TotalMilliseconds} ms");

      // Query entities
      DateTime beforeQuery = DateTime.Now;
      var allEntities = repo.GetEntities(ExpressionTree.Empty(), new string[] { }, 0, 0);
      DateTime afterQuery = DateTime.Now;
      logAction?.Invoke($"Queried {allEntities.Length} entities in {(afterQuery - beforeQuery).TotalMilliseconds} ms");
    }

  }
}
