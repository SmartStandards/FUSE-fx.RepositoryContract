using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Fuse;
using System.Data.Fuse.FileSupport;
using System.Data.ModelDescription;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContract.Tests {

  [TestClass]
  public class FIleRepositoryTests {

    protected static SchemaRoot SchemaRoot => ModelReader.GetSchema(
      typeof(PersonEntity).Assembly,
        new string[] {
          nameof(PersonEntity),
          nameof(NationEntity),
          nameof(ReligionEntity),
          nameof(AddressEntity),
          nameof(PetEntity)
        }
      );

    [TestMethod]
    public void FileRepository_AddOrUpdate_Works() {
      FileRepository<ReligionEntity, int> fileRepository = new FileRepository<ReligionEntity, int>(
        Path.GetTempPath(), SchemaRoot
      );

      // Add a new entity
      ReligionEntity religion = new ReligionEntity() {
        Id = 1,
        Name2 = "Christian"
      };

      fileRepository.AddOrUpdateEntity(religion);

      ReligionEntity[] religionEntities = fileRepository.GetEntitiesBySearchExpression("1=1", new string[] { });

      Assert.IsTrue(religionEntities.Count() == 1);

      // Update the entity
      ReligionEntity religion2 = new ReligionEntity() {
        Id = 1,
        Name2 = "Buddhism"
      };
      fileRepository.AddOrUpdateEntity(religion2);

      religionEntities = fileRepository.GetEntitiesBySearchExpression("1=1", new string[] { });

      Assert.IsTrue(religionEntities.Count() == 1);
      Assert.IsTrue(religionEntities.First().Name2 == "Buddhism");

    }   

  }
}
