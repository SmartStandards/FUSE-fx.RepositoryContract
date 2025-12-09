using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryTests;
using System;
using System.Collections.Generic;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.Fuse.Ef;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContract.Tests {
  public abstract class DatastoreTestsBase {

    protected abstract IDataStore CreateDataStore();

    protected void SeedDataStore(IDataStore store) {

      var keyToDelete = store.GetEntityRefs<LeafEntity1, int>(
        ExpressionTree.Empty()
      ).Select(r => r.Key).ToArray();

      store.TryDeleteEntities<LeafEntity1, int>(keyToDelete);
      for (int i = 1; i <= 10; i++) {
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
        store.AddOrUpdate<LeafEntity1, int>(entity);
      }
      int highestLeaf1Key = store.GetEntityRefs<LeafEntity1, int>(
        ExpressionTree.Empty(), new string[] { nameof(LeafEntity1.Id) }
      ).Max(r => r.Key);

      store.AddOrUpdate<LeafEntityWithCompositeKey, CompositeKey2<int, string>>(
        new LeafEntityWithCompositeKey() {
          Field1 = 1,
          Field2 = "A",
          LongValue = 111,
          StringValue = "Composite Entity A",
          DateValue = DateTime.Now,
          GuidValue = Guid.NewGuid(),
          BoolValue = true,
          FloatValue = 1.23f,
          DoubleValue = 4.56,
          DecimalValue = 7.89m
        }
      );

      store.AddOrUpdate<RootEntity1, int>(
        new RootEntity1() {
          Id = 1,
          Name = "Root Entity 1",
          Leaf1Id = highestLeaf1Key,
          OtherField1 = 1,
          OtherField2 = "A"
        }
      );
      int highestRootEntity1Key = store.GetEntityRefs<RootEntity1, int>(
        ExpressionTree.Empty(), new string[] { $"^{nameof(RootEntity1.Id)}" }
      ).Max(r => r.Key);

      for (int i = 0; i < 10; i++) {
        store.AddOrUpdate<ChildEntity1, int>(
          new ChildEntity1() {
            Id = i,
            Name = "Child Entity " + i,
            Root1Id = highestRootEntity1Key
          }
        );
      }

      store.AddOrUpdate<RootEntityWithCompositeKey, int>(
        new RootEntityWithCompositeKey() {
          KeyField1 = 1,
          KeyField2 = "A",
          Name = "Root Entity 1"
        }
      );     

      for (int i = 0; i < 10; i++) {
        store.AddOrUpdate<ChildEntityOfRootEntityWithCompositeKey, int>(
          new ChildEntityOfRootEntityWithCompositeKey() {
            Id = i,
            Name = "Child Entity " + i,
            Root1KeyField1 = 1,
            Root1KeyField2 = "A"
          }
        );
      }
    }

    [TestMethod]
    public void DataStore_Creation_Works() {
      IDataStore store = CreateDataStore();
      Assert.IsNotNull(store);
    }

    [TestMethod]
    public void DataStore_GetEntities_Works() {
      IDataStore store = CreateDataStore();
      this.SeedDataStore(store);

      var leafEntity1s = store.GetEntities<LeafEntity1, int>(ExpressionTree.Empty(), new string[] { }, 0, 0);
      Assert.IsNotNull(leafEntity1s);
      Assert.AreEqual(10, leafEntity1s.Length);
    }

    [TestMethod]
    public void DataStore_GetDependents_Works() {
      IDataStore store = CreateDataStore();
      this.SeedDataStore(store);

      var rootEntity1s = store.GetEntities<RootEntity1, int>(ExpressionTree.Empty(), new string[] { }, 0, 0);
      Assert.IsNotNull(rootEntity1s);
      Assert.AreEqual(1, rootEntity1s.Length);

      RootEntity1 rootEntity1 = rootEntity1s[0];
      Assert.AreEqual("Root Entity 1", rootEntity1.Name);

      ChildEntity1[] children = rootEntity1.GetForeign1<RootEntity1, ChildEntity1>(store);
      Assert.IsNotNull(children);
      Assert.AreEqual(10, children.Length);
    }

    [TestMethod]
    public void DataStore_GetLookUp_Works() {
      IDataStore store = CreateDataStore();
      this.SeedDataStore(store);

      var rootEntity1s = store.GetEntities<RootEntity1, int>(ExpressionTree.Empty(), new string[] { }, 0, 0);
      Assert.IsNotNull(rootEntity1s);
      Assert.AreEqual(1, rootEntity1s.Length);

      RootEntity1 rootEntity1 = rootEntity1s[0];
      Assert.AreEqual("Root Entity 1", rootEntity1.Name);

      LeafEntity1 leafEntity1 = rootEntity1.GetPrimary1<LeafEntity1, RootEntity1>(store);
      Assert.IsNotNull(leafEntity1);
      Assert.IsTrue(leafEntity1.StringValue.StartsWith("Entity "));

    }

    [TestMethod]
    public void DataStore_GetLookUpWithCompositeKey_Works() {
      IDataStore store = CreateDataStore();
      this.SeedDataStore(store);

      var rootEntity1s = store.GetEntities<RootEntity1, int>(ExpressionTree.Empty(), new string[] { }, 0, 0);
      Assert.IsNotNull(rootEntity1s);
      Assert.AreEqual(1, rootEntity1s.Length);

      RootEntity1 rootEntity1 = rootEntity1s[0];
      Assert.AreEqual("Root Entity 1", rootEntity1.Name);

      LeafEntityWithCompositeKey[] leafEntities = store.GetEntities<LeafEntityWithCompositeKey, CompositeKey2<int, string>>(
        ExpressionTree.Empty()
      );

      LeafEntityWithCompositeKey leafEntity1 = rootEntity1.GetPrimary<
        LeafEntityWithCompositeKey, CompositeKey2<int, string>, RootEntity1
      >(store);
      Assert.IsNotNull(leafEntity1);
      Assert.IsTrue(leafEntity1.StringValue.StartsWith("Composite Entity A"));

    }

    [TestMethod]
    public void DataStore_GetDependentsWithCompositeKey_Works() {
      IDataStore store = CreateDataStore();
      this.SeedDataStore(store);

      var rootEntity1s = store.GetEntities<RootEntityWithCompositeKey, int>(ExpressionTree.Empty(), new string[] { }, 0, 0);
      Assert.IsNotNull(rootEntity1s);
      Assert.AreEqual(1, rootEntity1s.Length);

      RootEntityWithCompositeKey rootEntity1 = rootEntity1s[0];
      Assert.AreEqual("Root Entity 1", rootEntity1.Name);

      ChildEntityOfRootEntityWithCompositeKey[] children = rootEntity1.GetForeign<
        RootEntityWithCompositeKey, ChildEntityOfRootEntityWithCompositeKey, int
      >(store);
      Assert.IsNotNull(children);
      Assert.AreEqual(10, children.Length);

    }

  }
}
