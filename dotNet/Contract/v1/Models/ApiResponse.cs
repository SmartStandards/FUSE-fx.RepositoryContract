
namespace System.Data.UDAS.v1.Models {
  public class ApiResponse<T> {
    public T Return { get; set; }
    public string Fault { get; set; }
  }
}
