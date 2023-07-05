using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Data.Fuse.Logic;

namespace System.Data.Fuse {

  public class EfGenericRepository : IGenericRepository {

    private Dictionary<string, EfRepository> _InnerRepos = new Dictionary<string, EfRepository>();

    protected EfRepository GetInnerRepo(string entityName) {
      if (!_InnerRepos.ContainsKey(entityName)) {
        _InnerRepos.Add(entityName, CreateInnerRepo(entityName));
      }
      return _InnerRepos[entityName];
    }

    protected readonly DbContext _DbContext;
    protected readonly Assembly _Assembly;

    public EfGenericRepository(DbContext dbContext, Assembly assembly) {
      this._DbContext = dbContext;
      this._Assembly = assembly;
    }

    public object AddOrUpdateEntity(string entityName, Dictionary<string, JsonElement> entity) {
      return GetInnerRepo(entityName).AddOrUpdate1(entity);
    }

    public void DeleteEntities(object[][] entityIdsToDelete) {
      throw new NotImplementedException();
    }

    public IList<Dictionary<string, object>> GetDtos(string entityName, SimpleExpressionTree filter) {
      return GetInnerRepo(entityName).GetDtos1(filter);
    }

    public Collections.IList GetEntities(string entityName, SimpleExpressionTree filter) {     
      return GetInnerRepo(entityName).GetEntities1(filter);
    }

    public IList<EntityRefById> GetEntityRefs(string entityName) {
      throw new NotImplementedException();
    }

    private EfRepository CreateInnerRepo(string entityName) {
      Type[] allTypes = _Assembly.GetTypes();
      Type entityType = allTypes.Where((Type t) => t.Name == entityName).FirstOrDefault();
      if (entityType == null) { return null; }
      Type repoType = typeof(EfRepository1<>);
      repoType = repoType.MakeGenericType(entityType);
      return (EfRepository)Activator.CreateInstance(repoType, _DbContext);
    }

  }
}
