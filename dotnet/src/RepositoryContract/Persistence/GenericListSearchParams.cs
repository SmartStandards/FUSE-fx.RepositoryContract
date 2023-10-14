using System.Collections.Generic;
using System.Data.Fuse.Logic;

namespace System.Data.Fuse { 

  public class GenericListSearchParams {
    public string EntityName { get; set; }
    public LogicalExpression Filter {  get; set; }
    public PagingParams PagingParams { get; set; }
    public SortingField[] SortingParams { get; set; }
  }

}
