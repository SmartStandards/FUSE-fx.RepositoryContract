using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Convenience;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.Fuse.Ef;
using System.Data.Fuse.Ef.InstanceManagement;
using System.Data.Fuse.Sql;
using System.Data.Fuse.Sql.InstanceManagement;
using System.Data.ModelDescription;
using System.Data.SqlClient;
using System.Linq;

namespace RepositoryTests {

  [TestClass]
  public abstract class ModelVsEntityRepositoryTestsBase : RepositoryTestsBase {

    protected abstract IRepository<LeafEntity2, int> CreateLeafEntity2Repository();

    protected abstract IDataStore CreateEntityDatastore();

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
      return this.SeedLeafEntity1Repository(repo, 10);
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

    //protected IRepository<LeafEntity1, int> CreateSqlRepository() {
    //  SchemaRoot schemaRoot = ModelReader.GetSchema(new Type[] {
    //    typeof(RootEntity1),
    //    typeof(LeafEntity1),
    //    typeof(ChildEntity1)
    //  }, false);
    //  using (SqlConnection c = new SqlConnection(
    //      "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RepositoryContractTests;Integrated Security=True;"
    //  )) {
    //    SqlSchemaInstaller.EnsureSchemaIsInstalled(c, schemaRoot);
    //  }
    //  ;
    //  return new SqlRepository<LeafEntity1, int>(
    //    new ShortLivingDbConnectionInstanceProvider(
    //      () => new SqlConnection(
    //        "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RepositoryContractTests;Integrated Security=True;"
    //      )
    //    ),
    //    schemaRoot
    //  );
    //}

    //protected IRepository<LeafEntity2, int> CreateSqlRepository2() {
    //  SchemaRoot schemaRoot = ModelReader.GetSchema(new Type[] {
    //    typeof(RootEntity2),
    //    typeof(LeafEntity2),
    //    typeof(ChildEntity2)
    //  }, false);
    //  using (SqlConnection c = new SqlConnection(
    //      "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RepositoryContractTests;Integrated Security=True;"
    //  )) {
    //    SqlSchemaInstaller.EnsureSchemaIsInstalled(c, schemaRoot);
    //  }
    //  ;
    //  return new SqlRepository<LeafEntity2, int>(
    //    new ShortLivingDbConnectionInstanceProvider(
    //      () => new SqlConnection(
    //        "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RepositoryContractTests;Integrated Security=True;"
    //      )
    //    ),
    //    schemaRoot
    //  );
    //}   

    protected IDataStore CreateModelDatastore() {
      return new ModelVsEntityDataStore(CreateEntityDatastore(),
        new ModelVsEntityType[] {
          new ModelVsEntityType<LeafModel1, LeafEntity1, int>(),
          new ModelVsEntityType<RootModel1, RootEntity1, int>(),
          new ModelVsEntityType<ChildModel1, ChildEntity1, int>(),
          new ModelVsEntityType<LeafModel2, LeafEntity2, int>(),
          new ModelVsEntityType<RootModel2, RootEntity2, int>(),
          new ModelVsEntityType<ChildModel2, ChildEntity2, int>(),
          new ModelVsEntityType<LeafModelWithCompositeKey, LeafEntityWithCompositeKey, CompositeKey2<int, string>>(),
          new ModelVsEntityType<RootModelWithCompositeKey, RootEntityWithCompositeKey, CompositeKey2<int, string>>(),
          new ModelVsEntityType<ChildModelOfRootModelWithCompositeKey, ChildEntityOfRootEntityWithCompositeKey, int>(),
        }
      );
    }

    protected void SeedModelRepositories(
      IRepository<RootModel1, int> repository,
      IRepository<ChildModel1, int> childRepository,
      IRepository<LeafModelWithCompositeKey, CompositeKey2<int, string>> lmwckRepo,
      int numEntities,
      int highestKeyLeaf1
    ) {
      var keyToDeleteLeafWithCk = lmwckRepo.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { }
      ).Select(r => r.Key).ToArray();
      lmwckRepo.TryDeleteEntities(keyToDeleteLeafWithCk);
      lmwckRepo.AddOrUpdateEntity(new LeafModelWithCompositeKey() {
        Field1 = 1,
        Field2 = "A",
        LongValue = 100,
        StringValue = "Leaf With CK 1",
        DateValue = DateTime.Now,
        GuidValue = Guid.NewGuid(),
        BoolValue = true,
        FloatValue = 1.1f,
        DoubleValue = 2.2,
        DecimalValue = 3.3m
      });
      var keyToDelete = repository.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { }
      ).Select(r => r.Key).ToArray();
      repository.TryDeleteEntities(keyToDelete);
      for (int i = 1; i <= numEntities; i++) {
        var entity = new RootModel1() {
          Id = i,
          Name = "Entity " + i,
          Leaf1Id = highestKeyLeaf1 - (i - 1),
          OtherField1 = 1,
          OtherField2 = "A"
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
          Root1Id = i,
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
          Leaf2Id = highestLeaf2Key - (numEntities + i),
        };
        repository.AddOrUpdateEntity(entity);
      }
      int highestKey = repository.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { nameof(RootModel2.Id) }
      ).Max(r => r.Key);
      for (int i = highestKey - numEntities + 1; i <= highestKey; i++) {
        ChildModel2 child = new ChildModel2() {
          Id = i,
          Name = "Child " + i,
          Root2Id = i,
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
      var repository = this.CreateLeaf1EntityRepository();
      var repository2 = this.CreateLeafEntity2Repository();
      IRepository<RootModel1, int> rootModel1Repository = this.CreateModelDatastore().GetRepository<RootModel1, int>();
      IRepository<ChildModel1, int> childModel1Repository = this.CreateModelDatastore().GetRepository<ChildModel1, int>();
      IRepository<RootModel2, int> rootModel2Repository = this.CreateModelDatastore().GetRepository<RootModel2, int>();
      IRepository<ChildModel2, int> childModel2Repository = this.CreateModelDatastore().GetRepository<ChildModel2, int>();
      IRepository<LeafModelWithCompositeKey, CompositeKey2<int, string>> lmwckRepo = this.CreateModelDatastore().GetRepository<LeafModelWithCompositeKey, CompositeKey2<int, string>>();
      int highestKeyLeaf1 = this.SeedRepository1(repository, childModel1Repository, rootModel1Repository);
      this.SeedRepositoryAlt2(repository2, childModel2Repository, rootModel2Repository);

      this.SeedModelRepositories(rootModel1Repository, childModel1Repository, lmwckRepo, 1, highestKeyLeaf1);

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
      IRepository<RootModel1, int> rootModel1Repository = this.CreateModelDatastore().GetRepository<RootModel1, int>();
      IRepository<ChildModel1, int> childModel1Repository = this.CreateModelDatastore().GetRepository<ChildModel1, int>();
      IRepository<LeafModelWithCompositeKey, CompositeKey2<int, string>> lmwckRepo = this.CreateModelDatastore().GetRepository<LeafModelWithCompositeKey, CompositeKey2<int, string>>();

      var repository = this.CreateLeaf1EntityRepository();
      int highestKeyLeaf1 = this.SeedRepository1(repository, childModel1Repository, rootModel1Repository);

      this.SeedModelRepositories(rootModel1Repository, childModel1Repository, lmwckRepo, 1, highestKeyLeaf1);

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
      var repository = this.CreateLeaf1EntityRepository();
      var repository2 = this.CreateLeafEntity2Repository();
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
      Assert.IsTrue(allEntities[0].Leaf2.StringValue.StartsWith("Entity "), "Expected Name to match.");
    }

    [TestMethod]
    public void GetEntities_With1ToNDependentsAndNavsOnEntities_Works() {

      // Arrange
      var repository = this.CreateLeaf1EntityRepository();
      var repository2 = this.CreateLeafEntity2Repository();
      IRepository<RootModel2, int> rootModel2Repository = this.CreateModelDatastore().GetRepository<RootModel2, int>();
      IRepository<ChildModel2, int> childModel2Repository = this.CreateModelDatastore().GetRepository<ChildModel2, int>();
      IRepository<RootModel1, int> rootModel1Repository = this.CreateModelDatastore().GetRepository<RootModel1, int>();
      IRepository<ChildModel1, int> childModel1Repository = this.CreateModelDatastore().GetRepository<ChildModel1, int>();
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
      Assert.IsTrue(allEntities[0].Children[0].Name.StartsWith("Child "), "Expected Child Name to match.");
    }

    [TestMethod]
    public void GetEntities_WithCompositeKeyLookup_Works() {
      // Arrange
      IRepository<LeafModelWithCompositeKey, CompositeKey2<int, string>> lmwckRepo =
        this.CreateModelDatastore().GetRepository<LeafModelWithCompositeKey, CompositeKey2<int, string>>();
      var repository = this.CreateLeaf1EntityRepository();
      IRepository<ChildModel1, int> childModel1Repository = this.CreateModelDatastore().GetRepository<ChildModel1, int>();
      IRepository<RootModel1, int> rootModel1Repository = this.CreateModelDatastore().GetRepository<RootModel1, int>();
      int highestKeyLeaf1 = this.SeedRepository1(repository, childModel1Repository, rootModel1Repository);
      this.SeedModelRepositories(rootModel1Repository, childModel1Repository, lmwckRepo, 1, highestKeyLeaf1);
      // Act
      var allEntities = rootModel1Repository.GetEntities(
        ExpressionTree.Empty(), new string[] { }
      );
      // Assert
      Assert.AreEqual(1, allEntities.Length, "Expected 1 entities.");

      LeafModelWithCompositeKey? leafModelWithCompositeKey = allEntities[0].OtherLeaf;
      Assert.IsNotNull(leafModelWithCompositeKey, "Expected OtherLeaf to be populated.");
      Assert.AreEqual(1, leafModelWithCompositeKey.Field1, "Expected Field1 to match.");
    }

    [TestMethod]
    public void GetEntities_WithCompositeKeyPrincipal_Works() {
      CreateLeaf1EntityRepository();
      IRepository<RootModelWithCompositeKey, CompositeKey2<int, string>> rmwckRepo =
        this.CreateModelDatastore().GetRepository<RootModelWithCompositeKey, CompositeKey2<int, string>>();
      IRepository<ChildModelOfRootModelWithCompositeKey, int> childModelRepo = 
        this.CreateModelDatastore().GetRepository<ChildModelOfRootModelWithCompositeKey, int>();

      var keyToDeleteRoot = rmwckRepo.GetEntityRefs(ExpressionTree.Empty(), new string[] { })
        .Select(r => r.Key).ToArray();
      rmwckRepo.TryDeleteEntities(keyToDeleteRoot);
      rmwckRepo.AddOrUpdateEntity(new RootModelWithCompositeKey() {
        KeyField1 = 1,
        KeyField2 = "A",
        Name = "Root With CK 1",
      });

      var keyToDeleteChild = childModelRepo.GetEntityRefs(ExpressionTree.Empty(), new string[] { })
        .Select(r => r.Key).ToArray();
      childModelRepo.TryDeleteEntities(keyToDeleteChild);
      childModelRepo.AddOrUpdateEntity(new ChildModelOfRootModelWithCompositeKey() {
        Id = 1,
        Name = "Child of Root With CK 1",
        Root1KeyField1 = 1,
        Root1KeyField2 = "A"
      });

      // Act
      var allEntities = rmwckRepo.GetEntities(
        ExpressionTree.Empty(), new string[] { }
      );

      // Assert
      Assert.AreEqual(1, allEntities.Length, "Expected 1 entities.");
      Assert.IsNotNull(allEntities[0].Children, "Expected Children to be populated.");
      Assert.AreEqual(1, allEntities[0].Children.Count(), "Expected 1 child entity.");
      Assert.IsTrue(allEntities[0].Children[0].Name.StartsWith("Child "), "Expected Child Name to match.");

    }
  }
}
