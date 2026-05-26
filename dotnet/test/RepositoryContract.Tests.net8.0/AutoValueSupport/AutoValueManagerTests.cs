using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Fuse;
using System.Data.Fuse.AutoValueSupport;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;

namespace RepositoryContract.Demo.Tests {

  [TestClass]
  public class AutoValueManagerTests {

    [TestMethod]
    public void ApplyValuesOnAdd_InitializesHighestValueOnlyOncePerScope() {
      AutoValueManager manager = new AutoValueManager(typeof(AutoValueEntity));
      string scopeKey = Guid.NewGuid().ToString();

      AutoValueEntity firstEntity = new AutoValueEntity();
      manager.ApplyValuesOnAdd(firstEntity, new ThrowAfterFirstEnumerationEnumerable(new[] {
        new AutoValueEntity { Number = 3 },
        new AutoValueEntity { Number = 7 }
      }), scopeKey);

      Assert.AreEqual(8, firstEntity.Number);

      AutoValueEntity secondEntity = new AutoValueEntity();
      manager.ApplyValuesOnAdd(secondEntity, new ThrowingEnumerable(), scopeKey);

      Assert.AreEqual(9, secondEntity.Number);
    }

    private class AutoValueEntity {
      [IncrementAutoValue]
      public int Number { get; set; }
    }

    [UniquePropertyGroup("PK", nameof(Id))]
    [PrimaryIdentity("PK")]
    private class AutoValueEntity2 {
      public Guid Id { get; set; }
      [Identity]
      public int OtherId { get; set; }
    }

    [TestMethod]
    public void InMemoryRepo_AutoValues_Work() {
      SchemaRoot schemaRoot = ModelReader.GetSchema(new Type[] { typeof(AutoValueEntity2) }, false);
      IRepository<AutoValueEntity2, Guid> repo = new InMemoryRepository<AutoValueEntity2, Guid>(
        schemaRoot, true
      );

      AutoValueEntity2 entity1 = repo.AddOrUpdateEntity(new AutoValueEntity2 { 
        Id=Guid.NewGuid(),
      });
      Assert.AreEqual(1, entity1.OtherId);
      AutoValueEntity2 entity2 = repo.AddOrUpdateEntity(new AutoValueEntity2 { 
        Id=Guid.NewGuid(),
      });
      Assert.AreEqual(2, entity2.OtherId);

      // Identity fields are immutable after insert — an explicit value must be silently ignored
      AutoValueEntity2 entity1AfterUpdate = repo.AddOrUpdateEntity(
        new AutoValueEntity2 { Id = entity1.Id, OtherId = 100 }
      );
      Assert.AreEqual(1, entity1AfterUpdate.OtherId, "Identity field must not be changed by an explicit value on update.");
    }

    private class ThrowAfterFirstEnumerationEnumerable : IEnumerable {
      private readonly IEnumerable _inner;
      private bool _wasEnumerated;

      public ThrowAfterFirstEnumerationEnumerable(IEnumerable inner) {
        _inner = inner;
      }

      public IEnumerator GetEnumerator() {
        if (_wasEnumerated) {
          throw new InvalidOperationException("Enumeration should only happen once.");
        }

        _wasEnumerated = true;
        return _inner.GetEnumerator();
      }
    }

    private class ThrowingEnumerable : IEnumerable {
      public IEnumerator GetEnumerator() {
        throw new InvalidOperationException("Enumeration should not happen after the initial cache population.");
      }
    }
  }
}
