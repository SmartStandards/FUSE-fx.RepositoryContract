namespace System.Data.Fuse {

  public interface ICommitableRepository<TEntity, TKey>
    : IRepository<TEntity, TKey>, ICommitable
    where TEntity : class { }

}