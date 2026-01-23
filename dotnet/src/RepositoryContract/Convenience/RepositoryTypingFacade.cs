using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.Fuse.Convenience.Internal;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Fuse.Convenience {

  public static class RepositoryTypingExtensions {

    public static IRepository<TEntity, TKey> GetTypedRepository<TEntity, TKey>(this IUniversalRepository ur) where TEntity : class {

      if (ur is UniversalRepositoryBase) {
        //HACK: mit geheimwissen 'herausstibizen' des inneren Repositories, anstatt 2 
        //sich gegenseitig auflösende wrapper ineinander zu packen...
        if (((UniversalRepositoryBase)ur).TryGetInnerRepo<TEntity, TKey>(out IRepository<TEntity, TKey> repo)) {
          return repo;
        }
      }

      return new RepositoryTypingFacade<TEntity, TKey>(ur);
    }

    public class RepositoryTypingFacade<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class {

      private readonly IUniversalRepository _InnerRepository;
      private readonly string _EntityName = typeof(TEntity).Name;

      public RepositoryTypingFacade(IUniversalRepository innerUniversalRepository) {
        _InnerRepository = innerUniversalRepository;
      }

      public TEntity AddOrUpdateEntity(TEntity entity) {
        return (TEntity)_InnerRepository.AddOrUpdateEntity(_EntityName, entity);
      }

      public Dictionary<string, object> AddOrUpdateEntityFields(Dictionary<string, object> fields) {
        return _InnerRepository.AddOrUpdateEntityFields(_EntityName, fields);
      }

      public bool ContainsKey(TKey key) {
        return _InnerRepository.ContainsKey(_EntityName, key);
      }

      public int Count(ExpressionTree filter) {
        return _InnerRepository.Count(_EntityName, filter);
      }

      public int CountAll() {
        return _InnerRepository.CountAll(_EntityName);
      }

      public int CountBySearchExpression(string searchExpression) {
        return _InnerRepository.CountBySearchExpression(_EntityName, searchExpression);
      }

      public RepositoryCapabilities GetCapabilities() {
        return _InnerRepository.GetCapabilities();
      }

      public TEntity[] GetEntities(ExpressionTree filter, string[] sortedBy, int limit = 500, int skip = 0) {
        return _InnerRepository.GetEntities(_EntityName, filter, sortedBy, limit, skip).OfType<TEntity>().ToArray();
      }

      public TEntity[] GetEntitiesByKey(TKey[] keysToLoad) {
        return _InnerRepository.GetEntitiesByKey(_EntityName, keysToLoad?.Cast<object>().ToArray() ?? Array.Empty<object>()).OfType<TEntity>().ToArray();
      }

      public TEntity[] GetEntitiesBySearchExpression(string searchExpression, string[] sortedBy, int limit = 500, int skip = 0) {
        return _InnerRepository.GetEntitiesBySearchExpression(_EntityName, searchExpression, sortedBy, limit, skip).OfType<TEntity>().ToArray();
      }

      public Dictionary<string, object>[] GetEntityFields(ExpressionTree filter, string[] includedFieldNames, string[] sortedBy, int limit = 500, int skip = 0) {
        return _InnerRepository.GetEntityFields(_EntityName, filter, includedFieldNames, sortedBy, limit, skip);
      }

      public Dictionary<string, object>[] GetEntityFieldsByKey(TKey[] keysToLoad, string[] includedFieldNames) {
        return _InnerRepository.GetEntityFieldsByKey(_EntityName, keysToLoad?.Cast<object>().ToArray() ?? Array.Empty<object>(), includedFieldNames);
      }

      public Dictionary<string, object>[] GetEntityFieldsBySearchExpression(string searchExpression, string[] includedFieldNames, string[] sortedBy, int limit = 500, int skip = 0) {
        return _InnerRepository.GetEntityFieldsBySearchExpression(_EntityName, searchExpression, includedFieldNames, sortedBy, limit, skip);
      }

      public EntityRef<TKey>[] GetEntityRefs(ExpressionTree filter, string[] sortedBy, int limit = 500, int skip = 0) {
        return _InnerRepository.GetEntityRefs(_EntityName, filter, sortedBy, limit, skip).OfType<EntityRef<TKey>>().ToArray();
      }

      public EntityRef<TKey>[] GetEntityRefsByKey(TKey[] keysToLoad) {
        return _InnerRepository.GetEntityRefsByKey(_EntityName, keysToLoad?.Cast<object>().ToArray() ?? Array.Empty<object>()).OfType<EntityRef<TKey>>().ToArray();
      }

      public EntityRef<TKey>[] GetEntityRefsBySearchExpression(string searchExpression, string[] sortedBy, int limit = 500, int skip = 0) {
        return _InnerRepository.GetEntityRefsBySearchExpression(_EntityName, searchExpression, sortedBy, limit, skip).OfType<EntityRef<TKey>>().ToArray();
      }

      public string GetOriginIdentity() {
        return _InnerRepository.GetOriginIdentity();
      }

      public TKey[] Massupdate(ExpressionTree filter, Dictionary<string, object> fields) {
        return _InnerRepository.Massupdate(_EntityName, filter, fields)?.Cast<TKey>().ToArray() ?? Array.Empty<TKey>();
      }

      public TKey[] MassupdateByKeys(TKey[] keysToUpdate, Dictionary<string, object> fields) {
        return _InnerRepository.MassupdateByKeys(_EntityName, keysToUpdate?.Cast<object>().ToArray() ?? Array.Empty<object>(), fields)?.Cast<TKey>().ToArray() ?? Array.Empty<TKey>();
      }

      public TKey[] MassupdateBySearchExpression(string searchExpression, Dictionary<string, object> fields) {
        return _InnerRepository.MassupdateBySearchExpression(_EntityName, searchExpression, fields)?.Cast<TKey>().ToArray() ?? Array.Empty<TKey>();
      }

      public TKey TryAddEntity(TEntity entity) {
        return (TKey)_InnerRepository.TryAddEntity(_EntityName, entity);
      }

      public TKey[] TryDeleteEntities(TKey[] keysToDelete) {
        return _InnerRepository.TryDeleteEntities(_EntityName, keysToDelete?.Cast<object>().ToArray() ?? Array.Empty<object>())?.Cast<TKey>().ToArray() ?? Array.Empty<TKey>();
      }

      public TEntity TryUpdateEntity(TEntity entity) {
        return (TEntity)_InnerRepository.TryUpdateEntity(_EntityName, entity);
      }

      public Dictionary<string, object> TryUpdateEntityFields(Dictionary<string, object> fields) {
        return _InnerRepository.TryUpdateEntityFields(_EntityName, fields);
      }

      public bool TryUpdateKey(TKey currentKey, TKey newKey) {
        return _InnerRepository.TryUpdateKey(_EntityName, currentKey, newKey);
      }
    }

  }

}
