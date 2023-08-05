using System.Collections.Generic;
using System.Data.Fuse.Logic;

namespace System.Data.Fuse { 

  public class GenericListSearchParams {
    public string EntityName { get; set; }
    public SimpleExpressionTree Filter {  get; set; }
    public PagingParams PagingParams { get; set; }
    public SortingField[] SortingParams { get; set; }
  }

}
