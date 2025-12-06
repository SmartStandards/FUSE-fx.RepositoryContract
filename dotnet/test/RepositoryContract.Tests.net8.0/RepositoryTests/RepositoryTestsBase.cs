using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryContract.Tests;
using System;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Linq;

namespace RepositoryTests {

  public abstract class RepositoryTestsBase {

    protected abstract IRepository<LeafEntity1, int> CreateRepository();

    protected int SeedRepository(
      IRepository<LeafEntity1, int> repository, int numEntities
    ) {
      var keyToDelete = repository.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { }
      ).Select(r => r.Key).ToArray();
      repository.TryDeleteEntities(keyToDelete);
      for (int i = 1; i <= numEntities; i++) {
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
        repository.AddOrUpdateEntity(entity);
      }
      int highestKey = repository.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { nameof(LeafEntity1.Id) }
      ).Max(r => r.Key);
      return highestKey;
    }

    [TestMethod]
    public void Repository_GetEntityRefs_Works() {

      // Arrange
      IRepository<LeafEntity1, int> repository = this.CreateRepository();

      int highestKey = SeedRepository(repository, 10);

      // Act
      EntityRef<int>[] result = repository.GetEntityRefs(
        ExpressionTree.Empty(), new string[] { $"^{nameof(LeafEntity1.Id)}" }
      );

      // Assert
      Assert.AreEqual(10, result.Length, "Expected 10 entities to be returned.");
      Assert.IsTrue(result.All(r => r.Key > 0), "All entity refs should have a valid Id.");

      Assert.AreEqual(highestKey, result[0].Key, "First entity ref should have a Key of 1.");

      // Act - with filter
      ExpressionTree filterLongGreater = ExpressionTree.And(
        FieldPredicate.Greater(nameof(LeafEntity1.LongValue), 50)
      );
      EntityRef<int>[] filteredResultLongGreater = repository.GetEntityRefs(
        filterLongGreater, new string[] { }
      );

      EntityRef<int>[] filteredResultStringEqual1 = repository.GetEntityRefsBySearchExpression(
        "StringValue == \"Entity 1\"", new string[] { }
      );
      EntityRef<int>[] filteredResultStringEqual2 = repository.GetEntityRefsBySearchExpression(
      "((StringValue = \"Entity 1\"))", new string[] { }
    );

      ExpressionTree filterStringEqual = ExpressionTree.And(
        FieldPredicate.Equal(nameof(LeafEntity1.StringValue), "Entity 1")
      );
      EntityRef<int>[] filteredResultStringEqual = repository.GetEntityRefs(
        filterStringEqual, new string[] { }
      );

      // Assert
      Assert.AreEqual(5, filteredResultLongGreater.Length, "Expected 5 entities to be returned after filtering.");
      Assert.AreEqual(1, filteredResultStringEqual.Length, "Expected 1 entity to be returned after filtering for 'Entity1'.");

      // Additional filter tests

      // Filter: LongValue < 50
      ExpressionTree filterLongLess = ExpressionTree.And(
        FieldPredicate.Less(nameof(LeafEntity1.LongValue), 50)
      );
      var filteredResultLongLess = repository.GetEntityRefs(filterLongLess, new string[] { });
      Assert.AreEqual(4, filteredResultLongLess.Length, "Expected 4 entities with LongValue < 50.");

      // Filter: BoolValue == true
      ExpressionTree filterBoolTrue = ExpressionTree.And(
        FieldPredicate.Equal(nameof(LeafEntity1.BoolValue), true)
      );
      var filteredResultBoolTrue = repository.GetEntityRefs(filterBoolTrue, new string[] { });
      Assert.AreEqual(5, filteredResultBoolTrue.Length, "Expected 5 entities with BoolValue == true.");

      // Filter: FloatValue >= 5.5
      ExpressionTree filterFloatGte = ExpressionTree.And(
        FieldPredicate.GreaterOrEqual(nameof(LeafEntity1.FloatValue), 5.5f)
      );
      var filteredResultFloatGte = repository.GetEntityRefs(filterFloatGte, new string[] { });
      Assert.AreEqual(6, filteredResultFloatGte.Length, "Expected 6 entities with FloatValue >= 5.5.");

      // Filter: DoubleValue <= 8.8
      ExpressionTree filterDoubleLte = ExpressionTree.And(
        FieldPredicate.LessOrEqual(nameof(LeafEntity1.DoubleValue), 8.8)
      );
      var filteredResultDoubleLte = repository.GetEntityRefs(filterDoubleLte, new string[] { });
      Assert.AreEqual(4, filteredResultDoubleLte.Length, "Expected 4 entities with DoubleValue <= 8.8.");

      // Filter: DecimalValue != 9.9
      ExpressionTree filterDecimalNotEqual = ExpressionTree.And(
        FieldPredicate.NotEqual(nameof(LeafEntity1.DecimalValue), 9.9m)
      );
      var filteredResultDecimalNotEqual = repository.GetEntityRefs(filterDecimalNotEqual, new string[] { });
      Assert.AreEqual(9, filteredResultDecimalNotEqual.Length, "Expected 9 entities with DecimalValue != 9.9.");

      // Filter: StringValue contains "Entity"
      ExpressionTree filterStringContains = ExpressionTree.And(
        FieldPredicate.Contains(nameof(LeafEntity1.StringValue), "Entity")
      );
      var filteredResultStringContains = repository.GetEntityRefs(filterStringContains, new string[] { });
      Assert.AreEqual(10, filteredResultStringContains.Length, "Expected 10 entities with StringValue containing 'Entity'.");

      // Filter: DateValue > DateTime.Now.AddDays(-5)
      var dateThreshold = DateTime.Now.AddDays(-5);
      ExpressionTree filterDateGreater = ExpressionTree.And(
        FieldPredicate.Greater(nameof(LeafEntity1.DateValue), dateThreshold)
      );
      var filteredResultDateGreater = repository.GetEntityRefs(filterDateGreater, new string[] { });
      Assert.AreEqual(4, filteredResultDateGreater.Length, "Expected 4 entities with DateValue > threshold.");

      // Filter: GuidValue != Guid.Empty
      ExpressionTree filterGuidNotEmpty = ExpressionTree.And(
        FieldPredicate.NotEqual(nameof(LeafEntity1.GuidValue), Guid.Empty)
      );
      var filteredResultGuidNotEmpty = repository.GetEntityRefs(filterGuidNotEmpty, new string[] { });
      Assert.AreEqual(10, filteredResultGuidNotEmpty.Length, "Expected 10 entities with GuidValue != Guid.Empty.");

      // Filter: Id in [1, 3, 5]
      //ExpressionTree filterIdIn = ExpressionTree.And(
      //  FieldPredicate.In(nameof(LeafEntity1.Id), new[] { 1, 3, 5 })
      //);
      //var filteredResultIdIn = repository.GetEntityRefs(filterIdIn, new string[] { });
      //Assert.AreEqual(3, filteredResultIdIn.Length, "Expected 3 entities with Id in [1, 3, 5].");

      // Filter: Negate (Id == 1)
      ExpressionTree filterIdNot1 = new ExpressionTree {
        MatchAll = true,
        Negate = true,
        Predicates = new System.Collections.Generic.List<FieldPredicate> {
          FieldPredicate.Equal(nameof(LeafEntity1.Id), highestKey)
        }
      };
      var filteredResultIdNot1 = repository.GetEntityRefs(filterIdNot1, new string[] { });
      Assert.AreEqual(9, filteredResultIdNot1.Length, "Expected 9 entities with Id != 1.");
    }

    [TestMethod]
    public void Repository_GetEntities_Works() {
      // Arrange
      var repository = this.CreateRepository();
      SeedRepository(repository, 10);

      // Act
      var allEntities = repository.GetEntities(ExpressionTree.Empty(), new string[] { });

      // Assert
      Assert.AreEqual(10, allEntities.Length, "Expected 10 entities.");

      // Filter: LongValue > 50
      var filtered = repository.GetEntities(
          ExpressionTree.And(FieldPredicate.Greater(nameof(LeafEntity1.LongValue), 50)),
          new string[] { }
      );
      Assert.AreEqual(5, filtered.Length, "Expected 5 entities with LongValue > 50.");

      // Filter: StringValue == "Entity 1"
      var filteredString = repository.GetEntities(
          ExpressionTree.And(FieldPredicate.Equal(nameof(LeafEntity1.StringValue), "Entity 1")),
          new string[] { }
      );
      Assert.AreEqual(1, filteredString.Length, "Expected 1 entity with StringValue == 'Entity 1'.");
      Assert.AreEqual("Entity 1", filteredString[0].StringValue);
    }

    [TestMethod]
    public void Repository_GetEntitiesBySearchExpression_Works() {
      var repository = this.CreateRepository();
      SeedRepository(repository, 10);

      // Act
      var result = repository.GetEntitiesBySearchExpression("LongValue > 50", new string[] { });

      // Assert
      Assert.AreEqual(5, result.Length, "Expected 5 entities with LongValue > 50.");

      var result2 = repository.GetEntitiesBySearchExpression("StringValue == \"Entity 1\"", new string[] { });
      Assert.AreEqual(1, result2.Length, "Expected 1 entity with StringValue == 'Entity 1'.");
    }

    [TestMethod]
    public void Repository_GetEntitiesByKey_Works() {
      var repository = this.CreateRepository();
      int highestKey = SeedRepository(repository, 10);

      // Act
      var result = repository.GetEntitiesByKey(new[] { highestKey, highestKey - 1, highestKey - 2 });

      // Assert
      Assert.AreEqual(3, result.Length, "Expected 3 entities for keys 1, 3, 5.");
      Assert.IsTrue(result.Any(e => e.Id == highestKey));
      Assert.IsTrue(result.Any(e => e.Id == highestKey - 1));
      Assert.IsTrue(result.Any(e => e.Id == highestKey - 2));
    }

    [TestMethod]
    public void Repository_GetEntityFields_Works() {
      var repository = this.CreateRepository();
      int highestKey = SeedRepository(repository, 10);

      // Act
      var fields = repository.GetEntityFields(
          ExpressionTree.And(FieldPredicate.Greater(nameof(LeafEntity1.Id), highestKey - 5)),
          new[] { nameof(LeafEntity1.Id), nameof(LeafEntity1.StringValue) },
          new string[] { }
      );

      // Assert
      Assert.AreEqual(5, fields.Length, "Expected 5 entities with Id > 5.");
      foreach (var dict in fields) {
        Assert.IsTrue(dict.ContainsKey("Id"));
        Assert.IsTrue(dict.ContainsKey("StringValue"));
        Assert.AreEqual(2, dict.Count, "Should only contain Id and StringValue.");
      }
    }

    [TestMethod]
    public void Repository_GetEntityFieldsBySearchExpression_Works() {
      var repository = this.CreateRepository();
      int highestKey = SeedRepository(repository, 10);

      var fields = repository.GetEntityFieldsBySearchExpression(
          $"Id >= {highestKey - 2}",
          new[] { nameof(LeafEntity1.Id), nameof(LeafEntity1.StringValue) },
          new string[] { }
      );

      Assert.AreEqual(3, fields.Length, "Expected 3 entities with Id <= 3.");
      foreach (var dict in fields) {
        Assert.IsTrue(dict.ContainsKey("Id"));
        Assert.IsTrue(dict.ContainsKey("StringValue"));
      }
    }

    [TestMethod]
    public void Repository_GetEntityFieldsByKey_Works() {
      var repository = this.CreateRepository();
      int highestKey = SeedRepository(repository, 10);

      var fields = repository.GetEntityFieldsByKey(
          new[] { highestKey, highestKey - 1 },
          new[] { nameof(LeafEntity1.Id), nameof(LeafEntity1.StringValue) }
      );

      Assert.AreEqual(2, fields.Length, "Expected 2 entities for keys 2 and 4.");
      Assert.IsTrue(fields.All(f => f.ContainsKey("Id") && f.ContainsKey("StringValue")));
    }

    [TestMethod]
    public void Repository_CountAll_And_Count_Works() {
      var repository = this.CreateRepository();
      SeedRepository(repository, 10);

      Assert.AreEqual(10, repository.CountAll(), "Expected 10 entities in total.");

      var count = repository.Count(ExpressionTree.And(FieldPredicate.Greater(nameof(LeafEntity1.LongValue), 50)));
      Assert.AreEqual(5, count, "Expected 5 entities with Id > 5.");
    }

    [TestMethod]
    public void Repository_CountBySearchExpression_Works() {
      var repository = this.CreateRepository();
      SeedRepository(repository, 10);

      var count = repository.CountBySearchExpression("LongValue < 40");
      Assert.AreEqual(3, count, "Expected 3 entities with Id < 4.");
    }

    [TestMethod]
    public void Repository_ContainsKey_Works() {
      var repository = this.CreateRepository();
      int highestKey = SeedRepository(repository, 10);

      Assert.IsTrue(repository.ContainsKey(highestKey), $"Repository should contain key {highestKey}.");
      Assert.IsFalse(repository.ContainsKey(highestKey + 1), $"Repository should not contain key {highestKey + 1}.");
    }

    [TestMethod]
    public void Repository_AddOrUpdateEntity_Works() {
      var repository = this.CreateRepository();
      int highestKey = SeedRepository(repository, 10);

      // Update existing
      var entity = new LeafEntity1 { Id = highestKey, StringValue = "Updated", LongValue = 100 };
      var updated = repository.AddOrUpdateEntity(entity);
      Assert.AreEqual("Updated", updated.StringValue);

      // Add new
      var newEntity = new LeafEntity1 { Id = highestKey + 1, StringValue = "New", LongValue = 999 };
      var added = repository.AddOrUpdateEntity(newEntity);
      Assert.AreEqual(highestKey + 1, added.Id);
      Assert.AreEqual("New", added.StringValue);
    }

    [TestMethod]
    public void Repository_AddOrUpdateEntityFields_Works() {
      var repository = this.CreateRepository();
      SeedRepository(repository, 10);

      // Update existing
      var fields = new System.Collections.Generic.Dictionary<string, object>
      {
            { "Id", 2 },
            { "StringValue", "FieldUpdated" }
        };
      var diff = repository.AddOrUpdateEntityFields(fields);
      Assert.IsFalse(diff.ContainsKey("StringValue"));
      Assert.IsTrue(diff.ContainsKey(nameof(LeafEntity1.DoubleValue)));
      // because value of stored entity is "FieldUpdated" the function only
      // returns the values of the stored entity, that are different than in the given fields

      // Add new
      var newFields = new System.Collections.Generic.Dictionary<string, object>
      {
            { "Id", 100 },
            { "StringValue", "FieldNew" }
        };
      var diffNew = repository.AddOrUpdateEntityFields(newFields);
      Assert.IsFalse(diffNew.ContainsKey("Id"));
      Assert.IsTrue(diff.ContainsKey(nameof(LeafEntity1.DoubleValue)));
    }

    [TestMethod]
    public void Repository_TryUpdateEntity_Works() {
      var repository = this.CreateRepository();
      int highestKey = SeedRepository(repository, 10);

      var entity = new LeafEntity1 { Id = highestKey - 7, StringValue = "TryUpdate" };
      var updated = repository.TryUpdateEntity(entity);
      Assert.IsNotNull(updated);
      Assert.AreEqual("TryUpdate", updated.StringValue);

      var nonExisting = new LeafEntity1 { Id = highestKey + 1, StringValue = "Nope" };
      var result = repository.TryUpdateEntity(nonExisting);
      Assert.IsNull(result);
    }

    [TestMethod]
    public void Repository_TryUpdateEntityFields_Works() {
      var repository = this.CreateRepository();
      int highestKey = SeedRepository(repository, 10);

      var fields = new System.Collections.Generic.Dictionary<string, object>
      {
            { "Id", highestKey-6 },
            { "StringValue", "TryFieldUpdate" }
        };
      var diff = repository.TryUpdateEntityFields(fields);
      Assert.IsFalse(diff.ContainsKey("StringValue"));
      Assert.IsTrue(diff.ContainsKey(nameof(LeafEntity1.DoubleValue)));

      var nonExistingFields = new System.Collections.Generic.Dictionary<string, object>
      {
            { "Id", highestKey+1 },
            { "StringValue", "Nope" }
        };
      var result = repository.TryUpdateEntityFields(nonExistingFields);
      Assert.IsNull(result);
    }

    [TestMethod]
    public void Repository_TryAddEntity_Works() {
      var repository = this.CreateRepository();
      int highestKey = SeedRepository(repository, 10);

      var entity = new LeafEntity1 { Id = highestKey + 1, StringValue = "TryAdd" };
      var key = repository.TryAddEntity(entity);
      Assert.AreEqual(highestKey + 1, key);

      // Try to add existing
      var existing = new LeafEntity1 { Id = highestKey, StringValue = "ShouldNotAdd" };
      var key2 = repository.TryAddEntity(existing);
      Assert.AreEqual(0, key2); // default(int) is 0
    }

    [TestMethod]
    public void Repository_MassupdateByKeys_Works() {
      var repository = this.CreateRepository();
      int highestKey = SeedRepository(repository, 10);

      var keys = new[] { highestKey - 1, highestKey - 2, highestKey - 3 };
      var fields = new System.Collections.Generic.Dictionary<string, object>
      {
            { "StringValue", "MassUpdated" }
        };
      var updatedKeys = repository.MassupdateByKeys(keys, fields);
      Assert.AreEqual(3, updatedKeys.Length);

      var updatedEntities = repository.GetEntitiesByKey(keys);
      Assert.IsTrue(updatedEntities.All(e => e.StringValue == "MassUpdated"));
    }

    [TestMethod]
    public void Repository_Massupdate_Works() {
      var repository = this.CreateRepository();
      int highestKey = SeedRepository(repository, 10);

      var filter = ExpressionTree.And(FieldPredicate.Greater(nameof(LeafEntity1.Id), highestKey - 2));
      var fields = new System.Collections.Generic.Dictionary<string, object>
      {
            { "StringValue", "MassUpdateByFilter" }
        };
      var updatedKeys = repository.Massupdate(filter, fields);
      Assert.AreEqual(2, updatedKeys.Length);

      var updatedEntities = repository.GetEntitiesByKey(updatedKeys);
      Assert.IsTrue(updatedEntities.All(e => e.StringValue == "MassUpdateByFilter"));
    }

    [TestMethod]
    public void Repository_MassupdateBySearchExpression_Works() {
      var repository = this.CreateRepository();
      int highestKey = SeedRepository(repository, 10);

      var fields = new System.Collections.Generic.Dictionary<string, object>
      {
            { "StringValue", "MassUpdateBySearch" }
        };
      var updatedKeys = repository.MassupdateBySearchExpression($"Id < {highestKey - 7}", fields);
      Assert.AreEqual(2, updatedKeys.Length);

      var updatedEntities = repository.GetEntitiesByKey(updatedKeys);
      Assert.IsTrue(updatedEntities.All(e => e.StringValue == "MassUpdateBySearch"));
    }

    [TestMethod]
    public void Repository_TryDeleteEntities_Works() {
      var repository = this.CreateRepository();
      int highestKey = SeedRepository(repository, 10);

      var keysToDelete = new[] { highestKey, highestKey - 1, highestKey + 1 };
      var deleted = repository.TryDeleteEntities(keysToDelete);
      Assert.AreEqual(2, deleted.Length, "Should delete 2 existing entities.");

      Assert.IsFalse(repository.ContainsKey(highestKey));
      Assert.IsFalse(repository.ContainsKey(highestKey - 1));
      Assert.IsTrue(repository.ContainsKey(highestKey - 2));
    }

    [TestMethod]
    public void Repository_TryUpdateKey_Works() {
      var repository = this.CreateRepository();
      int highestKey = SeedRepository(repository, 10);

      var result = repository.TryUpdateKey(highestKey, highestKey + 1);
      Assert.IsTrue(result, "Should update key from 1 to 1000.");
      Assert.IsFalse(repository.ContainsKey(highestKey));
      Assert.IsTrue(repository.ContainsKey(highestKey + 1));

      // Try to update to an existing key
      var result2 = repository.TryUpdateKey(highestKey - 1, highestKey - 2);
      Assert.IsFalse(result2, "Should not update to an existing key.");
    }
  }
}
