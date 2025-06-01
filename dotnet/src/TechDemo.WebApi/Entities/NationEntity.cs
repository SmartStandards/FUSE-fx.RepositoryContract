using System.ComponentModel.DataAnnotations;

namespace TechDemo.WebApi.Entities {
  [PrimaryIdentity(nameof(Id))]
  [UniquePropertyGroup(nameof(Id), nameof(Id))]
  public class NationEntity {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public NationCode Code { get; set; }
  }
}
