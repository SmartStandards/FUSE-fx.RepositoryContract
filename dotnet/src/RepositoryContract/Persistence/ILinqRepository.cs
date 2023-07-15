using System.Linq.Expressions;
using System.Linq;

namespace System.Data.Fuse {

  public interface ILinqRepository<TDbEntity> {
    IQueryable<TDbEntity> GetEntities(Expression<Func<TDbEntity, bool>> filter);

    IQueryable<TDbEntity> GetEntitiesDynamic(string dynamicLinqFilter);
  }

}