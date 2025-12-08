
namespace System.Data.Fuse {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// An property bag which holds information about the implemented/supported
  /// capabilities of an IRepository.
  /// </summary>
  public class RepositoryCapabilities {

    public static RepositoryCapabilities All {
      get {
        return new RepositoryCapabilities() {
          CanReadContent = true,
          CanUpdateContent = true,
          CanAddNewEntities = true,
          CanDeleteEntities = true,
          SupportsMassupdate = true,
          SupportsKeyUpdate = true,
          SupportsStringBasedSearchExpressions = true,
          RequiresExternalKeys = false
        };
      }
    }

    /// <summary> 
    /// Indicates, that this repository offers access to load entities(classes) or some their entity fields
    /// (if this is false, then only EntityRefs are accessable)
    /// </summary>
    public bool CanReadContent { get; set; } = false;

    public bool CanUpdateContent { get; set; } = false;
    public bool CanAddNewEntities { get; set; } = false;
    public bool CanDeleteEntities { get; set; } = false;
    public bool SupportsMassupdate { get; set; } = false;
    public bool SupportsKeyUpdate { get; set; } = false;

    /// <summary>
    /// Server side Content-Search over multiple fields.
    /// Depending on the concrete implementation this should be a mixture of
    /// A) fulltext-seach (on fields with 'content' semantics) and/or 
    /// B) exact-match (on fields with 'Identifier' semantics.
    /// Potentially the results are based on an external index-db.
    /// This is optimized for the usecase, where the consumer ui offers just a single 'Search'-Textbox,
    /// </summary>
    public bool SupportsStringBasedSearchExpressions { get; set; } = false;

    /// <summary> 
    /// Indicates, that entities can only be added to this repository,
    /// if ther key fields are pre-initialized by the caller. If false,
    /// then the persistence-technology behind the repository implementation
    /// will auto-generate a new key by its own.
    /// </summary>
    public bool RequiresExternalKeys { get; set; } = false;

  }

}
