
namespace System.Data.UDAS.v1.Models {
  public class EntityRef {
    public object[] KeyValues { get; set; }
    public string Label { get; set; } = string.Empty;
  }
  public class EntityRefById {
    public string Id { get; set; }
    public string Label { get; set; } = string.Empty;
  }
}
