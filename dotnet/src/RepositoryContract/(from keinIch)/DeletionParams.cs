using System.Data.Fuse.Logic;
using System.Text.Json;

namespace System.Data.Fuse {

  public class DeletionParams {
    public string EntityName { get; set; }
    public JsonElement[][] IdsToDelete { get; set; }
  }

}
