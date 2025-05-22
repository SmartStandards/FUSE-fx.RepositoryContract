using System;
using System.Collections.Generic;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.ModelDescription;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace System.Convenience {
  public class ModelVsEntityDataStore : IDataStore {

    IDataStore _InnerStore;
    private readonly Tuple<Type, Type>[] _ManagedTypes;
    private readonly SchemaRoot _SchemaRoot;

    public ModelVsEntityDataStore(IDataStore innerStore, Tuple<Type, Type>[] managedTypes) {
      _InnerStore = innerStore;
      _ManagedTypes = managedTypes;
      _SchemaRoot = ModelReader.GetSchema(managedTypes.Select((mt) => mt.Item1).ToArray(), true);
    }

    public void BeginTransaction() {
      _InnerStore.BeginTransaction();
    }

    public void CommitTransaction() {
      _InnerStore.CommitTransaction();
    }

    public IRepository<TModel, TKey> GetRepository<TModel, TKey>()
      where TModel : class {
      // Get all managed types from the inner store
      Tuple<Type, Type>[] managedTypes = _InnerStore.GetManagedTypes();

      // Find the entity type that matches the TModel name or TModel + "Entity"
      var entityType = Array.Find(
        managedTypes, (type) =>
          type.Item1.Name == typeof(TModel).Name ||
          type.Item1.Name == $"{typeof(TModel).Name}Entity"
      );

      if (entityType == null) {
        throw new InvalidOperationException($"No matching entity type found for model {typeof(TModel).Name}");
      }

      var getRepositoryMethod = typeof(ModelVsEntityDataStore)
        .GetMethod(nameof(GetRepositoryInternal))
        .MakeGenericMethod(typeof(TModel), entityType.Item1, typeof(TKey));

      return getRepositoryMethod.Invoke(this, null) as IRepository<TModel, TKey>;
    }

    public IRepository<TModel, TKey> GetRepositoryInternal<TModel, TEntity, TKey>()
      where TEntity : class
      where TModel : class {
      IRepository<TEntity, TKey> innerRepository = _InnerStore.GetRepository<TEntity, TKey>();
      ModelVsEntityParams<TModel, TEntity> modelVsEntityParams = new ModelVsEntityParams<TModel, TEntity>();

      Func<string, object[], EntityRef[]> getEntityRefsByKey = (string entityName, object[] keys) => {
        return ConversionHelper.GetEntityRefs(entityName, keys, typeof(TEntity), _InnerStore, _InnerStore.GetSchemaRoot());
      };

      Func<Type, object[], object[]> getModelsByKey = (Type modelType, object[] keys) => {
        return ConversionHelper.GetEntities(modelType, keys, this, this.GetSchemaRoot());
      };

      Func<string, ExpressionTree, EntityRef[]> getEntityRefsBySearchExpression = (string entityName, ExpressionTree searchExpression) => {
        return ConversionHelper.GetEntityRefsBySearchExpression(entityName, searchExpression, typeof(TEntity), _InnerStore, _InnerStore.GetSchemaRoot());
      };

      Func<Type, ExpressionTree, object[]> getModelsBySearchExpression = (Type modelType, ExpressionTree searchExpression) => {
        return ConversionHelper.GetEntitiesBySearchExpression(modelType, searchExpression, this, this.GetSchemaRoot());
      };

      return new ModelVsEntityRepository<TModel, TEntity, TKey>(
        innerRepository,
        (x, y) => { }, (x, y) => { },
        ConversionHelper.ResolveNavigations<TEntity>(_InnerStore.GetSchemaRoot()),
        ConversionHelper.LoadNavigations<TEntity, TModel>(
          _InnerStore.GetSchemaRoot(),
          getEntityRefsByKey,
          getModelsByKey,
          getEntityRefsBySearchExpression,
          getModelsBySearchExpression,
          NavigationRole.Dependent | NavigationRole.Lookup,
          true
        )
      );

      //return new ModelVsEntityRepository<TModel, TEntity, TKey>(innerRepository, modelVsEntityParams);
    }

    public SchemaRoot GetSchemaRoot() {
      return _SchemaRoot;
    }

    public void RollbackTransaction() {
      _InnerStore.RollbackTransaction();
    }

    public Tuple<Type, Type>[] GetManagedTypes() {
      return _ManagedTypes;
    }
  }
}
