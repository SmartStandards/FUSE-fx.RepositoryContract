using System.Collections.Generic;
using System.Data.ModelDescription;
using System.Data.ModelDescription.Convenience;
using System.Linq;
using System.Reflection;

namespace System.Data.Fuse.Convenience {

  public static class DataStoreExtensions {

    public static TEntity[] GetEntities<TEntity, TKey>(
      this IDataStore dataStore, ExpressionTree filter, string[] sortedBy = null, int limit = 500, int skip = 0
    )
      where TEntity : class {
      return dataStore.GetRepository<TEntity, TKey>().GetEntities(
        filter, sortedBy ?? Array.Empty<string>(), limit, skip
      );
    }

    public static EntityRef<TKey>[] GetEntityRefs<TEntity, TKey>(
      this IDataStore dataStore, ExpressionTree filter, string[] sortedBy = null, int limit = 500, int skip = 0
    )
      where TEntity : class {
      return dataStore.GetRepository<TEntity, TKey>().GetEntityRefs(
        filter, sortedBy ?? Array.Empty<string>(), limit, skip
      );
    }

    public static EntityRef<TKey>[] GetEntityRefsBySearchExpression<TEntity, TKey>(
      this IDataStore dataStore, string filter, string[] sortedBy = null, int limit = 500, int skip = 0
    )
      where TEntity : class {
      return dataStore.GetRepository<TEntity, TKey>().GetEntityRefsBySearchExpression(
        filter, sortedBy ?? Array.Empty<string>(), limit, skip
      );
    }

    public static TEntity[] GetEntitiesByKey<TEntity, TKey>(
      this IDataStore dataStore, TKey[] keys
    )
      where TEntity : class {
      return dataStore.GetRepository<TEntity, TKey>().GetEntitiesByKey(
        keys
      );
    }

    public static EntityRef<TKey>[] GetEntityRefsByKey<TEntity, TKey>(
      this IDataStore dataStore, TKey[] keys
    )
      where TEntity : class {
      return dataStore.GetRepository<TEntity, TKey>().GetEntityRefsByKey(
        keys
      );
    }

    public static TEntity[] GetEntitiesBySearchExpression<TEntity, TKey>(
      this IDataStore dataStore, string searchExpression, string[] sortedBy = null, int limit = 500, int skip = 0
    )
      where TEntity : class {
      return dataStore.GetRepository<TEntity, TKey>().GetEntitiesBySearchExpression(
        searchExpression, sortedBy ?? Array.Empty<string>(), limit, skip
      );
    }

    public static TEntity AddOrUpdate<TEntity, TKey>(
      this IDataStore dataStore, TEntity entity
    )
      where TEntity : class {
      return dataStore.GetRepository<TEntity, TKey>().AddOrUpdateEntity(entity);
    }

    public static TKey[] TryDeleteEntities<TEntity, TKey>(
      this IDataStore dataStore, params TKey[] keysToDelete
    )
    where TEntity : class {
      return dataStore.GetRepository<TEntity, TKey>().TryDeleteEntities(keysToDelete);
    }

    /// <summary>
    /// Only works when Primary Key is int
    /// </summary>
    /// <typeparam name="TPrimaryEntity"></typeparam>
    /// <typeparam name="TForeignEntity"></typeparam>
    /// <param name="foreignEntity"></param>
    /// <param name="dataStore"></param>
    /// <param name="foreignKeyIndexName"></param>
    /// <returns></returns>
    public static TPrimaryEntity GetPrimary1<TPrimaryEntity, TForeignEntity>(
      this TForeignEntity foreignEntity, IDataStore dataStore, string foreignKeyIndexName = null
    ) where TPrimaryEntity : class {
      return foreignEntity.GetPrimary<TPrimaryEntity, int, TForeignEntity>(dataStore, foreignKeyIndexName);
    }

    /// <summary>
    /// Only works when Primary Key is long
    /// </summary>
    /// <typeparam name="TPrimaryEntity"></typeparam>
    /// <typeparam name="TForeignEntity"></typeparam>
    /// <param name="foreignEntity"></param>
    /// <param name="dataStore"></param>
    /// <param name="foreignKeyIndexName"></param>
    /// <returns></returns>
    public static TPrimaryEntity GetPrimary2<TPrimaryEntity, TForeignEntity>(
      this TForeignEntity foreignEntity, IDataStore dataStore, string foreignKeyIndexName = null
    ) where TPrimaryEntity : class {
      return foreignEntity.GetPrimary<TPrimaryEntity, long, TForeignEntity>(dataStore, foreignKeyIndexName);
    }

    /// <summary>
    /// Only works when Primary Key is int
    /// </summary>
    /// <typeparam name="TPrimaryEntity"></typeparam>
    /// <typeparam name="TForeignEntity"></typeparam>
    /// <param name="foreignEntity"></param>
    /// <param name="dataStore"></param>
    /// <param name="foreignKeyIndexName"></param>
    /// <returns></returns>
    public static TPrimaryEntity GetPrimary3<TPrimaryEntity, TForeignEntity>(
      this TForeignEntity foreignEntity, IDataStore dataStore, string foreignKeyIndexName = null
    ) where TPrimaryEntity : class {
      return foreignEntity.GetPrimary<TPrimaryEntity, Guid, TForeignEntity>(dataStore, foreignKeyIndexName);
    }

    /// <summary>
    /// Only works when Primary Key is string
    /// </summary>
    /// <typeparam name="TPrimaryEntity"></typeparam>
    /// <typeparam name="TForeignEntity"></typeparam>
    /// <param name="foreignEntity"></param>
    /// <param name="dataStore"></param>
    /// <param name="foreignKeyIndexName"></param>
    /// <returns></returns>
    public static TPrimaryEntity GetPrimary4<TPrimaryEntity, TForeignEntity>(
      this TForeignEntity foreignEntity, IDataStore dataStore, string foreignKeyIndexName = null
    ) where TPrimaryEntity : class {
      return foreignEntity.GetPrimary<TPrimaryEntity, string, TForeignEntity>(dataStore, foreignKeyIndexName);
    }

    public static TPrimaryEntity GetPrimary<TPrimaryEntity, TPrimaryEntityKey, TForeignEntity>(
      this TForeignEntity foreignEntity, IDataStore dataStore, string foreignKeyIndexName = null
    ) where TPrimaryEntity : class {

      if (foreignEntity == null) {
        return null;
      }

      IRepository<TPrimaryEntity, TPrimaryEntityKey> primaryEntityRepo = dataStore.GetRepository<TPrimaryEntity, TPrimaryEntityKey>();
      SchemaRoot schemaRoot = dataStore.GetSchemaRoot();

      RelationSchema relation = schemaRoot.FindRelation<TForeignEntity, TPrimaryEntity>(foreignKeyIndexName);

      if (relation == null) {
        return null;
      }

      EntitySchema foreignEntitySchema = schemaRoot.GetSchema(relation.ForeignEntityName);
      IndexSchema foreignKeyIndex = foreignEntitySchema.GetIndex(relation.ForeignKeyIndexName);
      List<PropertyInfo> foreignKeyPropertyInfos = foreignKeyIndex.GetProperties(typeof(TForeignEntity));

      IndexSchema primaryKeyIndex = schemaRoot.GetPrimaryIndex(typeof(TPrimaryEntity));
      List<PropertyInfo> primaryKeyIndexProperties = primaryKeyIndex.GetProperties(typeof(TPrimaryEntity));

      object[] foreignKeyValues = foreignEntity.GetValues(foreignKeyPropertyInfos);
      ExpressionTree filter = QueryExtensions.GetExpressionTreeByValues(
        foreignKeyValues, primaryKeyIndexProperties.ToArray()
      );

      TPrimaryEntity primaryEntity = primaryEntityRepo.GetEntities(
        filter, new string[] { }, 1, 0
      ).FirstOrDefault();

      return primaryEntity;
    }

    /// <summary>
    /// Only works when Primary Key is int
    /// </summary>
    /// <typeparam name="TPrimaryEntity"></typeparam>
    /// <typeparam name="TForeignEntity"></typeparam>
    /// <param name="primaryEntity"></param>
    /// <param name="dataStore"></param>
    /// <param name="foreignKeyIndexName"></param>
    /// <returns></returns>
    public static TForeignEntity[] GetForeign1<TPrimaryEntity, TForeignEntity>(
      this TPrimaryEntity primaryEntity, IDataStore dataStore, string foreignKeyIndexName = null
    ) where TForeignEntity : class {
      return primaryEntity.GetForeign<TPrimaryEntity, TForeignEntity, int>(dataStore, foreignKeyIndexName);
    }

    /// <summary>
    /// Only works when Primary Key is long
    /// </summary>
    /// <typeparam name="TPrimaryEntity"></typeparam>
    /// <typeparam name="TForeignEntity"></typeparam>
    /// <param name="primaryEntity"></param>
    /// <param name="dataStore"></param>
    /// <param name="foreignKeyIndexName"></param>
    /// <returns></returns>
    public static TForeignEntity[] GetForeign2<TPrimaryEntity, TForeignEntity>(
      this TPrimaryEntity primaryEntity, IDataStore dataStore, string foreignKeyIndexName = null
    ) where TForeignEntity : class {
      return primaryEntity.GetForeign<TPrimaryEntity, TForeignEntity, long>(dataStore, foreignKeyIndexName);
    }

    /// <summary>
    /// Only works when Primary Key is Guid
    /// </summary>
    /// <typeparam name="TPrimaryEntity"></typeparam>
    /// <typeparam name="TForeignEntity"></typeparam>
    /// <param name="primaryEntity"></param>
    /// <param name="dataStore"></param>
    /// <param name="foreignKeyIndexName"></param>
    /// <returns></returns>
    public static TForeignEntity[] GetForeign3<TPrimaryEntity, TForeignEntity>(
      this TPrimaryEntity primaryEntity, IDataStore dataStore, string foreignKeyIndexName = null
    ) where TForeignEntity : class {
      return primaryEntity.GetForeign<TPrimaryEntity, TForeignEntity, Guid>(dataStore, foreignKeyIndexName);
    }

    /// <summary>
    /// Only works when Primary Key is string
    /// </summary>
    /// <typeparam name="TPrimaryEntity"></typeparam>
    /// <typeparam name="TForeignEntity"></typeparam>
    /// <param name="primaryEntity"></param>
    /// <param name="dataStore"></param>
    /// <param name="foreignKeyIndexName"></param>
    /// <returns></returns>
    public static TForeignEntity[] GetForeign4<TPrimaryEntity, TForeignEntity>(
      this TPrimaryEntity primaryEntity, IDataStore dataStore, string foreignKeyIndexName = null
    ) where TForeignEntity : class {
      return primaryEntity.GetForeign<TPrimaryEntity, TForeignEntity, string>(dataStore, foreignKeyIndexName);
    }

    public static TForeignEntity[] GetForeign<TPrimaryEntity, TForeignEntity, TForeignEntityKey>(
      this TPrimaryEntity primaryEntity, IDataStore dataStore, string foreignKeyIndexName = null
    ) where TForeignEntity : class {

      if (primaryEntity == null) {
        return Array.Empty<TForeignEntity>();
      }

      IRepository<TForeignEntity, TForeignEntityKey> foreignEntityRepo = dataStore.GetRepository<TForeignEntity, TForeignEntityKey>();
      SchemaRoot schemaRoot = dataStore.GetSchemaRoot();

      RelationSchema relation = schemaRoot.FindRelation<TForeignEntity, TPrimaryEntity>(foreignKeyIndexName);

      if (relation == null) {
        return Array.Empty<TForeignEntity>();
      }

      EntitySchema foreignEntitySchema = schemaRoot.GetSchema(relation.ForeignEntityName);
      IndexSchema foreignKeyIndex = foreignEntitySchema.GetIndex(relation.ForeignKeyIndexName);
      List<PropertyInfo> foreignKeyPropertyInfos = foreignKeyIndex.GetProperties(typeof(TForeignEntity));

      IndexSchema primaryKeyIndex = schemaRoot.GetPrimaryIndex(typeof(TPrimaryEntity));
      List<PropertyInfo> primaryKeyIndexProperties = primaryKeyIndex.GetProperties(typeof(TPrimaryEntity));

      object[] primaryKeyValues = primaryEntity.GetValues(primaryKeyIndexProperties);
      ExpressionTree filter = QueryExtensions.GetExpressionTreeByValues(
        primaryKeyValues, foreignKeyPropertyInfos.ToArray()
      );

      TForeignEntity[] foreignEntities = foreignEntityRepo.GetEntities(
        filter, new string[] { }, 0, 0
      );

      return foreignEntities;
    }

    public static RelationSchema FindRelation<TForeignEntity, TPrimaryEntity>(
      this SchemaRoot schemaRoot, string foreignKeyIndexName = null
    ) {
      string foreignEntityName = typeof(TForeignEntity).Name;
      string primaryEntityName = typeof(TPrimaryEntity).Name;

      // Find the relation where the foreign entity points to the primary entity
      if (string.IsNullOrEmpty(foreignKeyIndexName)) {
        return schemaRoot.Relations.FirstOrDefault(r =>
          r.ForeignEntityName == foreignEntityName &&
          r.PrimaryEntityName == primaryEntityName
        );
      }
      return schemaRoot.Relations.FirstOrDefault(r =>
        r.ForeignEntityName == foreignEntityName &&
        r.PrimaryEntityName == primaryEntityName &&
        r.ForeignKeyIndexName == foreignKeyIndexName
      );
    }

  }

}