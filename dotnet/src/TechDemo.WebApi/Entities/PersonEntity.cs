using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace TechDemo.WebApi.Entities {

  [PrimaryIdentity(nameof(Id))]
  [UniquePropertyGroup(nameof(Id), nameof(Id))]
  [HasDependent(nameof(Addresses), nameof(AddressEntity.PersonId),nameof(AddressEntity.Person),null,nameof(AddressEntity))]
  [HasLookup(nameof(Nation), nameof(NationId), "", null, nameof(NationEntity))]
  public class PersonEntity {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public int NationId { get; set; }
    public virtual NationEntity Nation { get; set; } = null!;

    public virtual ObservableCollection<AddressEntity> Addresses { get; set; } = null!;
  }
}
