﻿using System;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Linq;
using System.Reflection;

namespace RepositoryContract.Demo.Persistence {
  public class LocalDynamicRepository : UniversalRepository {

    protected readonly Assembly _Assembly;

    public LocalDynamicRepository(Assembly assembly) {
      this._Assembly = assembly;
    }

    protected override UniversalRepositoryFacade CreateInnerRepo(string entityName) {

      throw new NotImplementedException();

      //Type[] allTypes = _Assembly.GetTypes();
      //Type? entityType = allTypes.Where((Type t) => t.Name == entityName).FirstOrDefault();
      //if (entityType == null) { return null; }

      //Type repoFacadeType = typeof(DynamicRepositoryFacade<>);
      //repoFacadeType = repoFacadeType.MakeGenericType(entityType);

      //Type repoType = typeof(LocalRepository<>);
      //repoType = repoType.MakeGenericType(entityType);
      //object repo = Activator.CreateInstance(repoType);

      //return (UniversalRepositoryFacade)Activator.CreateInstance(repoFacadeType, repo);
    }

  }
}
