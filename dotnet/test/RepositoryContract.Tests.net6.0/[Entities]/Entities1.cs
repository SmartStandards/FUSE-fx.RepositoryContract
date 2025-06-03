using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContract.Tests {

  [PrimaryIdentity(nameof(Id))]
  [PropertyGroup(nameof(Id), nameof(Id))]
  [HasLookup(nameof(Nation), nameof(NationId), "", null, nameof(NationEntity))]
  [HasLookup(nameof(Religion), nameof(ReligionId), "", null, nameof(ReligionEntity))]
  [HasDependent(nameof(Addresses), nameof(AddressEntity.PersonId), "")]
  [HasDependent("", nameof(PetEntity.PersonId), "", "", nameof(PetEntity))]
  public class PersonEntity {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int NationId { get; set; }
    public int ReligionId { get; set; }
    public ReligionEntity Religion { get; set; } = null!;
    public NationEntity Nation { get; set; } = null!;
    public ICollection<AddressEntity> Addresses { get; set; } = new ObservableCollection<AddressEntity>();
  }

  public class NationEntity {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
  }

  public class ReligionEntity {
    public int Id { get; set; }
    public string Name2 { get; set; } = string.Empty;
  }

  [PrimaryIdentity(nameof(Id))]
  [PropertyGroup(nameof(Id), nameof(Id))]
  [PropertyGroup(nameof(PersonId), nameof(PersonId))]
  public class AddressEntity {
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public int PersonId { get; set; }
  }

  public class PetEntity {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PersonId { get; set; }
  }
}
