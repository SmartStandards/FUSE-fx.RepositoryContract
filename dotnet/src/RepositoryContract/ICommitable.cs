namespace System.Data.Fuse {

  public interface ICommitable {

    void BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction(); 

  }
}
