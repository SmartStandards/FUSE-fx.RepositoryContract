using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;

namespace System.Data.Fuse.Convenience {

  public abstract class UniversalRepositoryFacade {

    public abstract Dictionary<string, object> AddOrUpdateEntity(Dictionary<string, object> entity);

    public abstract void DeleteEntities(object[][] entityIdsToDelete);

    public abstract IList<Dictionary<string, object>> GetEntities(
      LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    );

    public abstract IList<Dictionary<string, object>> GetEntities(
      string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    );

    public abstract IList<EntityRef> GetEntityRefs(
      LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    );

    public abstract IList<EntityRef> GetEntityRefs(
      string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    );

    public abstract int GetCount(LogicalExpression filter);

    public abstract int GetCount(string dynamicLinqFilter);

  }

  public class DynamicRepositoryFacade<T>  
    : UniversalRepositoryFacade
    where T : class{

    //private IRepository<T> _InternalRepo;
    private KvpModelVsEntityRepository<T> _InternalRepo;

    public DynamicRepositoryFacade(
      IRepository<T> internalRepo,
      Func<PropertyInfo, bool> isForeignKey,
      Func<PropertyInfo, bool> isNavigation
    ) {
      _InternalRepo = new KvpModelVsEntityRepository<T>( internalRepo, isForeignKey, isNavigation);
    }

    public override Dictionary<string, object> AddOrUpdateEntity(Dictionary<string, object> entity) {
      return _InternalRepo.AddOrUpdateEntity(entity);
    }

    public override void DeleteEntities(object[][] entityIdsToDelete) {
      _InternalRepo.DeleteEntities(entityIdsToDelete);
    }

    public override IList<Dictionary<string, object>> GetEntities(
      LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return _InternalRepo.GetEntities(filter, pagingParams, sortingParams);
    }

    public override IList<Dictionary<string, object>> GetEntities(
      string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return _InternalRepo.GetEntities(dynamicLinqFilter, pagingParams, sortingParams);
    }

    public override IList<EntityRef> GetEntityRefs(
      LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    ) {      
      return _InternalRepo.GetEntityRefs(filter, pagingParams, sortingParams).ToList();
    }

    public override IList<EntityRef> GetEntityRefs(
      string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return _InternalRepo.GetEntityRefs(dynamicLinqFilter, pagingParams, sortingParams).ToList();
    }

    public override int GetCount(LogicalExpression filter) {
      return _InternalRepo.GetCount(filter);
    }

    public override int GetCount(string dynamicLinqFilter) {
      return _InternalRepo.GetCount(dynamicLinqFilter);
    }

  }

}