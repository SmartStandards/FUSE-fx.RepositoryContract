using System.Data.ModelDescription;

namespace System.Data.Fuse {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public interface IRepoFactory<TEntity, TKey> where TEntity : class {

    IRepository<TEntity, TKey> GetRepositoryInternal();

  }

  public static class RepoFactoryExtensions {

    public static IRepository<TEntity, TKey> GetRepository1<TEntity, TKey>(this IRepoFactory<TEntity, TKey> factory) where TEntity : class {
      return factory.GetRepositoryInternal();
    }

    public static IRepository<TEntity, int> GetRepository1<TEntity>(this IRepoFactory<TEntity, int> factory) where TEntity : class {
      return factory.GetRepositoryInternal();
    }

    public static IRepository<TEntity, long> GetRepository1<TEntity>(this IRepoFactory<TEntity, long> factory) where TEntity : class {
      return factory.GetRepositoryInternal();
    }

    public static IRepository<TEntity, Guid> GetRepository1<TEntity>(this IRepoFactory<TEntity, Guid> factory) where TEntity : class {
      return factory.GetRepositoryInternal();
    }

  }

}
