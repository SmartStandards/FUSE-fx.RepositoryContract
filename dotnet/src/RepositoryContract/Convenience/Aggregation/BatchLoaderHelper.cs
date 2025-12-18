using System;
using System.Collections.Generic;

namespace System.Data.Fuse.Convenience.Aggregation {

  /// <summary>
  /// Helper methods for efficient batched loading by key.
  /// </summary>
  public static class BatchLoaderHelper {

    /// <summary>
    /// Loads entities by keys in batches to avoid oversized IN-queries or request payloads.
    /// Order is not guaranteed; caller may re-order if needed.
    /// </summary>
    public static TEntity[] LoadByKeysBatched<TEntity, TKey>(
      IRepository<TEntity, TKey> repository,
      TKey[] keys,
      int batchSize
    ) where TEntity : class {

      if (repository == null) {
        throw new ArgumentNullException(nameof(repository));
      }

      if (keys == null) {
        throw new ArgumentNullException(nameof(keys));
      }

      if (batchSize < 1) {
        throw new ArgumentOutOfRangeException(nameof(batchSize));
      }

      if (keys.Length == 0) {
        return new TEntity[0];
      }

      List<TEntity> result = new List<TEntity>();

      int index = 0;
      while (index < keys.Length) {
        int remaining = keys.Length - index;
        int take = remaining;
        if (take > batchSize) {
          take = batchSize;
        }

        TKey[] batch = new TKey[take];
        Array.Copy(keys, index, batch, 0, take);

        TEntity[] loaded = repository.GetEntitiesByKey(batch);
        if (loaded != null && loaded.Length > 0) {
          result.AddRange(loaded);
        }

        index += take;
      }

      return result.ToArray();
    }

  }

}
