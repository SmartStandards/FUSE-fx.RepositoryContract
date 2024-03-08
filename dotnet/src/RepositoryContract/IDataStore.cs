namespace System.Data.Fuse {
  public interface IDataStore : ICommitable{

    IRepository<TEntity,TKey> GetRepository<TEntity, TKey>()
      where TEntity : class;

    //TODO: macht ggf. sinn, dass hier auch eine auskumftsfunktion zum abrufen des EntitySchemas angeboten wird

  }
}
