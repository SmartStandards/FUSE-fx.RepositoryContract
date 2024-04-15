namespace System.Data.Fuse {
  public enum NavigationRole {
    Dependent = 1,
    Lookup = 2,
    Referrer = 4,
    Principal = 8,
    All = 15,
  }
}
