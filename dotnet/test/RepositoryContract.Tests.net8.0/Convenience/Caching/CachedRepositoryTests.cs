using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Data.ModelDescription.Convenience;
using System.Threading;

namespace System.Data.Fuse.Convenience.Caching {

  [TestClass]
  public class CachedRepositoryTests {

    private SchemaRoot CreateSchema() {
      ModelReader reader = new ModelReader();
      
      SchemaRoot schemaRoot = ModelReader.GetSchema(
        new[] { typeof(TestEntity) }, true
      );

      return schemaRoot;
    }

    private CachedRepository<TestEntity, int> CreateRepository(
      out InMemoryRepository<TestEntity, int> innerRepo
    ) {
      SchemaRoot schema = this.CreateSchema();

      innerRepo = new InMemoryRepository<TestEntity, int>(schema);

      CachedRepositoryOptions<TestEntity, int> options =
        new CachedRepositoryOptions<TestEntity, int>();

      options.KeySelector = (e) => e.Id;
      options.KeyFieldNames = new[] { "Id" };
      options.EnableQueryCacheOptIn = true;
      options.ChangeProcessing = ChangeProcessing.Patch;
      options.ReadMode = CacheReadMode.AllowStaleWhileRefresh;
      options.AccessExtensionSeconds = new[] { 0 };

      return new CachedRepository<TestEntity, int>(innerRepo, options);
    }

    [TestMethod]
    public void QueryCache_Should_Return_Same_Instance_On_Repeated_Call() {
      CachedRepository<TestEntity, int> repo =
        this.CreateRepository(out InMemoryRepository<TestEntity, int> inner);

      inner.TryAddEntity(new TestEntity { Id = 1, Name = "A" });

      TestEntity[] first = repo.GetEntities(null, null);
      TestEntity[] second = repo.GetEntities(null, null);

      Assert.AreSame(first, second, "Query cache did not reuse cached array instance.");
    }

    [TestMethod]
    public void Cache_Should_Patch_Query_On_Entity_Update() {
      CachedRepository<TestEntity, int> repo =
        this.CreateRepository(out InMemoryRepository<TestEntity, int> inner);

      inner.TryAddEntity(new TestEntity { Id = 1, Name = "Old" });

      TestEntity[] before = repo.GetEntities(null, null);
      Assert.AreEqual("Old", before[0].Name);

      repo.AddOrUpdateEntity(new TestEntity { Id = 1, Name = "New" });

      TestEntity[] after = repo.GetEntities(null, null);

      Assert.AreEqual("New", after[0].Name, "Cached entity was not patched.");
    }

    [TestMethod]
    public void Cache_Should_Remove_Entity_From_Query_On_Delete() {
      CachedRepository<TestEntity, int> repo =
        this.CreateRepository(out InMemoryRepository<TestEntity, int> inner);

      inner.TryAddEntity(new TestEntity { Id = 1, Name = "X" });
      inner.TryAddEntity(new TestEntity { Id = 2, Name = "Y" });

      TestEntity[] before = repo.GetEntities(null, null);
      Assert.AreEqual(2, before.Length);

      repo.TryDeleteEntities(new[] { 1 });

      TestEntity[] after = repo.GetEntities(null, null);
      Assert.AreEqual(1, after.Length);
      Assert.AreEqual(2, after[0].Id);
    }

    [TestMethod]
    public void StaleWhileRevalidate_Should_Return_Stale_Value() {
      CachedRepository<TestEntity, int> repo =
        this.CreateRepository(out InMemoryRepository<TestEntity, int> inner);

      inner.TryAddEntity(new TestEntity { Id = 1, Name = "Initial" });

      TestEntity[] first = repo.GetEntities(null, null);

      // force expiration
      Thread.Sleep(5);

      TestEntity[] second = repo.GetEntities(null, null);

      Assert.AreSame(
        first,
        second,
        "Stale value was not served while refresh was running."
      );
    }

    private sealed class TestEntity {
      public int Id { get; set; }
      public string Name { get; set; }

      public override string ToString() {
        return this.Id + ":" + this.Name;
      }
    }

  }

}
