using System.Collections.Generic;
using System.Data.Fuse;

namespace RepositoryContract.Tests {
  public class Religion {
    public int Id { get; set; }
    public string Name2 { get; set; } = string.Empty;
  }

  public class Person {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public EntityRef<int> Nation { get; set; } = new EntityRef<int>();
    public Religion Religion { get; set; } = new Religion();
    public List<Address> Addresses { get; set; } = new List<Address>();
    public List<Pet> Pets { get; set; } = new List<Pet>();
  }

  public class Address {
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
  }

  public class Pet {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
  }
}
