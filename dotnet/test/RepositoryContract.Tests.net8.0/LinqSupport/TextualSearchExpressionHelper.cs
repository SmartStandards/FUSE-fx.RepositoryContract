using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace System.Data.Fuse.LinqSupport {

  [TestClass]
  public class TextualSearchExpressionHelperTest {

    private class TestEntity_WithoutAnyAttribs {
      public int Id { get; set; }
      public string Name { get; set; } = string.Empty;
      public string Value { get; set; } = string.Empty;
      public long ASnowflakeId { get; set; }
      public Guid AGuid { get; set; } = Guid.Empty;
      public bool ABool { get; set; } = true;
    }

    private class TestEntity_1 {

      //should be skipped because of explicit usage of Content&Filterable attribs on other props
      public int Id { get; set; }
      
      [Content, Filterable(Filterability.ExactMatch)] //should be included, but as ExactMatch!
      public string Name { get; set; } = string.Empty;

      [Content,  Filterable(Filterability.None)] //should be skipped because of Filterability.None
      public string Value { get; set; } = string.Empty;

      [Content, SystemInternal] //should be skipped because of SystemInternal
      public long ASnowflakeId { get; set; }

      //should be skipped because of explicit usage of Content&Filterable attribs on other props
      public Guid AGuid { get; set; } = Guid.Empty;

      [Content] //should be skipped because of bool is not supported
      public bool ABool { get; set; } = true;
    }


    [TestMethod]
    public void ÁttributeBasedFilterability_IncludesCorrectSetOfProps() {
      FilterabilityInfo[] info;

      info = TextualSearchExpressionHelper.GetFilterabilityInfos(typeof(TestEntity_WithoutAnyAttribs));
      Assert.AreEqual(5, info.Length);
      Assert.AreEqual(Filterability.ExactMatch, info[0].EnumValue);
      Assert.AreEqual(Filterability.Substring, info[1].EnumValue);
      Assert.AreEqual(Filterability.Substring, info[2].EnumValue);
      Assert.AreEqual(Filterability.ExactMatch, info[3].EnumValue);
      Assert.AreEqual(Filterability.ExactMatch, info[4].EnumValue);

      info = TextualSearchExpressionHelper.GetFilterabilityInfos(typeof(TestEntity_1));
      Assert.AreEqual(1, info.Length);

      Assert.AreEqual(nameof(TestEntity_1.Name), info[0].Property.Name);
      Assert.AreEqual(Filterability.ExactMatch, info[0].EnumValue);

    }

    [TestMethod]
    public void ÁttributeBasedFilterability_BuildsValidExpression() {

      var entitites = new List<TestEntity_WithoutAnyAttribs>();

      entitites.Add(new TestEntity_WithoutAnyAttribs {
        Id = 12,
        Name = "Test1",
        Value = "Value2",
        ASnowflakeId = 123,
        AGuid = Guid.Parse("{430017B6-CAFA-0CA8-1111-4EE68A019E7B}"),
        ABool = true
      });

      entitites.Add(new TestEntity_WithoutAnyAttribs {
        Id = 22,
        Name = "Bar",
        Value = "Value1",
        ASnowflakeId = 456,
        AGuid = Guid.Parse("{430017B6-CAFA-0CA8-2222-4EE68A019E7B}"),
        ABool = false
      });

      entitites.Add(new TestEntity_WithoutAnyAttribs {
        Id = 33,
        Name = "Foooooo",
        Value = "Bar22",
        ASnowflakeId = 789,
        AGuid = Guid.Parse("{430017B6-CAFA-0CA8-3333-4EE68A019E7B}"),
        ABool = true
      });

      TestEntity_WithoutAnyAttribs[] result;

      result = entitites.WhereContentContains("Bar").ToArray();
      Assert.AreEqual(2, result.Length);
      Assert.AreEqual(22, result[0].Id);
      Assert.AreEqual(33, result[1].Id);

      result = entitites.WhereContentContains("2").ToArray();
      Assert.AreEqual(2, result.Length);
      Assert.AreEqual(12, result[0].Id);
      Assert.AreEqual(33, result[1].Id);

      result = entitites.WhereContentContains("22").ToArray();
      Assert.AreEqual(2, result.Length);
      Assert.AreEqual(22, result[0].Id);
      Assert.AreEqual(33, result[1].Id);

      result = entitites.WhereContentContains("3333").ToArray();
      Assert.AreEqual(0, result.Length);

      result = entitites.WhereContentContains("430017B6-CAFA-0CA8-3333-4EE68A019E7B").ToArray();
      Assert.AreEqual(1, result.Length);
      Assert.AreEqual(33, result[0].Id);

    }

  }

}
