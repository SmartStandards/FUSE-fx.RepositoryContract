using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Fuse;
using System.Data.ModelDescription;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContract.Tests {
  
  [TestClass]
  public class RepoFactoryTests {

    private class SampleEntity1 {
      public int Id { get; set; }
    }
    private class SampleEntity2 {
      public int Id { get; set; }
    }
    private class SampleEntity3 {
      public int Id { get; set; }
    }

    private class SampleRepoFacade 
      : IRepoFactory<SampleEntity1, int>,
       IRepoFactory<SampleEntity2,int>,
      IDataStore {
      public void BeginTransaction() {
        throw new NotImplementedException();
      }  

      public void CommitTransaction() {
        throw new NotImplementedException();
      }

      public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class {
        throw new NotImplementedException();
      }

      public SchemaRoot GetSchemaRoot() {
        throw new NotImplementedException();
      }

      public void RollbackTransaction() {
        throw new NotImplementedException();
      }

      //IRepository<SampleEntity1, int> IRepoFactory<SampleEntity1, int>.GetRepository1() {
      //  throw new NotImplementedException();
      //}
      public IRepository<SampleEntity1, int> GetRepositoryInternal() {
        throw new NotImplementedException();
      }

      IRepository<SampleEntity2, int> IRepoFactory<SampleEntity2, int>.GetRepositoryInternal() {
        throw new NotImplementedException();
      }

      public Tuple<Type, Type>[] GetManagedTypes() {
        throw new NotImplementedException();
      }
    }

    [TestMethod, Ignore]
    public void GetRepository1_ReturnsRepository() {
      SampleRepoFacade repo = new SampleRepoFacade();
      IRepository<SampleEntity1, int> r1 =  repo.GetRepository1<SampleEntity1>();
    }

  }
}
