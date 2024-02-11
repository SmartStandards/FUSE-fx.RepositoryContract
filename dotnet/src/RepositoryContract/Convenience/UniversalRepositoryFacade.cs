using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Linq;

namespace System.Data.Fuse.Convenience {

  public abstract class UniversalRepositoryFacade {

    public abstract object AddOrUpdateEntity(Dictionary<string, object> entity);

    public abstract void DeleteEntities(object[][] entityIdsToDelete);

    public abstract IList GetEntities(
      LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    );

    public abstract IList GetEntities(
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

    private IRepository<T> _InternalRepo;

    public DynamicRepositoryFacade(IRepository<T> internalRepo) {
      _InternalRepo = internalRepo;
    }

    public override object AddOrUpdateEntity(Dictionary<string, object> entity) {
      return _InternalRepo.AddOrUpdateEntity(entity.Deserialize<T>());
    }

    public override void DeleteEntities(object[][] entityIdsToDelete) {
      _InternalRepo.DeleteEntities(entityIdsToDelete);
    }

    public override IList GetEntities(
      LogicalExpression filter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return _InternalRepo.GetEntities(filter, pagingParams, sortingParams).ToList();
    }

    public override IList GetEntities(
      string dynamicLinqFilter, PagingParams pagingParams, SortingField[] sortingParams
    ) {
      return _InternalRepo.GetEntities(dynamicLinqFilter, pagingParams, sortingParams).ToList();
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