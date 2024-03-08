
namespace System.Data.Fuse {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public static class ValueMatchBehaviour {

    /// <summary> Valid for all data types </summary>
    public const int NotEqual = 0;

    /// <summary> Valid for all data types </summary>
    public const int Equal = 1;

    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Valid for all NUMERIC data types and DATES 
    /// (for strings it will affect in matching 'EndsWith')
    /// </summary>
    public const int Less = 2;

    /// <summary>
    /// Valid for all NUMERIC data types and DATES 
    /// (for strings it will affect in matching 'SubstringOf')
    /// </summary>
    public const int LessOrEqual = 3;

    /// <summary>
    /// Valid for all NUMERIC data types and DATES 
    /// (for strings it will affect in matching 'StartsWith')
    /// </summary>
    public const int More = 4;

    /// <summary>
    /// Valid for all NUMERIC data types and DATES 
    /// (for strings it will affect in matching 'Contains')
    /// </summary>
    public const int MoreOrEqual = 5;

    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Valid for all STRINGs ONLY
    /// (for other datatypes it will affect in matching 'Less')
    /// </summary>
    public const int EndsWith = 2;

    /// <summary>
    /// Valid for all STRINGs ONLY
    /// (for other datatypes it will affect in matching 'LessOrEqual')
    /// </summary>
    public const int SubstringOf = 3;

    /// <summary>
    /// Valid for all STRINGs ONLY
    /// (for other datatypes it will affect in matching 'More')
    /// </summary>
    public const int StartsWith = 4;

    /// <summary>
    /// Valid for all STRINGs ONLY
    /// (for other datatypes it will affect in matching 'MoreOrEqual')
    /// </summary>
    public const int Contains = 5;

    /// <summary>
    /// Valid for all STRINGs ONLY
    /// (for other datatypes it will affect in matching 'NotEqual')
    /// </summary>
    public const int ContainsNot = 6;

    ////////////////////////////////////////////////////////////////////

  }

}
