﻿using System.Collections.ObjectModel;

namespace ModelReader.Demo.Model {

  public class Employee {

    public int Id { get; set; }
    public string LastName { get; set; } = string.Empty;  
    public string FirstName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Level { get; set; } = 0;
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = string.Empty; 
    public string Email { get; set; } = string.Empty;
        
    // Dependent
    public ObservableCollection<Address> Addresses { get; set; } = new ObservableCollection<Address>();

    // Dependent
    public ContractDetails ContractDetails { get; set; } = null!;

    // LookUp
    public BusinessUnit BusinessUnit { get; set; } = null!;

    // Lookup
    public ObservableCollection<BusinessProject> AssignedBusinessProjects { get; set; } = new ObservableCollection<BusinessProject>();
  }
}
