namespace RepositoryContract.Demo.Model {
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