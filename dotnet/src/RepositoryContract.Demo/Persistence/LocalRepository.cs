//using System.Collections.Generic;
//using System.Data.Fuse.Logic;
//using System.Linq;
//using System.Text;
//using System.Linq.Dynamic.Core;
//using System.Reflection;
//using System.Text.RegularExpressions;

//namespace System.Data.Fuse.Convenience {

//  public class LocalRepository<T> : IRepository<T> where T : class, IEquatable<T> {

//    private static List<T> _Entities = new List<T>();

//    private PropertyInfo[] _KeyProperties;

//    public LocalRepository() {
//      _KeyProperties = GetKeyProperties();
//    }

//    private void DeleteInternal(object[] keys) {
//      T existing = null;
//      foreach (T entity in _Entities) {
//        bool match = false;
//        for (int i = 0; i < _KeyProperties.Length; i++) {
//          object? keyValue = _KeyProperties[i].GetValue(entity);
//          if (keyValue == null) {
//            if (keys[i] == null) {
//              match = true;
//              break;
//            } else { 
//              continue; 
//            }
//          }
//          if (keyValue.Equals(keys[i])) {
//            match = true;
//            break;
//          }
//        }
//        if (match) {
//          existing = entity;
//          break;
//        }
//      }
//      if (existing != null) {
//        _Entities.Remove(existing);
//      }
//    }

//    protected virtual PropertyInfo[] GetKeyProperties() {
//      return typeof(T).GetProperties().Where((pi) => pi.Name == "Id").ToArray();
//    }

//    public T AddOrUpdateEntity(T entity) {
//      T? existing = _Entities.FirstOrDefault((e) => e.Equals(entity));
//      if (existing == null) {
//        _Entities.Add(entity);
//        return entity;
//      }
//      _Entities.Remove(existing);
//      _Entities.Add(entity);
//      return entity;
//    }

//    public void DeleteEntities(object[][] entityIdsToDelete) {
//      QueryExtensions.DeleteEntities(
//        entityIdsToDelete,
//        () => _KeyProperties,
//        (keys) => DeleteInternal(keys)
//      );
//    }

//    public int GetCount(ExpressionTree filter) {
//      return GetEntities(filter, null, null).Count();
//    }

//    public int GetCount(string dynamicLinqFilter) {
//      return GetEntities(dynamicLinqFilter, null, null).Count();
//    }

//    public IList<T> GetEntities(
//      ExpressionTree filter, PagingParams pagingParams, SortingField[] sortingParams
//    ) {
//      return GetEntities(filter.CompileToDynamicLinq(), pagingParams, sortingParams);
//    }

//    public IList<T> GetEntities(
//      string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
//    ) {
//      IQueryable<T> result = null;
//      if (string.IsNullOrEmpty(dynamicLinqFilter)) {
//        result = _Entities.AsQueryable();
//      } else {
//        result = _Entities.AsQueryable().Where(dynamicLinqFilter);
//      }
//      return ApplyPaging(ApplySorting(result, sortingParams), pagingParams).ToList();
//    }

//    public IList<EntityRef> GetEntityRefs(
//      ExpressionTree filter, PagingParams pagingParams, SortingField[] sortingParams
//    ) {
//      throw new NotImplementedException();
//    }

//    public IList<EntityRef> GetEntityRefs(
//      string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
//    ) {
//      throw new NotImplementedException();
//    }

//    private IQueryable<TEntity> ApplyPaging<TEntity>(
//       IQueryable<TEntity> result, PagingParams pagingParams
//     ) {
//      if (pagingParams == null || pagingParams.PageSize == 0) {
//        return result;
//      }
//      int skip = pagingParams.PageSize * (pagingParams.PageNumber - 1);
//      return result.Skip(skip).Take(pagingParams.PageSize);
//    }

//    private IQueryable<TEntity> ApplySorting<TEntity>(
//      IQueryable<TEntity> result, SortingField[] sortingParams
//    ) {
//      if (sortingParams == null || sortingParams.Count() == 0) {
//        return result;
//      }
//      StringBuilder sorting = new StringBuilder();
//      foreach (SortingField sortingField in sortingParams) {
//        if (sortingField.Descending) {
//          sorting.Append(sortingField.FieldName + " descending,");
//        } else {
//          sorting.Append(sortingField.FieldName + ",");
//        }
//      }
//      sorting.Length -= 1;
//      //HACK: internal usage of System.Linq.Dynamic.Core
//      result = result.OrderBy(sorting.ToString());
//      return result;
//    }
//  }

//}