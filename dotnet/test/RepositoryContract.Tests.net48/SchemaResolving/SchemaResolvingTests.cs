using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryContract.Tests.Net4;
using System;
using System.Collections.Generic;
using System.Data.Fuse;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Fuse.SchemaResolving {

  [TestClass]
  public class SchemaResolvingTests {

    [TestMethod]
    public void TestEF4SchemaResolving1() {
      IEntityResolver resolver = new AssemblySearchEntityResolver(
        typeof(Person).Assembly, typeof(Person).Namespace
      );

      Type[] entityTypes = resolver.GetWellknownTypes();
      Assert.AreEqual(4, entityTypes.Length);

    }

    [TestMethod]
    public void TestEF4SchemaResolving2() {
      IEntityResolver resolver = new DbContextDeclaratedEntityResolver(
        typeof(MyDbContext), false
      );

      Type[] entityTypes = resolver.GetWellknownTypes();
      Assert.AreEqual(2, entityTypes.Length);

    }

    [TestMethod, Ignore()] //auf ignore, da er wegen EF4 ewig läuft...
    public void TestEF4SchemaResolving3() {
      IEntityResolver resolver = new DbContextRuntimeEntityResolver(
       () => new MyDbContext(), false
      ); 

      Type[] entityTypes = resolver.GetWellknownTypes();
      Assert.AreEqual(2, entityTypes.Length);

    }

  }

}
