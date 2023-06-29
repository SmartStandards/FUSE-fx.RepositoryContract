using System.Data.Fuse.Logic;

namespace System.Data.Fuse { 

  public class GenericListSearchParams {
    public string EntityName { get; set; }
    public SimpleExpressionTree Filter {  get; set; }
  }

}
