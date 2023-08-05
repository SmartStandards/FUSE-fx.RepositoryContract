namespace RepositoryContract.Demo.Model {
  public class Address {
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    public int EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;
  }
}