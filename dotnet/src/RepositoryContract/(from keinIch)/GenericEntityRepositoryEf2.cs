//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data.Fuse.Logic;
//using System.Linq;
//using System.Reflection;
//using System.Text.Json;

//namespace System.Data.Fuse {

//  public class GenericEntityRepositoryEfOuter {

//    private Dictionary<string, GenericEntityRepositoryEfNonGeneric> _InnerRepos = new Dictionary<string, GenericEntityRepositoryEfNonGeneric>();

//    protected readonly DbContext _DbContext;
//    protected readonly Assembly _Assembly;

//    public GenericEntityRepositoryEfOuter(DbContext dbContext, Assembly assembly) {
//      this._DbContext = dbContext;
//      this._Assembly = assembly;
//    }

//    public IList GetEntities(string entityName, string dynamicLinqStatement) {
//      if (!_InnerRepos.ContainsKey(entityName)) {
//        _InnerRepos.Add(entityName, CreateInnerRepo(entityName));
//      }
//      return _InnerRepos[entityName].GetEntities(dynamicLinqStatement);
//    }

//    private GenericEntityRepositoryEfNonGeneric CreateInnerRepo(string entityName) {
//      Type[] allTypes = _Assembly.GetTypes();
//      Type entityType = allTypes.Where((Type t) => t.Name == entityName).FirstOrDefault();
//      if (entityType == null) { return null; }
//      Type repoType = typeof(GenericEntityRepositoryEf2<>);
//      repoType = repoType.MakeGenericType();
//      return (GenericEntityRepositoryEfNonGeneric)Activator.CreateInstance(repoType);
//    }
//  }

//  public abstract class GenericEntityRepositoryEfNonGeneric {
//    protected readonly DbContext _DbContext;
//    protected readonly Assembly _Assembly;

//    public GenericEntityRepositoryEfNonGeneric(DbContext dbContext, Assembly assembly) {
//      this._DbContext = dbContext;
//      this._Assembly = assembly;
//    }

//    public abstract IList GetEntities(string dynamicLinqStatement);
//  }

//  public class GenericEntityRepositoryEf2<T> : GenericEntityRepositoryEfNonGeneric where T : class {
   

//    public GenericEntityRepositoryEf2(DbContext dbContext, Assembly assembly) : base(dbContext, assembly) {
     
//    }

//    public override IList GetEntities(string dynamicLinqStatement) {
//      return _DbContext.Set<T>().FromSql($"select * from dbo.Employees where LastName = 'John'").ToList();
//      return _DbContext.Set<T>().Where(dynamicLinqStatement).ToList();
//    }
   
//  }

//  internal static class StringExtensions2 {
//    public static string CapitalizeFirst(this string str) {
//      return char.ToUpper(str[0]) + str.Substring(1);
//    }
//    public static string ToLowerFirst(this string str) {
//      return char.ToLower(str[0]) + str.Substring(1);
//    }
//  }
//}
