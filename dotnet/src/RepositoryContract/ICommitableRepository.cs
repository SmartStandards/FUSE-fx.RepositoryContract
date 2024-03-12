
namespace System.Data.Fuse {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public interface ICommitableRepository<TEntity, TKey>
    : IRepository<TEntity, TKey>, ICommitable
    where TEntity : class { 
  
  }

}
