using System.ComponentModel.DataAnnotations;

namespace TechDemo.WebApi.DomainObjects {
  [PrimaryIdentity(nameof(Id))]
  [UniquePropertyGroup(nameof(Id), nameof(Id))]
  [PropertyGroup(nameof(PersonId), nameof(PersonId))]
  [PluralName("Addresses")]
  public class Address {
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int PersonId { get; set; }
  }
}
