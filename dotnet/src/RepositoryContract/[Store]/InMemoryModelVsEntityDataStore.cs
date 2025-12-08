using System.Collections.Generic;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Reflection;

namespace System.Data.Fuse.Convenience {

  /// <summary>
  /// (from 'FUSE-fx.RepositoryContract')
  /// </summary>
  public class InMemoryModelVsEntityDataStore {

    private SchemaRoot _SchemaRoot;

    private readonly Action<object, object> _OnAfterModelToEntity;
    private readonly Action<object, object> _OnAfterEntityToModel;

    private readonly Func<PropertyInfo, Dictionary<string, object>, object, bool> _HandlePropertyModelToEntity;
    private readonly Func<object, object, PropertyInfo, bool> _HandlePropertyEntityToModel;

    public InMemoryModelVsEntityDataStore(
      SchemaRoot schemaRoot,
      Action<object, object> onAfterModelToEntity,
      Action<object, object> onAfterEntityToModel,
      Func<PropertyInfo, Dictionary<string, object>, object, bool> handlePropertyModelToEntity,
      Func<object, object, PropertyInfo, bool> handlePropertyEntityToModel) {
      _SchemaRoot = schemaRoot;
      _OnAfterModelToEntity = onAfterModelToEntity;
      _OnAfterEntityToModel = onAfterEntityToModel;
      _HandlePropertyModelToEntity = handlePropertyModelToEntity;
      _HandlePropertyEntityToModel = handlePropertyEntityToModel;
    }

    public IRepository<TModel, TKey> GetRepository<TModel, TEntity, TKey>()
      where TEntity : class
      where TModel : class {
      return new ModelVsEntityRepository<TModel, TEntity, TKey>(
        new InMemoryRepository<TEntity, TKey>(_SchemaRoot),
        (m, e) => _OnAfterModelToEntity(m, e),
        (e, m) => _OnAfterEntityToModel(e, m),
        _HandlePropertyModelToEntity,
        _HandlePropertyEntityToModel
      );
    }

  }

}
