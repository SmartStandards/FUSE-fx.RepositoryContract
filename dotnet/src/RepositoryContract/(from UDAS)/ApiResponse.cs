
//TODO: rename namespace of the final versions to "System.Data.Fuse"
namespace System.Data.UDAS.v1.Models {

  public class ApiResponse {
    public string Fault { get; set; }

    public static ApiResponse<T> Ok<T>(T returnValue) {
      return new ApiResponse<T>() { Return = returnValue };
    }
    public static ApiResponse<T> Error<T>(string faultMessage) {
      return new ApiResponse<T>() { Fault = faultMessage };
    }
  }

  public class ApiResponse<T> {
    public T Return { get; set; }
    public string Fault { get; set; }
   
  }

}
