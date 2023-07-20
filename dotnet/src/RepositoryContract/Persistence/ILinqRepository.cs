using System.Linq.Expressions;
using System.Linq;

namespace System.Data.Fuse {

  public interface ILinqRepository<TDbEntity> {
    IQueryable<TDbEntity> QueryDbEntities(Expression<Func<TDbEntity, bool>> filter);

    IQueryable<TDbEntity> QueryDbEntities(string dynamicLinqFilter);
  }

}