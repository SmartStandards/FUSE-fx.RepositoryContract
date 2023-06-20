//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Data.UDAS.v1.Models;
//using System.Reflection;

//TODO: rename namespace of the final versions to "System.Data.Fuse"
//namespace ModelReader.Persistence {
//  public class GenericEntityRepositoryEf : IGenericEntityRepository {
//    private readonly DbContext _DbContext;

//    public GenericEntityRepositoryEf(DbContext dbContext) {
//      this._DbContext = dbContext;
//    }

//    public object AddOrUpdateEntity(object entity) {
//      throw new System.NotImplementedException();
//    }

//    public void DeleteEntities(object[][] entityIdsToDelete) {
//      throw new System.NotImplementedException();
//    }

//    public IList<object> GetEntities(string entityName) {
//      throw new System.NotImplementedException();
//    }

//    public IList<EntityRef> GetEntityRefs(string entityName) {
//      Type entityType = Assembly.GetExecutingAssembly().GetType(entityName);
//      throw new NotImplementedException();
//    }
//  }
//}
