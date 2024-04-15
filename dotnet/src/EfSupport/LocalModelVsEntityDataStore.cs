using System.Collections.Generic;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Reflection;

namespace System.Data.Fuse.Ef {

  public class LocalModelVsEntityDataStore {

    private SchemaRoot _SchemaRoot;

    private readonly Action<object, object> _OnAfterModelToEntity;
    private readonly Action<object, object> _OnAfterEntityToModel;

    private readonly Func<PropertyInfo, Dictionary<string, object>, object, bool> _HandlePropertyModelToEntity;
    private readonly Func<PropertyInfo, object, Dictionary<string, object>, bool> _HandlePropertyEntityToModel;

    public LocalModelVsEntityDataStore(
      SchemaRoot schemaRoot,
      Action<object, object> onAfterModelToEntity,
      Action<object, object> onAfterEntityToModel,
      Func<PropertyInfo, Dictionary<string, object>, object, bool> handlePropertyModelToEntity,
      Func<PropertyInfo, object, Dictionary<string, object>, bool> handlePropertyEntityToModel) {
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
        new LocalRepository<TEntity, TKey>(_SchemaRoot),
        (m, e) => _OnAfterModelToEntity(m, e),
        (e, m) => _OnAfterEntityToModel(e, m),
        _HandlePropertyModelToEntity,
        _HandlePropertyEntityToModel
      );
    }

  }

}
