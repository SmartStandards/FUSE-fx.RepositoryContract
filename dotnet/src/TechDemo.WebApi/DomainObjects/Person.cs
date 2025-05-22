using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Fuse;

namespace TechDemo.WebApi.DomainObjects {

  [PrimaryIdentity(nameof(Id))]
  [UniquePropertyGroup(nameof(Id), nameof(Id))]
  [PropertyGroup(nameof(NationId), nameof(NationId))]
  [PluralName("People")]
  [HasDependent(nameof(Addresses), nameof(Address.PersonId), "", null, nameof(Address))]
  [HasLookup(nameof(Nation), nameof(NationId), "", null, nameof(Nation))]
  public class Person {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int NationId { get; set; }
    public virtual Nation Nation { get; set; } = null!;

    public virtual EntityRef<int>[] Addresses { get; set; } = null!;
  }
}
