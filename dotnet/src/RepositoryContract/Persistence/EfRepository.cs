using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse.Logic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Linq.Dynamic.Core;

namespace System.Data.Fuse {

  public abstract class EfRepository {
    protected readonly DbContext context;

    public EfRepository(DbContext dbContext) {
      this.context = dbContext;
    }

    public abstract IList GetEntities1(SimpleExpressionTree filter);
  }

  public class EfRepository1<TEntity> : EfRepository, IRepository<TEntity> where TEntity : class {

    public EfRepository1(DbContext context) : base(context){
    }

    public TEntity AddOrUpdateEntity(Dictionary<string, JsonElement> entity) {
      throw new NotImplementedException();
    }

    public void DeleteEntities(object[][] entityIdsToDelete) {
      throw new NotImplementedException();
    }

    public IList<Dictionary<string, object>> GetDtos() {
      throw new NotImplementedException();
    } 

    public override IList GetEntities1(SimpleExpressionTree filter) {
      return GetEntitiesDynamic(filter.CompileToDynamicLinq()).ToList();
    }

    public IList<EntityRefById> GetEntityRefs() {
      throw new NotImplementedException();
    }

    public IQueryable<TEntity> GetEntitiesDynamic(string dynamicLinqFilter) {
      return context.Set<TEntity>().Where(dynamicLinqFilter);
    }

    public IQueryable<TEntity> GetEntities(Expression<Func<TEntity, bool>> filter) {
      return context.Set<TEntity>().Where(filter);
    }   

  }

}
