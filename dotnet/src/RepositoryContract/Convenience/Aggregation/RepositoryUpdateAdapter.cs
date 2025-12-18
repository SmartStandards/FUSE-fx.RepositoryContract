using System.Collections.Generic;

namespace System.Data.Fuse.Convenience {

  /// <summary>
  /// Non-generic adapter interface for executing dictionary-based updates
  /// on repositories with unknown entity/key types.
  /// </summary>
  public interface IRepositoryUpdateAdapter {

    /// <summary>
    /// Updates or inserts an entity using field dictionary semantics.
    /// </summary>
    Dictionary<string, object> AddOrUpdateEntityFields(
      Dictionary<string, object> fields
    );

  }

  /// <summary>
  /// Adapter wrapping a strongly typed IRepository to a non-generic update interface.
  /// </summary>
  public sealed class RepositoryUpdateAdapter<TEntity, TKey>
    : IRepositoryUpdateAdapter
    where TEntity : class {

    private readonly IRepository<TEntity, TKey> _Repository;

    public RepositoryUpdateAdapter(IRepository<TEntity, TKey> repository) {
      if (repository == null) {
        throw new ArgumentNullException(nameof(repository));
      }
      this._Repository = repository;
    }

    public Dictionary<string, object> AddOrUpdateEntityFields(
      Dictionary<string, object> fields) {

      return this._Repository.AddOrUpdateEntityFields(fields);
    }
  }

}
