
namespace System.Data.UDAS.v1.Models {
  public class ListSearchParams<TSearchFilter> {
    public string SortingField { get; set; }
    public bool SordDescending { get; set; }
    public TSearchFilter SearchFilter { get; set; }
    public int Pagesize { get; set; }
    public int Pagenumber { get; set; }
  }
  public class ListSearchParamsByParent {
    public string SortingField { get; set; }
    public bool SordDescending { get; set; }
    public string ParentId { get; set; }
    public int Pagesize { get; set; }
    public int Pagenumber { get; set; }
  }
}
