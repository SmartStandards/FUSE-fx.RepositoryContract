using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace System.Data.Fuse {
  public class GenericAddOrUpdateParams {
    public string EntityName { get; set; }
    public Dictionary<string, JsonElement> Entity { get; set; }
  }
}
