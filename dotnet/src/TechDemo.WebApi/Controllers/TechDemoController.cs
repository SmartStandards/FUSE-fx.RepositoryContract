using Microsoft.AspNetCore.Mvc;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using TechDemo.WebApi.DomainObjects;
using TechDemo.WebApi.Entities;
using TechDemo.WebApi.Persistence;

namespace TechDemo.WebApi.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class TechDemoController : ControllerBase {
   
    private readonly ILogger<TechDemoController> _Logger;
    private readonly TechDemoEfDataStore _EfDataStore;
    private readonly TechDemoMveDataStore _MveDataStore;
    private readonly TechDemoSqlDataStore _SqlDataStore;

    public TechDemoController(
      ILogger<TechDemoController> logger, 
      TechDemoEfDataStore efDataStore,
      TechDemoMveDataStore mveDataStore,
      TechDemoSqlDataStore sqlDataStore
    ) {
      _Logger = logger;
      _EfDataStore = efDataStore;
      _MveDataStore = mveDataStore;
      _SqlDataStore = sqlDataStore;
    }

    [HttpGet(Name = "Test")]
    public IActionResult Test() {
      //TestEfDataStore();
      //TestMveDataStore();

      Nation nation1 = _SqlDataStore.AddOrUpdate<Nation, int>(new Nation() {
        Code = 1,
        Name = "United States",
      });
      Person person1 = _SqlDataStore.AddOrUpdate<Person, int>(new Person() {
        Name = "John Doe",
        NationId = nation1.Id,
      });
      _SqlDataStore.AddOrUpdate<Address, int>(new Address() {
        City = "New York",
        Street = "123 Main St",
        PersonId = person1.Id
      });
      Person[] people = _SqlDataStore.GetEntities<Person, int>(ExpressionTree.Empty());
      var loadedNation = people[0].Nation;
      var loadedAddresses = people[0].Addresses;

      return Ok();
    }

    private void TestMveDataStore() {
      Nation nation1 = _MveDataStore.AddOrUpdate<Nation, int>(new Nation() {
        Code = 1,
        Name = "United States",
      });
      Person person1 = _MveDataStore.AddOrUpdate<Person, int>(new Person() {
        Name = "John Doe",
        NationId = nation1.Id,
      });
      _MveDataStore.AddOrUpdate<Address, int>(new Address() {
        City = "New York",
        Street = "123 Main St",
        PersonId = person1.Id
      });
      Person[] people = _MveDataStore.GetEntities<Person, int>(ExpressionTree.Empty());
      var loadedNation = people[0].Nation;
    }

    private void TestEfDataStore() {
      NationEntity nation1 = _EfDataStore.AddOrUpdate<NationEntity, int>(new NationEntity() {
        Code = 1,
        Name = "United States",
      });
      PersonEntity person1 = _EfDataStore.AddOrUpdate<PersonEntity, int>(new PersonEntity() {
        Name = "John Doe",
        NationId = nation1.Id,
      });
      _EfDataStore.AddOrUpdate<AddressEntity, int>(new AddressEntity() {
        City = "New York",
        Street = "123 Main St",
        PersonId = person1.Id
      });

      PersonEntity[] people = _EfDataStore.GetEntities<PersonEntity, int>(ExpressionTree.Empty());
      NationEntity loadedNation = people[0].Nation;
    }
  }
}
