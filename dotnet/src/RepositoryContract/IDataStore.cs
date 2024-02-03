namespace System.Data.Fuse {
  public interface IDataStore : ICommitable{

    IRepository<T> GetRepository<T>()
      where T : class;

  }
}
