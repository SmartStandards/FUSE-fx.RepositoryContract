using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryContract.Tests._Models_;
using System;
using System.Collections.Generic;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.Fuse.Ef;
using System.Data.Fuse.SchemaResolving;
using System.Data.ModelDescription;
using System.Linq;
using System.Reflection;

namespace RepositoryContract.Tests {

  [TestClass]
  public class ConversionHelperTests {

    private static IEntityResolver _Resolver = new AssemblySearchEntityResolver(
      typeof(PersonEntity).Assembly, typeof(PersonEntity).Namespace
    );

    [TestMethod]
    public void LoadNavigations_Works() {

      // Arrange

      SchemaRoot entitySchemaRoot = ModelReader.GetSchema(
        typeof(PersonEntity).Assembly,
        new string[] {
          nameof(PersonEntity), nameof(NationEntity), nameof(ReligionEntity)
        }
      );
      SchemaRoot modelSchemaRoot = ModelReader.GetSchema(
        typeof(PersonEntity).Assembly,
        new string[] {
            nameof(Person), nameof(Religion)
        }
      );

      IUniversalRepository universalEntityRepository = new InMemoryUniversalRepository(
        entitySchemaRoot, _Resolver
      );
      IDataStore localEntityDataStore = new InMemoryDataStore(entitySchemaRoot);
      IRepository<ReligionEntity, int> religionRepo = localEntityDataStore.GetRepository<ReligionEntity, int>();

      RepositoryCollection modelDataStore = new RepositoryCollection(_Resolver);
      modelDataStore.RegisterRepository(
        new ModelVsEntityRepository<Religion, ReligionEntity, int>(
          religionRepo,
          new ModelVsEntityParams<Religion, ReligionEntity>()
        )
      );

      universalEntityRepository.AddOrUpdateEntity(
        nameof(NationEntity),
        new NationEntity() { Id = 1, Name = "Germany" }
      );
      universalEntityRepository.AddOrUpdateEntity(
        nameof(ReligionEntity),
        new ReligionEntity() { Id = 1, Name2 = "Buddhism" }
      );
      PersonEntity entity = new PersonEntity() {
        Id = 0,
        Name = "TestName",
        Value = "TestValue",
        NationId = 1,
        ReligionId = 1
      };

      var handleProperty = ConversionHelper.LoadNavigations<PersonEntity, Person>(
        entitySchemaRoot,
        (entityName, keys) => universalEntityRepository.GetEntityRefsByKey(entityName, keys),
        (modelType, keys) => modelDataStore.GetEntitiesByKey(modelType.Name, keys),
        (modelType, keys) => throw new NotImplementedException(),
        (modelType, keys) => throw new NotImplementedException(),
        NavigationRole.Lookup, false
      );

      // Act
      Person testModel = entity.ToBusinessModel<PersonEntity, Person>(handleProperty);

      // Assert
      Assert.IsNotNull(testModel);
      Assert.IsNotNull(testModel.Nation);
      Assert.AreEqual(1, testModel.Nation.Key);
      Assert.IsNotNull(testModel.Religion);
      Assert.AreEqual(1, testModel.Religion.Id);
    }

    [TestMethod]
    public void CreateModelVsEntityRepositry_Works() {

      // Arrange

      SchemaRoot entitySchemaRoot = ModelReader.GetSchema(
        typeof(PersonEntity).Assembly,
        new string[] {
          nameof(PersonEntity),
          nameof(NationEntity),
          nameof(ReligionEntity),
          nameof(AddressEntity),
          nameof(PetEntity)
        }
      );
      SchemaRoot modelSchemaRoot = ModelReader.GetSchema(
        typeof(PersonEntity).Assembly,
        new string[] {
          nameof(Person),
          nameof(Religion),
          nameof(Address),
          nameof(Pet)
        }
      );

      IUniversalRepository universalEntityRepository = new InMemoryUniversalRepository(
        entitySchemaRoot, _Resolver
      );
      IDataStore localEntityDataStore = new InMemoryDataStore(entitySchemaRoot);
      IRepository<ReligionEntity, int> religionRepo = localEntityDataStore.GetRepository<ReligionEntity, int>();
      IRepository<AddressEntity, int> addressRepo = localEntityDataStore.GetRepository<AddressEntity, int>();
      IRepository<PetEntity, int> petRepo = localEntityDataStore.GetRepository<PetEntity, int>();

      RepositoryCollection modelDataStore = new RepositoryCollection(_Resolver);

      modelDataStore.RegisterRepository(
        new ModelVsEntityRepository<Religion, ReligionEntity, int>(
          religionRepo,
          new ModelVsEntityParams<Religion, ReligionEntity>()
        )
      );
      modelDataStore.RegisterRepository(
        new ModelVsEntityRepository<Address, AddressEntity, int>(
          addressRepo,
          new ModelVsEntityParams<Address, AddressEntity>()
        )
      );
      modelDataStore.RegisterRepository(
        new ModelVsEntityRepository<Pet, PetEntity, int>(
          petRepo,
          new ModelVsEntityParams<Pet, PetEntity>()
        )
      );

      universalEntityRepository.AddOrUpdateEntity(
        nameof(NationEntity),
        new NationEntity() { Id = 1, Name = "Germany" }
      );
      universalEntityRepository.AddOrUpdateEntity(
        nameof(ReligionEntity),
        new ReligionEntity() { Id = 1, Name2 = "Buddhism" }
      );
      PersonEntity entity = new PersonEntity() {
        Id = 1,
        Name = "TestName",
        Value = "TestValue",
        NationId = 1,
        ReligionId = 1
      };
      universalEntityRepository.AddOrUpdateEntity(nameof(PersonEntity), entity);
      AddressEntity addressEntity = new AddressEntity() {
        Id = 1,
        Street = "TestStreet",
        PersonId = entity.Id
      };
      universalEntityRepository.AddOrUpdateEntity(nameof(AddressEntity), addressEntity);

      universalEntityRepository.AddOrUpdateEntity(
        nameof(PetEntity),
        new PetEntity() { Id = 1, Name = "Bobby", PersonId = entity.Id }
      );

      ModelVsEntityRepository<Person, PersonEntity, int> personRepo = ConversionHelper.CreateModelVsEntityRepositry<
        Person, PersonEntity, int
      >(
        localEntityDataStore, modelDataStore,
        NavigationRole.Lookup | NavigationRole.Dependent,
        true
      );

      // Act
      Person[] people = personRepo.GetEntities(new ExpressionTree(), new string[] { });

      // Assert
      Assert.IsTrue(people.Count() > 0);
      Person person = people[0];
      Assert.IsNotNull(person);
      Assert.IsNotNull(person.Nation);
      Assert.AreEqual(1, person.Nation.Key);
      Assert.IsNotNull(person.Religion);
      Assert.AreEqual(1, person.Religion.Id);
      List<Address> addresses = person.Addresses;
      Assert.IsNotNull(addresses);
      Assert.IsTrue(addresses.Count > 0);
      List<Pet> pets = person.Pets;
      Assert.IsNotNull(pets);
      Assert.IsTrue(pets.Count > 0);

      // Act
      Person newPerson = personRepo.AddOrUpdateEntity(
        new Person {
          Name = "New Person",
          Nation = new EntityRef<int>() { Key = 1, Label = "Germany" },
          Religion = new Religion { Id = 1 },
        }
      );

      // Assert
      Assert.IsNotNull(newPerson);
      Assert.IsNotNull(newPerson.Nation);
      Assert.AreEqual(1, newPerson.Nation.Key);
      Assert.IsNotNull(newPerson.Religion);
      Assert.AreEqual(1, newPerson.Religion.Id);
    }

    [TestMethod]
    public void CreateDictVsEntityRepositry_Works() {

      // Arrange

      var resolver = new ListBasedEntityResolver(
          typeof(PersonEntity),
          typeof(NationEntity),
          typeof(ReligionEntity),
          typeof(AddressEntity),
          typeof(PetEntity)
      );

      SchemaRoot entitySchemaRoot = ModelReader.GetSchema(resolver.GetWellknownTypes(), true);

      IUniversalRepository universalDictVsEntityRepository = new InMemoryDictUniversalRepository(
        entitySchemaRoot, _Resolver
      );
      Dictionary<string, object> nationValues = new Dictionary<string, object>();
      nationValues["Id"] = 1;
      nationValues["Name"] = "Germany";

      Dictionary<string, object> religionValues = new Dictionary<string, object>();
      religionValues["Id"] = 1;
      religionValues["Name2"] = "Buddhism";

      Dictionary<string, object> personValues = new Dictionary<string, object>();
      personValues["Id"] = 1;
      personValues["Name"] = "TestName";
      personValues["Value"] = "TestValue";
      personValues["NationId"] = 1;
      personValues["Religion"] = religionValues;

      Dictionary<string, object> addressValues = new Dictionary<string, object>();
      addressValues["Id"] = 1;
      addressValues["Street"] = "TestStreet";
      addressValues["PersonId"] = 1;

      // Act

      Dictionary<string, object> nation = (Dictionary<string, object>)universalDictVsEntityRepository.AddOrUpdateEntity(
        nameof(NationEntity),
        nationValues
      );
      Dictionary<string, object> religion = (Dictionary<string, object>)universalDictVsEntityRepository.AddOrUpdateEntity(
        nameof(ReligionEntity),
        religionValues
      );
      Dictionary<string, object> address = (Dictionary<string, object>)universalDictVsEntityRepository.AddOrUpdateEntity(
        nameof(AddressEntity),
        addressValues
      );
      Dictionary<string, object> person = (Dictionary<string, object>)universalDictVsEntityRepository.AddOrUpdateEntity(
        nameof(PersonEntity),
        personValues
      );

      // Assert
      Assert.IsNotNull(nation);
      Assert.AreEqual(1, nation["Id"]);
      Assert.IsNotNull(religion);
      Assert.AreEqual(1, religion["Id"]);
      Assert.IsNotNull(person);
      Assert.AreEqual(1, person["Id"]);
      EntityRef nationRef = (EntityRef)person["Nation"];
      Assert.IsNotNull(nationRef);
      Assert.AreEqual(1, nationRef.Key);
      EntityRef religionRef = (EntityRef)person["Religion"];
      Assert.IsNotNull(religionRef);
      Assert.AreEqual(1, religionRef.Key);
      EntityRef<object>[] addressesRef = (EntityRef<object>[])person["Addresses"];
      Assert.IsNotNull(addressesRef);
      Assert.AreEqual(1, addressesRef.Length);
      EntityRef<object> addressRef = addressesRef[0];
      Assert.IsNotNull(addressRef);
      Assert.AreEqual(1, addressRef.Key);

    }

    [TestMethod]
    public void LoadNavigations_PreventsCircularLoading() {
      // Arrange

      SchemaRoot entitySchemaRoot = ModelReader.GetSchema(
        typeof(PrincipalEntity).Assembly,
        new string[] {
          nameof(PrincipalEntity), nameof(StudentEntity)
        }
      );
      SchemaRoot modelSchemaRoot = ModelReader.GetSchema(
        typeof(Principal).Assembly,
        new string[] {
            nameof(Principal), nameof(Student)
        }
      );


      IUniversalRepository universalEntityRepository = new InMemoryUniversalRepository(
        entitySchemaRoot, _Resolver
      );
      IDataStore localEntityDataStore = new InMemoryDataStore(entitySchemaRoot);
      IRepository<StudentEntity, int> studentRepo = localEntityDataStore.GetRepository<StudentEntity, int>();
      IRepository<PrincipalEntity, int> princinpalRepo = localEntityDataStore.GetRepository<PrincipalEntity, int>();

      RepositoryCollection modelDataStore = new RepositoryCollection(_Resolver);
      ModelVsEntityRepository<Principal, PrincipalEntity, int> principalModelVsEntityRepo = ConversionHelper.CreateModelVsEntityRepositry<
        Principal, PrincipalEntity, int
      >(
        localEntityDataStore, modelDataStore,
        NavigationRole.Lookup | NavigationRole.Dependent | NavigationRole.Principal,
        true
      );
      ModelVsEntityRepository<Student, StudentEntity, int> studentModelVsEntityRepo = ConversionHelper.CreateModelVsEntityRepositry<
       Student, StudentEntity, int
     >(
       localEntityDataStore, modelDataStore,
       NavigationRole.Lookup | NavigationRole.Dependent | NavigationRole.Principal,
       true
     );

      modelDataStore.RegisterRepository(principalModelVsEntityRepo);
      modelDataStore.RegisterRepository(studentModelVsEntityRepo);

      PrincipalEntity principal = new PrincipalEntity() { Id = 1, Name = "Principal1" };
      universalEntityRepository.AddOrUpdateEntity(
       nameof(PrincipalEntity),
       principal
     );
      universalEntityRepository.AddOrUpdateEntity(
        nameof(StudentEntity),
        new StudentEntity() { Id = 1, Name = "Student1", PrincipalId = principal.Id }
      );

      // Act

      try {
        Principal[] principals = principalModelVsEntityRepo.GetEntities(new ExpressionTree(), new string[] { });
        // Assert
        Assert.IsNotNull(principals);
        Assert.IsTrue(principals.Length > 0);
      } catch (Exception ex) {
        throw;
      }

    }

  }

}
