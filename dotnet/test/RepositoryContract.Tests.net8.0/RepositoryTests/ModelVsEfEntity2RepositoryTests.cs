using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Convenience;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.Fuse.Ef;
using System.Data.Fuse.Ef.InstanceManagement;
using System.Data.Fuse.Sql;
using System.Data.Fuse.Sql.InstanceManagement;
using System.Data.ModelDescription;
using System.Data.SqlClient;
using System.Linq;

namespace RepositoryTests {

  [TestClass]
  public class ModelVsEfEntity2RepositoryTests : ModelVsEntityRepositoryTestsBase {

    protected override IRepository<LeafEntity1, int> CreateLeaf1EntityRepository() {   
      var options = new DbContextOptionsBuilder<TestDbContext2>().UseSqlServer(
        "Server=(localdb)\\mssqllocaldb;Database=EfRepositoryTestsDb2;Trusted_Connection=True;MultipleActiveResultSets=true"
      ).Options;

      using var context = new TestDbContext2(options);

      EfRepository<LeafEntity1, int> efRepo = new EfRepository<LeafEntity1, int>(
        new ShortLivingDbContextInstanceProvider<TestDbContext2>()
      );

      return new ModelVsEntityRepository<LeafEntity1, LeafEntity1, int>(
        efRepo, new ModelVsEntityParams<LeafEntity1, LeafEntity1>()
      );
    }

    protected override IRepository<LeafEntity2, int> CreateLeafEntity2Repository() {
      var options = new DbContextOptionsBuilder<TestDbContext2>()
                      .UseInMemoryDatabase(databaseName: "TestDb")
                      .Options;

      using var context = new TestDbContext2(options);

      EfRepository<LeafEntity2, int> efRepo = new EfRepository<LeafEntity2, int>(
        new ShortLivingDbContextInstanceProvider<TestDbContext2>()
      );

      return new ModelVsEntityRepository<LeafEntity2, LeafEntity2, int>(
        efRepo, new ModelVsEntityParams<LeafEntity2, LeafEntity2>()
      );
    }

    protected override IDataStore CreateEntityDatastore() {
      return new EfDataStore<TestDbContext2>(
        new ShortLivingDbContextInstanceProvider<TestDbContext2>()
      );
    }
    
  }
}
