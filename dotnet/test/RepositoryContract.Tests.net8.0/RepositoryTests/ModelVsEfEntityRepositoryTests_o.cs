using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Convenience;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.Fuse.Ef;
using System.Data.Fuse.Ef.InstanceManagement;
using System.Linq;

namespace RepositoryTests {

  [TestClass]
  public class ModelVsEfEntityRepositoryTests_o : RepositoryTestsBase {

    protected override IRepository<LeafEntity1, int> CreateRepository() {
      var options = new DbContextOptionsBuilder<TestDbContext>()
                      .UseInMemoryDatabase(databaseName: "TestDb")
                      .Options;

      using var context = new TestDbContext(options);

      EfRepository<LeafEntity1, int> efRepo = new EfRepository<LeafEntity1, int>(
        new ShortLivingDbContextInstanceProvider<TestDbContext>()
      );

      return new ModelVsEntityRepository<LeafEntity1, LeafEntity1, int>(
        efRepo, new ModelVsEntityParams<LeafEntity1, LeafEntity1>()
      );
    }

    protected IRepository<LeafEntity2, int> CreateRepository2() {
      var options = new DbContextOptionsBuilder<TestDbContext>()
                      .UseInMemoryDatabase(databaseName: "TestDb")
                      .Options;

      using var context = new TestDbContext(options);

      EfRepository<LeafEntity2, int> efRepo = new EfRepository<LeafEntity2, int>(
        new ShortLivingDbContextInstanceProvider<TestDbContext>()
      );

      return new ModelVsEntityRepository<LeafEntity2, LeafEntity2, int>(
        efRepo, new ModelVsEntityParams<LeafEntity2, LeafEntity2>()
      );
    }

    protected IDataStore CreateEntityDatastore() {
      return new EfDataStore<TestDbContext>(new ShortLivingDbContextInstanceProvider<TestDbContext>());
    }

    protected IDataStore CreateModelDatastore() {
      return new ModelVsEntityDataStore(CreateEntityDatastore(),
        new ModelVsEntityType[] {
          new ModelVsEntityType<LeafModel1, LeafEntity1, int>(),
          new ModelVsEntityType<RootModel1, RootEntity1, int>(),
          new ModelVsEntityType<ChildModel1, ChildEntity1, int>(),
          new ModelVsEntityType<LeafModel2, LeafEntity2, int>(),
          new ModelVsEntityType<RootModel2, RootEntity2, int>(),
          new ModelVsEntityType<ChildModel2, ChildEntity2, int>()
        }
      );
    }

    private int SeedRepository1(
     IRepository<LeafEntity1, int> repo,
     IRepository<ChildModel1, int> childModel1Repo,
     IRepository<RootModel1, int> rootModel1Repo
   ) {
      var keyToDeletChild1 = childModel1Repo.GetEntityRefs(
       ExpressionTree.Empty(), new string[] { }
     ).Select(r => r.Key).ToArray();
      childModel1Repo.TryDeleteEntities(keyToDeletChild1);
      var keyToDeleteRoot1 = rootModel1Repo.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { }
      ).Select(r => r.Key).ToArray();
      rootModel1Repo.TryDeleteEntities(keyToDeleteRoot1);
      return this.SeedRepository(repo, 10);
    }

    private int SeedRepositoryAlt2(
      IRepository<LeafEntity2, int> repo,
      IRepository<ChildModel2, int> childModel2Repo,
      IRepository<RootModel2, int> rootModel2Repo
    ) {
      var keyToDeletChild2 = childModel2Repo.GetEntityRefs(
       ExpressionTree.Empty(), new string[] { }
     ).Select(r => r.Key).ToArray();
      childModel2Repo.TryDeleteEntities(keyToDeletChild2);
      var keyToDeleteRoot2 = rootModel2Repo.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { }
      ).Select(r => r.Key).ToArray();
      rootModel2Repo.TryDeleteEntities(keyToDeleteRoot2);
      return this.SeedRepository2(repo, 10);
    }

    protected void SeedModelRepositories(
      IRepository<RootModel1, int> repository,
      IRepository<ChildModel1, int> childRepository,
      int numEntities, int highestLeaf1Key
    ) {
      var keyToDelete = repository.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { }
      ).Select(r => r.Key).ToArray();
      repository.TryDeleteEntities(keyToDelete);
      for (int i = 1; i <= numEntities; i++) {
        var entity = new RootModel1() {
          Id = i,
          Name = "Entity " + i,
          Leaf1Id = highestLeaf1Key - numEntities + i,
        };
        repository.AddOrUpdateEntity(entity);
      }
      int highestKey = repository.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { nameof(RootModel1.Id) }
      ).Max(r => r.Key);
      for (int i = highestKey - numEntities + 1; i <= highestKey; i++) {
        ChildModel1 child = new ChildModel1() {
          Id = i,
          Name = "Child " + i,
          Root1Id =  i,
        };
        childRepository.AddOrUpdateEntity(child);
      }
    }

    protected void SeedModelRepositories2(
      IRepository<RootModel2, int> repository,
      IRepository<ChildModel2, int> childRepository,
      int numEntities, int highestLeaf2Key
    ) {
      var keyToDelete = repository.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { }
      ).Select(r => r.Key).ToArray();
      repository.TryDeleteEntities(keyToDelete);
      for (int i = 1; i <= numEntities; i++) {
        var entity = new RootModel2() {
          Id = i,
          Name = "Entity " + i,
          Leaf2Id = highestLeaf2Key - numEntities + i,
        };
        repository.AddOrUpdateEntity(entity);
      }
      int highestKey = repository.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { nameof(RootModel1.Id) }
      ).Max(r => r.Key);
      for (int i = 1; i <= numEntities; i++) {
        ChildModel2 child = new ChildModel2() {
          Id = i,
          Name = "Child " + i,
          Root2Id = highestKey - numEntities + i,
        };
        childRepository.AddOrUpdateEntity(child);
      }
    }

    protected int SeedRepository2(
      IRepository<LeafEntity2, int> repository, int numEntities
    ) {
      var keyToDelete = repository.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { }
      ).Select(r => r.Key).ToArray();
      repository.TryDeleteEntities(keyToDelete);
      for (int i = 1; i <= numEntities; i++) {
        var entity = new LeafEntity2() {
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
        repository.AddOrUpdateEntity(entity);
      }
      int highestKey = repository.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { nameof(LeafEntity2.Id) }
      ).Max(r => r.Key);
      return highestKey;
    }

    [TestMethod]
    public void GetEntities_WithLookup_Works() {

      // Arrange
      var repository = this.CreateRepository();
      var repository2 = this.CreateRepository2();
      int highestLeaf1Key = this.SeedRepository(repository, 10);
      this.SeedRepository2(repository2, 10);
      IRepository<RootModel1, int> rootModel1Repository = this.CreateModelDatastore().GetRepository<RootModel1, int>();
      IRepository<ChildModel1, int> childModel1Repository = this.CreateModelDatastore().GetRepository<ChildModel1, int>();

      this.SeedModelRepositories(rootModel1Repository, childModel1Repository, 1, highestLeaf1Key);

      // Act
      var allEntities = rootModel1Repository.GetEntities(
        ExpressionTree.Empty(), new string[] { }
      );

      // Assert
      Assert.AreEqual(1, allEntities.Length, "Expected 1 entities.");
      Assert.IsNotNull(allEntities[0].Leaf1, "Expected Leaf1 to be populated.");
      Assert.IsTrue(allEntities[0].Leaf1.StringValue.StartsWith("Entity "), "Expected Name to match.");
    }

    [TestMethod]
    public void GetEntities_With1ToNDependents_Works() {

      // Arrange
      var repository = this.CreateRepository();
      IRepository<RootModel1, int> rootModel1Repository = this.CreateModelDatastore().GetRepository<RootModel1, int>();
      IRepository<ChildModel1, int> childModel1Repository = this.CreateModelDatastore().GetRepository<ChildModel1, int>();
      int highestLeaf1Key = this.SeedRepository1(repository, childModel1Repository, rootModel1Repository);

      this.SeedModelRepositories(rootModel1Repository, childModel1Repository, 1, highestLeaf1Key);

      // Act
      var allEntities = rootModel1Repository.GetEntities(
        ExpressionTree.Empty(), new string[] { }
      );

      // Assert
      Assert.AreEqual(1, allEntities.Length, "Expected 1 entities.");
      Assert.IsNotNull(allEntities[0].Children, "Expected Leaf1 to be populated.");
      Assert.AreEqual(1, allEntities[0].Children.Count(), "Expected 1 child entity.");
      Assert.IsTrue(allEntities[0].Children[0].Name.StartsWith("Child "), "Expected Child Name to match.");
    }

    [TestMethod]
    public void GetEntities_WithLookupAndNavsOnEntities_Works() {

      // Arrange
      var repository = this.CreateRepository();
      var repository2 = this.CreateRepository2();
      IRepository<RootModel1, int> rootModel1Repository = this.CreateModelDatastore().GetRepository<RootModel1, int>();
      IRepository<ChildModel1, int> childModel1Repository = this.CreateModelDatastore().GetRepository<ChildModel1, int>();
      IRepository<RootModel2, int> rootModel2Repository = this.CreateModelDatastore().GetRepository<RootModel2, int>();
      IRepository<ChildModel2, int> childModel2Repository = this.CreateModelDatastore().GetRepository<ChildModel2, int>();
      this.SeedRepository1(repository, childModel1Repository, rootModel1Repository);
      int highestLeaf2Key = this.SeedRepositoryAlt2(repository2, childModel2Repository, rootModel2Repository);

      this.SeedModelRepositories2(rootModel2Repository, childModel2Repository, 1, highestLeaf2Key);

      // Act
      var allEntities = rootModel2Repository.GetEntities(
        ExpressionTree.Empty(), new string[] { }
      );

      // Assert
      Assert.AreEqual(1, allEntities.Length, "Expected 1 entities.");
      Assert.IsNotNull(allEntities[0].Leaf2, "Expected Leaf2 to be populated.");
      Assert.IsTrue(allEntities[0].Leaf2.StringValue.StartsWith("Entity 1"), "Expected Name to match.");
    }

    [TestMethod]
    public void GetEntities_With1ToNDependentsAndNavsOnEntities_Works() {

      // Arrange
      var repository = this.CreateRepository();
      var repository2 = this.CreateRepository2();
      IRepository<RootModel1, int> rootModel1Repository = this.CreateModelDatastore().GetRepository<RootModel1, int>();
      IRepository<ChildModel1, int> childModel1Repository = this.CreateModelDatastore().GetRepository<ChildModel1, int>();
      IRepository<RootModel2, int> rootModel2Repository = this.CreateModelDatastore().GetRepository<RootModel2, int>();
      IRepository<ChildModel2, int> childModel2Repository = this.CreateModelDatastore().GetRepository<ChildModel2, int>();
      this.SeedRepository1(repository, childModel1Repository, rootModel1Repository);
      int highestLeaf2Key = this.SeedRepositoryAlt2(repository2, childModel2Repository, rootModel2Repository);

      this.SeedModelRepositories2(rootModel2Repository, childModel2Repository, 1, highestLeaf2Key);

      // Act
      var allEntities = rootModel2Repository.GetEntities(
        ExpressionTree.Empty(), new string[] { }
      );

      // Assert
      Assert.AreEqual(1, allEntities.Length, "Expected 1 entities.");
      Assert.IsNotNull(allEntities[0].Children, "Expected Leaf1 to be populated.");
      Assert.AreEqual(1, allEntities[0].Children.Count(), "Expected 1 child entity.");
      Assert.AreEqual("Child 1", allEntities[0].Children[0].Name, "Expected Child Name to match.");
    }
  }
}
