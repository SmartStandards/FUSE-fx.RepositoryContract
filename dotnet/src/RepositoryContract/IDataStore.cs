
namespace System.Data.Fuse {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public interface IDataStore : ICommitable{

    IRepository<TEntity,TKey> GetRepository<TEntity, TKey>()
      where TEntity : class;

    //TODO: überlegen, ob hier eine auskumftsfunktionzum abrufen des EntitySchemas sinn macht

  }

}
