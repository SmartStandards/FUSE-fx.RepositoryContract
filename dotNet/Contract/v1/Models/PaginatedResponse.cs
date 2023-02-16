using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.UDAS.v1.Models {
  public class PaginatedResponse<T> {
    public IList<T> Page { get; set; }
    public int TotalNumber { get; set; }
  }
}
