namespace System.Data.Fuse {

  public interface ICommitableRepository<T>
    : IRepository<T>, ICommitable
    where T : class { }

}