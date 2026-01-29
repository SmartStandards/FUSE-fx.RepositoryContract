using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace System.Data.Fuse.Convenience {

  [TestClass]
  public class SelectorMapperTests {

    private class Person {
      public string Name { get; set; } = "";
      public int Age { get; set; }
    }

    [TestMethod]
    public void MapToDict_FullProjection_MapsProperties() {

      Dictionary<string, object>[] rows = new Dictionary<string, object>[] {
        new Dictionary<string, object> {
          { "Name", "Alice" },
          { "Age", 30 }
        },
        new Dictionary<string, object> {
          { "Name", "Bob" },
          { "Age", 42 }
        }
      };

      Person[] mapped = SelectorMapper.Map(
        rows, (Person p) => p
      );

      Assert.AreEqual(2, mapped.Length);
      Assert.AreEqual("Alice", mapped[0].Name);
      Assert.AreEqual(30, mapped[0].Age);
      Assert.AreEqual("Bob", mapped[1].Name);
      Assert.AreEqual(42, mapped[1].Age);

    }

    [TestMethod]
    public void MapToDict_NewProjectionViaConstrcutor_MapsProperties() {

      Dictionary<string, object>[] rows = new Dictionary<string, object>[] {
        new Dictionary<string, object> {
          { "Name", "Alice" },
          { "Age", 30 }
        },
        new Dictionary<string, object> {
          { "Name", "Bob" },
          { "Age", 42 }
        }
      };

      Tuple<string, int>[] mapped = SelectorMapper.Map(
        rows, (Person p) => new Tuple<string,int>(p.Name,p.Age)
      );

      Assert.AreEqual(2, mapped.Length);
      Assert.AreEqual("Alice", mapped[0].Item1);
      Assert.AreEqual(30, mapped[0].Item2);
      Assert.AreEqual("Bob", mapped[1].Item1);
      Assert.AreEqual(42, mapped[1].Item2);

    }

    [TestMethod]
    public void MapToDict_NewAnonymous_MapsProperties() {

      Dictionary<string, object>[] rows = new Dictionary<string, object>[] {
        new Dictionary<string, object> {
          { "Name", "Alice" },
          { "Age", 30 }
        },
        new Dictionary<string, object> {
          { "Name", "Bob" },
          { "Age", 42 }
        }
      };

      var mapped = SelectorMapper.Map(
        rows, (Person p) => new { FirstName=p.Name, p.Age, }
      );

      Assert.AreEqual(2, mapped.Length);
      Assert.AreEqual("Alice", mapped[0].FirstName);
      Assert.AreEqual(30, mapped[0].Age);
      Assert.AreEqual("Bob", mapped[1].FirstName);
      Assert.AreEqual(42, mapped[1].Age);

    }

    [TestMethod]
    public void MapToDict_AtomicPrimitive_MapsProperties() {

      Dictionary<string, object>[] rows = new Dictionary<string, object>[] {
        new Dictionary<string, object> {
          { "Name", "Alice" },
          { "Age", 30 }
        },
        new Dictionary<string, object> {
          { "Name", "Bob" },
          { "Age", 42 }
        }
      };

      int[] mapped = SelectorMapper.Map(
        rows, (Person p) => p.Age
      );

      Assert.AreEqual(2, mapped.Length);
      Assert.AreEqual(30, mapped[0]);
      Assert.AreEqual(42, mapped[1]);

    }

  }

}
