using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Fuse;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContract.Tests.Logic {
  [TestClass]
  public class FieldPredicateTests {

    [TestMethod]
    public void SerializeValueForNetFramework_Array_Works() {
      IList<long> list = new List<long>() { 1, 2 };
      Assert.AreEqual("[1,2]", FieldPredicate.SerializeValueForNetFramework(list));
    }

  }
}
