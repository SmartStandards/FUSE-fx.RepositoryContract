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
  public class ModelVsEfEntityRepositoryTests : RepositoryTestsBase {

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

    protected void SeedModelRepositories(
      IRepository<RootModel1, int> repository,
      IRepository<ChildModel1, int> childRepository,
      int numEntities
    ) {
      var keyToDelete = repository.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { }
      ).Select(r => r.Key).ToArray();
      repository.TryDeleteEntities(keyToDelete);
      for (int i = 1; i <= numEntities; i++) {
        var entity = new RootModel1() {
          Id = i,
          Name = "Entity " + i,
          Leaf1Id = i,
        };
        repository.AddOrUpdateEntity(entity);
      }
      for (int i = 1; i <= numEntities; i++) {
        ChildModel1 child = new ChildModel1() {
          Id = i,
          Name = "Child " + i,
          Root1Id = i,
        };
        childRepository.AddOrUpdateEntity(child);
      }
    }

    protected void SeedModelRepositories2(
      IRepository<RootModel2, int> repository,
      IRepository<ChildModel2, int> childRepository,
      int numEntities
    ) {
      var keyToDelete = repository.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { }
      ).Select(r => r.Key).ToArray();
      repository.TryDeleteEntities(keyToDelete);
      for (int i = 1; i <= numEntities; i++) {
        var entity = new RootModel2() {
          Id = i,
          Name = "Entity " + i,
          Leaf2Id = i,
        };
        repository.AddOrUpdateEntity(entity);
      }
      for (int i = 1; i <= numEntities; i++) {
        ChildModel2 child = new ChildModel2() {
          Id = i,
          Name = "Child " + i,
          Root2Id = i,
        };
        childRepository.AddOrUpdateEntity(child);
      }
    }

    protected void SeedRepository2(
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
    }

    [TestMethod]
    public void GetEntities_WithLookup_Works() {

      // Arrange
      var repository = this.CreateRepository();
      var repository2 = this.CreateRepository2();
      this.SeedRepository(repository, 10);
      this.SeedRepository2(repository2, 10);
      IRepository<RootModel1, int> rootModel1Repository = this.CreateModelDatastore().GetRepository<RootModel1, int>();
      IRepository<ChildModel1, int> childModel1Repository = this.CreateModelDatastore().GetRepository<ChildModel1, int>();

      this.SeedModelRepositories(rootModel1Repository, childModel1Repository, 1);

      // Act
      var allEntities = rootModel1Repository.GetEntities(
        ExpressionTree.Empty(), new string[] { }
      );

      // Assert
      Assert.AreEqual(1, allEntities.Length, "Expected 1 entities.");
      Assert.IsNotNull(allEntities[0].Leaf1, "Expected Leaf1 to be populated.");
      Assert.AreEqual("Entity 1", allEntities[0].Leaf1.StringValue, "Expected Name to match.");
    }

    [TestMethod]
    public void GetEntities_With1ToNDependents_Works() {

      // Arrange
      var repository = this.CreateRepository();
      this.SeedRepository(repository, 10);
      IRepository<RootModel1, int> rootModel1Repository = this.CreateModelDatastore().GetRepository<RootModel1, int>();
      IRepository<ChildModel1, int> childModel1Repository = this.CreateModelDatastore().GetRepository<ChildModel1, int>();

      this.SeedModelRepositories(rootModel1Repository, childModel1Repository, 1);

      // Act
      var allEntities = rootModel1Repository.GetEntities(
        ExpressionTree.Empty(), new string[] { }
      );

      // Assert
      Assert.AreEqual(1, allEntities.Length, "Expected 1 entities.");
      Assert.IsNotNull(allEntities[0].Children, "Expected Leaf1 to be populated.");
      Assert.AreEqual(1, allEntities[0].Children.Count(), "Expected 1 child entity.");
      Assert.AreEqual("Child 1", allEntities[0].Children[0].Name, "Expected Child Name to match.");
    }

    [TestMethod]
    public void GetEntities_WithLookupAndNavsOnEntities_Works() {

      // Arrange
      var repository = this.CreateRepository();
      var repository2 = this.CreateRepository2();
      this.SeedRepository(repository, 10);
      this.SeedRepository2(repository2, 10);
      IRepository<RootModel2, int> rootModel2Repository = this.CreateModelDatastore().GetRepository<RootModel2, int>();
      IRepository<ChildModel2, int> childModel2Repository = this.CreateModelDatastore().GetRepository<ChildModel2, int>();

      this.SeedModelRepositories2(rootModel2Repository, childModel2Repository, 1);

      // Act
      var allEntities = rootModel2Repository.GetEntities(
        ExpressionTree.Empty(), new string[] { }
      );

      // Assert
      Assert.AreEqual(1, allEntities.Length, "Expected 1 entities.");
      Assert.IsNotNull(allEntities[0].Leaf2, "Expected Leaf2 to be populated.");
      Assert.AreEqual("Entity 1", allEntities[0].Leaf2.StringValue, "Expected Name to match.");
    }

    [TestMethod]
    public void GetEntities_With1ToNDependentsAndNavsOnEntities_Works() {

      // Arrange
      var repository = this.CreateRepository();
      this.SeedRepository(repository, 10);
      IRepository<RootModel2, int> rootModel2Repository = this.CreateModelDatastore().GetRepository<RootModel2, int>();
      IRepository<ChildModel2, int> childModel2Repository = this.CreateModelDatastore().GetRepository<ChildModel2, int>();

      this.SeedModelRepositories2(rootModel2Repository, childModel2Repository, 1);

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
