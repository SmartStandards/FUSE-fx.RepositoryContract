
namespace System.Data.Fuse {

  //TODO: überlegen, ob es für strings ein ~~ und !~
  //für eine CASE-INSENSITIVE suche geben könnte

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public static class FieldOperators {
    
    /// <summary> Valid for all data types </summary>
    public const string NotEqual = "!=";

    /// <summary> Valid for all data types </summary>
    public const string Equal = "==";

    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Valid for all NUMERIC data types and DATES 
    /// (for other data types it will affect in matching never)
    /// </summary>
    public const string Less = "<";

    /// <summary>
    /// Valid for all NUMERIC data types and DATES 
    /// (for strings it will affect in matching 'SubstringOf')
    /// </summary>
    public const string LessOrEqual = "<=";

    /// <summary>
    /// Valid for all NUMERIC data types and DATES 
    /// (for other data types it will affect in matching never)
    /// </summary>
    public const string Greater = ">";

    /// <summary>
    /// Valid for all NUMERIC data types and DATES 
    /// (for strings it will affect in matching 'Contains')
    /// </summary>
    public const string GreaterOrEqual = ">=";

    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Valid for all STRINGs ONLY
    /// (for other data types it will affect in matching never)
    /// </summary>
    public const string StartsWith = "|*";

    /// <summary>
    /// Valid for all STRINGs
    /// (for other data types it will affect in matching 'LessOrEqual')
    /// </summary>
    public const string SubstringOf = "<=";
    //<=

    /// <summary>
    /// Valid for all STRINGs ONLY
    /// (for other data types it will affect in matching never)
    /// </summary>
    public const string EndsWith = "*|";

    /// <summary>
    /// Valid for all STRINGs
    /// (for other data types it will affect in matching 'GreaterOrEqual')
    /// </summary>
    public const string Contains = ">=";

    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Valid for all data types.
    /// In this special case the given 'value' to match must NOT be scalar!
    /// Instead it must be an ARRAY. A match is given if a field equals to
    /// at least one value within that array.
    /// </summary>
    public const string In = "in";

  }

}
