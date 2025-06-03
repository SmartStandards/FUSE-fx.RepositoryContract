using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Data.Fuse.Convenience;

namespace RepositoryContract.Tests {

  [TestClass]
  public class DataStoreTests {

    [TestMethod]
    public void Test() {
      SchemaRoot entitySchemaRoot = ModelReader.GetSchema(
       new Type[] {
         typeof(PersonEntity),
         typeof(NationEntity),
         typeof(ReligionEntity),
         typeof(AddressEntity),
         typeof(PetEntity)
       }, false
      );

      IDataStore dataStore = new InMemoryDataStore(entitySchemaRoot);

      PersonEntity person = dataStore.AddOrUpdate<PersonEntity, int>(new PersonEntity() {
        Id = 1,
        Name = "John Doe",
        Value = "Value1",
        NationId = 1,
        ReligionId = 1
      });

      //Add address under person
      AddressEntity address = dataStore.AddOrUpdate<AddressEntity, int>(new AddressEntity() {
        Id = 1,
        Street = "123 Main St",
        PersonId = person.Id
      });

      PersonEntity parentPerson = address.GetPrimary1<PersonEntity, AddressEntity>(dataStore);
      Assert.IsNotNull(parentPerson);

      AddressEntity[] addressesOfPerson = parentPerson.GetForeign1<PersonEntity, AddressEntity>(dataStore);
      Assert.IsNotNull(addressesOfPerson);
      Assert.AreEqual(1, addressesOfPerson.Length);
    }

  }
}
