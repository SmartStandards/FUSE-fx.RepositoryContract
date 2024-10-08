using System.Data.ModelDescription;

namespace System.Data.Fuse {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public interface IDataStore : ICommitable, ISchemaProvider {

    IRepository<TEntity,TKey> GetRepository<TEntity, TKey>()
      where TEntity : class;

  }

}
