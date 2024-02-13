using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Fuse.Convenience;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RepositoryContract.Demo.Tests {

  [TestClass]
  public class ConversionExtensionsTests {

    private class TestLookUpEntity {
      public int Id { get; set; }
      public string Name { get; set; } = string.Empty;
    }

    private class TestEntity {
      public int Id { get; set; }
      public string Name { get; set; } = string.Empty;
      public string Value { get; set; } = string.Empty;
      public int TestLookUpEntityId { get; set; }

    }

    [TestMethod]
    public void Serialize_Works() {
      TestEntity entity = new TestEntity() { 
        Id = 0, Name = "TestName", Value = "TestValue", TestLookUpEntityId = 1 
      };

      string entityJson = "{\"id\":1,\"testLookUpEntityId\": 1}";
      Dictionary<string, object>? test = JsonSerializer.Deserialize<Dictionary<string, object>>(entityJson); 
      
      TestEntity entityAfterDeserialization = ConversionExtensions.Deserialize<TestEntity>(test);
      Assert.IsNotNull(entityAfterDeserialization);
    }

  }
}
