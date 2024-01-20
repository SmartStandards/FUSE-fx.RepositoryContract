#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Data.Fuse;
using System.Data.Fuse.Logic;
using System.Data.ModelDescription;
using System.Reflection;

namespace RepositoryConrract.WebApi {
  [ApiController]
  [Route("[controller]/[action]")]
  public class DbContextBasedFuseController<TDbContext>
    : ControllerBase where TDbContext : DbContext {

    private readonly ILogger<DbContextBasedFuseController<TDbContext>> _Logger;
    private readonly IRepository _Repo;
    private readonly Assembly _ModelAssembly;
    private readonly string _ModelNamespace;

    public DbContextBasedFuseController(
      ILogger<DbContextBasedFuseController<TDbContext>> logger, 
      TDbContext demoDbContext, 
      Assembly modelAssembly,
      string modelNamespace
    ) {
      _Logger = logger;
      _ModelAssembly = modelAssembly;
      _ModelNamespace = modelNamespace;
      _Repo = new EfRepository(demoDbContext, modelAssembly);
    }

    [HttpPost]
    public IActionResult GetSchemaRoot() {
      try {
        SchemaRoot result = ModelReader.ModelReader.GetSchema(_ModelAssembly, _ModelNamespace);
        return Ok(new { Return = result });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }

    [HttpPost]
    public IActionResult GetEntities([FromBody] GenericListSearchParams searchParams) {
      try {
        IList page = _Repo.GetDbEntities(searchParams.EntityName, searchParams.Filter, searchParams.PagingParams, searchParams.SortingParams);
        int count = _Repo.GetCount(searchParams.EntityName, searchParams.Filter);
        return Ok(new { Return = new PaginatedResponse() { Page = page, Total = count } });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }

    [HttpPost]
    public IActionResult GetDtos([FromBody] GenericListSearchParams searchParams) {
      try {
        var result = _Repo.GetBusinessModels(searchParams.EntityName, searchParams.Filter, searchParams.PagingParams, searchParams.SortingParams);
        return Ok(new { Return = result });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }

    [HttpPost]
    public IActionResult GetEntityRefs([FromBody] GenericListSearchParams searchParams) {
      try {
        var page = _Repo.GetEntityRefs(searchParams.EntityName, searchParams.Filter, searchParams.PagingParams, searchParams.SortingParams);
        int count = _Repo.GetCount(searchParams.EntityName, searchParams.Filter);
        return Ok(new { Return = new PaginatedResponse<EntityRef>() { Page = page, Total = count } });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }

    [HttpPost]
    public IActionResult AddOrUpdate([FromBody] GenericAddOrUpdateParams addOrUpdateParams) {
      try {
        var result = _Repo.AddOrUpdateEntity(
          addOrUpdateParams.EntityName, addOrUpdateParams.Entity
        );
        return Ok(new { Return = result });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }

    [HttpPost]
    public IActionResult DeleteEntities([FromBody] DeletionParams deletionParams) {
      try {
        _Repo.DeleteEntities(
          deletionParams.EntityName, deletionParams.IdsToDelete
        );
        return Ok(new { Return = true });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }

    [HttpPost]
    public IActionResult SearchTest([FromBody] SimpleExpressionTree tree) {
      try {
        return Ok(new { Return = tree });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }
  }
}
#endif