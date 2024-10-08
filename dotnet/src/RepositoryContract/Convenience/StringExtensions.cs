
namespace System.Data.Fuse.Convenience.Internal {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  internal static class StringExtensions {

    public static string CapitalizeFirst(this string str) {
      return char.ToUpper(str[0]) + str.Substring(1);
    }

    public static string ToLowerFirst(this string str) {
      return char.ToLower(str[0]) + str.Substring(1);
    }

  }

}
