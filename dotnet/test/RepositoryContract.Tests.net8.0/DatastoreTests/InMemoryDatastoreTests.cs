using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryTests;
using System;
using System.Collections.Generic;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContract.Tests {

  [TestClass]
  public class InMemoryDatastoreTests : DatastoreTestsBase {
    protected override IDataStore CreateDataStore() {
      SchemaRoot schemaRoot = ModelReader.GetSchema(
        new Type[] {
          typeof(LeafEntity1) ,
          typeof(LeafEntity2),
          typeof(RootEntity1),
          typeof(RootEntity2),
          typeof(ChildEntity1),
          typeof(ChildEntity2),
          typeof(LeafEntityWithCompositeKey),
          typeof(RootEntityWithCompositeKey),
          typeof(ChildEntityOfRootEntityWithCompositeKey)
        }, false
      );
      return new InMemoryDataStore(schemaRoot);
    }
  }
}
