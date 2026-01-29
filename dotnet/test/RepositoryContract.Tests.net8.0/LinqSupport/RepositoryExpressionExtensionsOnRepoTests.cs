using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryContract.Tests;
using System;
using System.Collections.Generic;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Cache;

namespace System.Data.Fuse.LinqSupport {

  [TestClass]
  public class RepositoryExpressionExtensionsOnRepoTests {

    private class Person {
      public int Id { get; set; }
      public string Name { get; set; }
      public int Age { get; set; }
      public bool IsActive { get; set; }
      public string Country { get; set; }
    }

    #region " Init Repository & Schema "

    private static SchemaRoot _SchemaRoot = new SchemaRoot {
      Entities = new EntitySchema[] {
        new EntitySchema {
          Name = nameof(Person),
          PrimaryKeyIndexName = "PK_Person",
          Fields = [
            new FieldSchema { Name = "Id", Type = "int" },
            new FieldSchema { Name = "Name", Type = "string" },
            new FieldSchema { Name = "Age", Type = "int" },
            new FieldSchema { Name = "IsActive", Type = "bool" },
            new FieldSchema { Name = "Country", Type = "string" }
          ],
          Indices = [
            new IndexSchema { Name = "PK_Person", Unique = true, MemberFieldNames = [ "Id" ] }
          ]
        }
      }

    };

    private static IRepository<Person, int> InitRepo() {
      IRepository<Person, int> repo = new InMemoryRepository<Person, int>(_SchemaRoot);

      repo.AddOrUpdateEntity(new Person { Id = 1, Name = "Alice", Age = 30, IsActive = true, Country = "USA" });
      repo.AddOrUpdateEntity(new Person { Id = 2, Name = "Bob", Age = 25, IsActive = false, Country = "Canada" });
      repo.AddOrUpdateEntity(new Person { Id = 3, Name = "Charlie", Age = 35, IsActive = true, Country = "USA" });
      repo.AddOrUpdateEntity(new Person { Id = 4, Name = "Diana", Age = 28, IsActive = true, Country = "UK" });
      repo.AddOrUpdateEntity(new Person { Id = 5, Name = "Ethan", Age = 40, IsActive = false, Country = "Canada" });
      repo.AddOrUpdateEntity(new Person { Id = 6, Name = "Fiona", Age = 32, IsActive = true, Country = "USA" });

      repo.AddOrUpdateEntity(new Person { Id = 7, Name = "Gunter", Age = 45, IsActive = true, Country = "Germany" });
      repo.AddOrUpdateEntity(new Person { Id = 8, Name = "Heidi", Age = 29, IsActive = false, Country = "Germany" });
      repo.AddOrUpdateEntity(new Person { Id = 9, Name = "Ingrid", Age = 33, IsActive = true, Country = "Germany" });
      repo.AddOrUpdateEntity(new Person { Id = 10, Name = "Jürgen", Age = 38, IsActive = true, Country = "Germany" });
      repo.AddOrUpdateEntity(new Person { Id = 11, Name = "Katrin", Age = 27, IsActive = false, Country = "Germany" });
      repo.AddOrUpdateEntity(new Person { Id = 12, Name = "Lars", Age = 31, IsActive = true, Country = "Germany" });
      repo.AddOrUpdateEntity(new Person { Id = 13, Name = "Monika", Age = 36, IsActive = true, Country = "Germany" });

      return repo;
    }

    #endregion

    [TestMethod]
    public void LinqSupp_PrediacteTest1() {
      IRepository<Person, int> repo = InitRepo();

      Person[] results = repo.GetEntitiesWhere((p) => p.IsActive && p.Country == "Germany" && p.Age > 30);

      Assert.AreEqual(5, results.Length);
      Assert.IsTrue(results.All(p => p.IsActive && p.Country == "Germany" && p.Age > 30));

    }

    [TestMethod]
    public void LinqSupp_PrediacteTest2() {
      IRepository<Person, int> repo = InitRepo();

      Person[] results = repo.GetEntitiesWhere((p) => !p.IsActive && p.Country == "Germany");

      Assert.AreEqual(2, results.Length);
      Assert.IsTrue(results.All(p => !p.IsActive && p.Country == "Germany"));

    }

    // --------------------------------------------------------------------
    // COMPLEX NESTED LOGICAL AND/OR/NOT (3 levels)
    // --------------------------------------------------------------------
    [TestMethod]
    public void LinqSupp_ComplexNestedPredicate_ThreeLevels_1() {
      IRepository<Person, int> repo = InitRepo();

      Person[] results = repo.GetEntitiesWhere(
          p =>
              (p.Country == "Germany" && (p.Age > 30 && p.IsActive))
              ||
              (!p.IsActive && p.Country == "USA")
      );

      // Should match:
      // Germany active > 30 → Ids 7,9,10,12,13
      // USA inactive → none
      Assert.AreEqual(5, results.Length);
      Assert.IsTrue(results.All(p =>
          (p.Country == "Germany" && p.Age > 30 && p.IsActive)
          ||
          (!p.IsActive && p.Country == "USA")
      ));
    }

    // --------------------------------------------------------------------
    // NOT over nested AND/OR tree
    // --------------------------------------------------------------------
    [TestMethod]
    public void LinqSupp_ComplexNestedPredicate_NotOverGroup() {
      IRepository<Person, int> repo = InitRepo();

      Person[] results = repo.GetEntitiesWhere(
          p =>
              !(p.Country == "Germany" && p.Age > 35)
              && p.IsActive
      );

      // Germany & Age > 35 → Ids: 7 (45), 10 (38), 12 (31 no), 13 (36 yes)
      // Inverted set: Active but NOT (Germany & Age > 35)

      Person[] allActive = repo.GetEntitiesWhere(p => p.IsActive);
      int expected = allActive
          .Where(p => !(p.Country == "Germany" && p.Age > 35))
          .Count();

      Assert.AreEqual(expected, results.Length);
      Assert.IsTrue(results.All(p => p.IsActive && !(p.Country == "Germany" && p.Age > 35)));
    }

    // --------------------------------------------------------------------
    // OR inside AND inside OR
    // --------------------------------------------------------------------
    [TestMethod]
    public void LinqSupp_ComplexNestedPredicate_ThreeLevels_2() {
      IRepository<Person, int> repo = InitRepo();

      Person[] results = repo.GetEntitiesWhere(
          p =>
              (
                  (p.Country == "USA" || p.Country == "Canada")
                  &&
                  (p.Age > 25)
              )
              ||
              (p.IsActive && p.Age < 30)
      );

      Person[] expected = repo
          .GetEntitiesWhere(x => true)
          .Where(p =>
              (
                  (p.Country == "USA" || p.Country == "Canada")
                  && p.Age > 25
              )
              ||
              (p.IsActive && p.Age < 30)
          )
          .ToArray();

      Assert.AreEqual(expected.Length, results.Length);
      Assert.IsTrue(results.All(expected.Contains));
    }

    // --------------------------------------------------------------------
    // Left/right swapped operands: constant == field
    // --------------------------------------------------------------------
    [TestMethod]
    public void LinqSupp_SwappedOperands_Equal_LeftConstant() {
      IRepository<Person, int> repo = InitRepo();

      Person[] results = repo.GetEntitiesWhere(p => "Germany" == p.Country);

      Assert.AreEqual(7, results.Length);
      Assert.IsTrue(results.All(p => p.Country == "Germany"));
    }

    [TestMethod]
    public void LinqSupp_SwappedOperands_GreaterOrEqual_LeftConstant() {
      IRepository<Person, int> repo = InitRepo();

      Person[] results = repo.GetEntitiesWhere(p => 30 <= p.Age); // equivalent to p.Age >= 30

      Person[] expected = repo
          .GetEntitiesWhere(x => true)
          .Where(p => p.Age >= 30)
          .ToArray();

      Assert.AreEqual(expected.Length, results.Length);
      Assert.IsTrue(results.All(expected.Contains));
    }

    // --------------------------------------------------------------------
    // Boolean swapped operands: true == p.IsActive
    // --------------------------------------------------------------------
    [TestMethod]
    public void LinqSupp_SwappedOperands_Bool_EqualTrue() {
      IRepository<Person, int> repo = InitRepo();

      Person[] results = repo.GetEntitiesWhere(p => true == p.IsActive);

      Person[] expected = repo.GetEntitiesWhere(p => p.IsActive);

      Assert.AreEqual(expected.Length, results.Length);
      Assert.IsTrue(results.All(p => p.IsActive));
    }

    // --------------------------------------------------------------------
    // Deep AND + OR + NOT combination (max complexity)
    // --------------------------------------------------------------------
    [TestMethod]
    public void LinqSupp_VeryComplexPredicate_ThreeLevelMix() {
      IRepository<Person, int> repo = InitRepo();

      Person[] results = repo.GetEntitiesWhere(
          p =>
              (
                  (p.Country == "Germany" && p.Age > 30)
                  ||
                  (!p.IsActive && p.Country == "Canada")
              )
              &&
              !(
                  (p.Age < 25)
                  ||
                  (p.Country == "UK" && p.IsActive)
              )
      );

      Person[] expected = repo.GetEntitiesWhere(p => true)
          .Where(p =>
              (
                  (p.Country == "Germany" && p.Age > 30)
                  ||
                  (!p.IsActive && p.Country == "Canada")
              )
              &&
              !(
                  (p.Age < 25)
                  ||
                  (p.Country == "UK" && p.IsActive)
              )
          )
          .ToArray();

      Assert.AreEqual(expected.Length, results.Length);
      Assert.IsTrue(results.All(expected.Contains));
    }

    // --------------------------------------------------------------------
    // NOT inside OR inside AND (to stress recursion)
    // --------------------------------------------------------------------
    [TestMethod]
    public void LinqSupp_NestedPredicate_NotInsideOrInsideAnd() {
      IRepository<Person, int> repo = InitRepo();

      Person[] results = repo.GetEntitiesWhere(
          p =>
              (p.IsActive && (p.Age > 25 || !p.Country.StartsWith("G")))
      );

      Person[] expected = repo.GetEntitiesWhere(x => true)
          .Where(p =>
              (p.IsActive && (p.Age > 25 || !p.Country.StartsWith("G")))
          )
          .ToArray();

      Assert.AreEqual(expected.Length, results.Length);
      Assert.IsTrue(results.All(expected.Contains));
    }

    // --------------------------------------------------------------------
    // Contains + AND + NOT + swapped comparison
    // --------------------------------------------------------------------
    [TestMethod]
    public void LinqSupp_MixedOperators_Contains_And_Not_Swapped() {
      IRepository<Person, int> repo = InitRepo();

      string[] allowedCountries = new[] { "Germany", "USA", "UK" };

      Person[] results = repo.GetEntitiesWhere(
          p =>
              allowedCountries.Contains(p.Country)
              &&
              !(50 > p.Age)   // swapped → p.Age < 50
              &&
              p.Name.Contains("a")
      );

      Person[] expected = repo.GetEntitiesWhere(x => true)
          .Where(p =>
              allowedCountries.Contains(p.Country)
              && !(50 > p.Age)
              && p.Name.Contains("a", StringComparison.OrdinalIgnoreCase)
          )
          .ToArray();

      Assert.AreEqual(expected.Length, results.Length);
      Assert.IsTrue(results.All(expected.Contains));
    }

    // --------------------------------------------------------------------
    // FieldSelector
    // --------------------------------------------------------------------

    [TestMethod]
    public void LinqSupp_FieldSelector_SingleProperty() {

      string[] names = SelectorMapper.ExtractSelectorFieldNames(
        (Person p) => p.Age
      );

      Assert.IsNotNull(names);
      Assert.AreEqual(1, names.Length);
      Assert.AreEqual(names[0], nameof(Person.Age));

    }

    [TestMethod]
    public void LinqSupp_FieldSelector_MultiPropertyAnonymous() {

      string[] names = SelectorMapper.ExtractSelectorFieldNames(
        (Person p) => new{ p.Age, p.Country }
      );

      Assert.IsNotNull(names);
      Assert.AreEqual(2, names.Length);
      Assert.AreEqual(names[0], nameof(Person.Age));
      Assert.AreEqual(names[1], nameof(Person.Country));

    }

    [TestMethod]
    public void LinqSupp_FieldSelector_MultiPropertyClass() {

      string[] names = SelectorMapper.ExtractSelectorFieldNames(
        (Person p) => new Person { Age = p.Age, Country = p.Country }
      );

      Assert.IsNotNull(names);
      Assert.AreEqual(2, names.Length);
      Assert.AreEqual(names[0], nameof(Person.Age));
      Assert.AreEqual(names[1], nameof(Person.Country));

    }

    [TestMethod]
    public void LinqSupp_FieldSelector_Self() {

      string[] names = SelectorMapper.ExtractSelectorFieldNames(
        (Person p) => p
      );

      Assert.IsNotNull(names);
      Assert.AreEqual(5, names.Length);
      Assert.AreEqual(names[0], nameof(Person.Id));
      Assert.AreEqual(names[1], nameof(Person.Name));
      Assert.AreEqual(names[2], nameof(Person.Age));
      Assert.AreEqual(names[3], nameof(Person.IsActive));
      Assert.AreEqual(names[4], nameof(Person.Country));

    }

  }

}
