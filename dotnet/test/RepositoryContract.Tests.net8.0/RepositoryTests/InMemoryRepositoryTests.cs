using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryContract.Tests;
using System;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Linq;

namespace RepositoryTests {

  [TestClass]
  public class InMemoryRepositoryTests : RepositoryTestsBase {

    protected override IRepository<LeafEntity1, int> CreateRepository() {
      SchemaRoot schemaRoot = ModelReader.GetSchema(
          new Type[] { typeof(LeafEntity1) }, false
        );
      return new InMemoryRepository<LeafEntity1, int>(schemaRoot);
    }
   
  }
}
