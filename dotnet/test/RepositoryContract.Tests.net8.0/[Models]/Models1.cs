using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Fuse;

namespace RepositoryContract.Tests {
  [PrimaryIdentity(nameof(Id))]
  [PropertyGroup(nameof(Id), nameof(Id))]
  public class Religion {
    public int Id { get; set; }
    public string Name2 { get; set; } = string.Empty;
  }

  [PrimaryIdentity(nameof(Id))]
  [PropertyGroup(nameof(Id), nameof(Id))]
  [HasLookup(nameof(Nation), "NationId", "", null, nameof(Nation))]
  [HasLookup(nameof(Religion), "ReligionId", "", null, nameof(Religion))]
  [HasDependent(nameof(Addresses), "PersonId", null, null, nameof(Address))]
  [HasDependent(nameof(Pets), "PersonId", null, null, nameof(Pet))]
  public class Person {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public EntityRef<int> Nation { get; set; } = new EntityRef<int>();
    public Religion Religion { get; set; } = new Religion();
    public List<Address> Addresses { get; set; } = new List<Address>();
    public List<Pet> Pets { get; set; } = new List<Pet>();
  }

  [PrimaryIdentity(nameof(Id))]
  [PropertyGroup(nameof(Id), nameof(Id))]
  public class Address {
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
  }

  [PrimaryIdentity(nameof(Id))]
  [PropertyGroup(nameof(Id), nameof(Id))]
  public class Pet {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
  }

  [PrimaryIdentity(nameof(Id))]
  [PropertyGroup(nameof(Id), nameof(Id))]
  public class Nation {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
  }
}
