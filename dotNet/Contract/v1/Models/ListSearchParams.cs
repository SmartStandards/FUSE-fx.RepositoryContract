using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.UDAS.v1.Models {
  public class ListSearchParams<TSearchFilter> {
    public string SortingField { get; set; }
    public bool SordDescending { get; set; }
    public TSearchFilter SearchFilter { get; set; }
    public int Pagesize { get; set; }
    public int Pagenumber { get; set; }
  }
}
