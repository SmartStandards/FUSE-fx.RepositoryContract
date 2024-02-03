using System.Collections.Generic;

namespace System.Data.Fuse {
  public class GenericAddOrUpdateParams {
    public string EntityName { get; set; }
    public Dictionary<string, object> Entity { get; set; }
  }
}
