using System.Collections;
using System.Collections.Generic;

namespace System.Data.Fuse {

  public class PaginatedResponse {

    public IList Page { get; set; }

    public int Total { get; set; }

  }

  public class PaginatedResponse<T> {

    public IList<T> Page { get; set; }

    public int Total { get; set; }

  }

}