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
using Microsoft.EntityFrameworkCore.Metadata;

namespace System.Data.Fuse {

  public abstract class EfRepositoryBase {
    protected readonly DbContext context;

    public EfRepositoryBase(DbContext dbContext) {
      this.context = dbContext;
    }

    public abstract IList GetEntities1(SimpleExpressionTree filter);
    public abstract IList GetEntities1(string dynamicLinqFilter);

    public abstract object AddOrUpdate1(Dictionary<string, JsonElement> entity);

    public abstract IList<Dictionary<string, object>> GetDtos1(SimpleExpressionTree filter);
    public abstract void DeleteEntities1(JsonElement[][] entityIdsToDelete);
    public abstract IList<Dictionary<string, object>> GetDtos1(string dynamicLinqFilter);
  
  }
  
}
