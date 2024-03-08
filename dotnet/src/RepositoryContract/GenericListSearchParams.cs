
namespace System.Data.Fuse {

  public class GenericListSearchParams {

    public string EntityName { get; set; }

    public ExpressionTree Filter { get; set; }

    /// <summary>
    /// An array of field names to be used for sorting the results (before 'limit' and 'skip' is processed).
    /// Use the character "^" as prefix for DESC sorting. Sample: ['^Age','Lastname']
    /// </summary>
    public string[] SortedBy { get; set; }

    public int Limit { get; set; }
    public int Skip { get; set; }

  }

}
