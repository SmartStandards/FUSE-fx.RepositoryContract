using System.ComponentModel.DataAnnotations;

namespace TechDemo.WebApi.Entities {
  [PrimaryIdentity(nameof(Id))]
  [UniquePropertyGroup(nameof(Id), nameof(Id))]
  public class AddressEntity {
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int PersonId { get; set; }
    public virtual PersonEntity Person { get; set; } = null!;
  }
}
