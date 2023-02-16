
namespace System.Data.UDAS.v1.Models {
  public class ApiResponse<T> {
    public T Return { get; set; }
    public string Fault { get; set; }
    public static ApiResponse<T> Ok(T returnValue) {
      return new ApiResponse<T>() { Return = returnValue };
    }
    public static ApiResponse<T> Error(string faultMessage) {
      return new ApiResponse<T>() { Fault = faultMessage };
    }
  }
}
