
namespace System.Data.Fuse {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public enum NavigationRole {
    Dependent = 1,
    Lookup = 2,
    Referrer = 4,
    Principal = 8,
    All = 15,
  }

}
