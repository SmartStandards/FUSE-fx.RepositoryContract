
namespace System.Data.Fuse {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public interface ICommitable {

    void BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction(); 

  }

}
