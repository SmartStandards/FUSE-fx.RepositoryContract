#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif
using System.Reflection;
using System.Linq;
using System.Data.Fuse.Convenience;

namespace System.Data.Fuse.Ef {

 // public class EfUniversalRepository : UniversalRepository {  

    //protected readonly DbContext _DbContext;
    //protected readonly Assembly _Assembly;

    //public EfUniversalRepository(DbContext dbContext, Assembly assembly) {
    //  this._DbContext = dbContext;
    //  this._Assembly = assembly;
    //}
    //protected override UniversalRepositoryFacade CreateInnerRepo(string entityName) {
    //  Type[] allTypes = _Assembly.GetTypes();
    //  Type entityType = allTypes.Where((Type t) => t.Name == entityName).FirstOrDefault();
    //  if (entityType == null) { return null; }

    //  Type repoFacadeType = typeof(DynamicRepositoryFacade<>);
    //  repoFacadeType = repoFacadeType.MakeGenericType(entityType);

    //  Type repoType = typeof(EfRepository<>);
    //  repoType = repoType.MakeGenericType(entityType);
    //  object repo = Activator.CreateInstance(repoType, _DbContext);

    //  return (UniversalRepositoryFacade)Activator.CreateInstance(repoFacadeType, repo);
    //}
//}

}
