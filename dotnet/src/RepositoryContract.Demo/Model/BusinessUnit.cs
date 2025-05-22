using System.ComponentModel.DataAnnotations;

namespace RepositoryContract.Demo.Model {
  [PrimaryIdentity(nameof(Id))]
  [PropertyGroup(nameof(Id), nameof(Id))]
  public class BusinessUnit {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public override string ToString() {
      return $"{this.Name} ({this.Description})";
    }
  }
}