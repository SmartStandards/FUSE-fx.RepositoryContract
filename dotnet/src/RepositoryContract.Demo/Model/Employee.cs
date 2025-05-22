using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace RepositoryContract.Demo.Model {

  [PrimaryIdentity(nameof(Id))]
  [PropertyGroup(nameof(Id), nameof(Id))]
  [PropertyGroup(nameof(BusinessUnitId), nameof(BusinessUnitId))]
  public class Employee : IEquatable<Employee> {

    public int Id { get; set; }

    public long SomeUid { get; set; }

    public Guid SomeGuid { get; set; } = Guid.NewGuid();

    public string LastName { get; set; } = string.Empty;  
    public string FirstName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Level { get; set; } = 0;
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = string.Empty; 
    public string Email { get; set; } = string.Empty;


    [Dependent]
    public virtual ObservableCollection<Address> Addresses { get; set; } = new ObservableCollection<Address>();

    public int? ContractDetailsId { get; set; } = null;
    [Dependent]
    public virtual ContractDetails? ContractDetails { get; set; } = null;

    public int? BusinessUnitId { get; set; } = null;
    [Lookup]
    public virtual BusinessUnit? BusinessUnit { get; set; } = null;

    [Lookup]
    public virtual ObservableCollection<BusinessProject> AssignedBusinessProjects { get; set; } = new ObservableCollection<BusinessProject>();

    public bool Equals(Employee? other) {
      return this.Id == other?.Id;
    }

    public override string ToString() {
      return $"{FirstName} {LastName}";
    }
  }
}
