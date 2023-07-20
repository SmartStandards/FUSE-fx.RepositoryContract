using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Data.Fuse.Logic;

namespace System.Data.Fuse {

  public class EfRepository : IRepository {

    private Dictionary<string, EfRepositoryBase> _InnerRepos = new Dictionary<string, EfRepositoryBase>();

    protected EfRepositoryBase GetInnerRepo(string entityName) {
      if (!_InnerRepos.ContainsKey(entityName)) {
        _InnerRepos.Add(entityName, CreateInnerRepo(entityName));
      }
      return _InnerRepos[entityName];
    }

    protected readonly DbContext _DbContext;
    protected readonly Assembly _Assembly;

    public EfRepository(DbContext dbContext, Assembly assembly) {
      this._DbContext = dbContext;
      this._Assembly = assembly;
    }

    public object AddOrUpdateEntity(string entityName, Dictionary<string, JsonElement> entity) {
      return GetInnerRepo(entityName).AddOrUpdate1(entity);
    }

    public void DeleteEntities(string entityName, JsonElement[][] entityIdsToDelete) {
      GetInnerRepo(entityName).DeleteEntities1(entityIdsToDelete);
    }

    public IList<Dictionary<string, object>> GetBusinessModels(string entityName, SimpleExpressionTree filter) {
      return GetInnerRepo(entityName).GetBusinessModels1(filter);
    }

    public IList<Dictionary<string, object>> GetBusinessModels(string entityName, string dynamicLinqFilter) {
      return GetInnerRepo(entityName).GetBusinessModels1(dynamicLinqFilter);
    }

    public Collections.IList GetDbEntities(string entityName, SimpleExpressionTree filter) {     
      return GetInnerRepo(entityName).GetEntities1(filter);
    }

    public Collections.IList GetDbEntities(string entityName, string dynamicLinqFilter) {
      return GetInnerRepo(entityName).GetEntities1(dynamicLinqFilter);
    }

    public IList<EntityRefById> GetEntityRefs(string entityName) {
      throw new NotImplementedException();
    }

    private EfRepositoryBase CreateInnerRepo(string entityName) {
      Type[] allTypes = _Assembly.GetTypes();
      Type entityType = allTypes.Where((Type t) => t.Name == entityName).FirstOrDefault();
      if (entityType == null) { return null; }
      Type repoType = typeof(EfRepository1<>);
      repoType = repoType.MakeGenericType(entityType);
      return (EfRepositoryBase)Activator.CreateInstance(repoType, _DbContext);
    }

  }

}
