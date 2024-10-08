
namespace System.Data.Fuse {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public interface ICommitableRepository<TEntity, TKey>
    : IRepository<TEntity, TKey>, ISchemaProvider
    where TEntity : class { 
  
  }

}
