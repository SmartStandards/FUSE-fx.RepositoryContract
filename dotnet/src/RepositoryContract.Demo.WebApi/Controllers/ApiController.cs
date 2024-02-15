using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using RepositoryContract.Demo.Model;
using RepositoryContract.Demo.Persistence;
using RepositoryContract.Demo.WebApi.Persistence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Fuse;
using System.Data.Fuse.Logic;
using System.Linq;

namespace RepositoryConrract.Demo.WebApi.Controllers {
  [ApiController]
  [Route("[controller]/[action]")]
  public class ApiController : ControllerBase {

    private readonly ILogger<ApiController> _Logger;
    private readonly IUniversalRepository _Repo;

    public ApiController(ILogger<ApiController> logger, DemoDbContext demoDbContext) {
      _Logger = logger;
      _Repo = new LocalDynamicRepository(typeof(Employee).Assembly);
    }

    [HttpPost(Name = "GetSchemaRoot")]
    public IActionResult GetSchemaRoot() {
      try {
        //SchemaRoot result = ModelReader.ModelReader.GetSchema(typeof(Employee).Assembly, "RepositoryContract.Demo.Model");
        //return Ok(new { Return = result });
        return Ok();
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return new StatusCodeResult(500);
      }
    }

    [HttpPost(Name = "GetEntities")]
    public IActionResult GetEntities([FromBody] GenericListSearchParams searchParams) {
      try {
        IList<Dictionary<string,object>> page = _Repo.GetEntities(searchParams.EntityName, searchParams.Filter, searchParams.PagingParams, searchParams.SortingParams);
        int count = _Repo.GetCount(searchParams.EntityName, searchParams.Filter);
        return Ok(new { Return = new PaginatedResponse() { Page = page.ToList(), Total = count } });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return new StatusCodeResult(500);
      }
    }

    [HttpPost(Name = "GetEntityRefs")]
    public IActionResult GetEntityRefs([FromBody] GenericListSearchParams searchParams) {
      try {
        var page = _Repo.GetEntityRefs(searchParams.EntityName, searchParams.Filter, searchParams.PagingParams, searchParams.SortingParams);
        int count = _Repo.GetCount(searchParams.EntityName, searchParams.Filter);
        return Ok(new { Return = new PaginatedResponse<EntityRef>() { Page = page, Total = count } });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return new StatusCodeResult(500);
      }
    }

    [HttpPost(Name = "AddOrUpdate")]
    public IActionResult AddOrUpdate([FromBody] GenericAddOrUpdateParams addOrUpdateParams) {
      try {
        var result = _Repo.AddOrUpdateEntity(
          addOrUpdateParams.EntityName, addOrUpdateParams.Entity
        );
        return Ok(new { Return = result });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return new StatusCodeResult(500);
      }
    }

    [HttpPost(Name = "DeleteEntities")]
    public IActionResult DeleteEntities([FromBody] DeletionParams deletionParams) {
      try {
        _Repo.DeleteEntities(
          deletionParams.EntityName, deletionParams.IdsToDelete
        );
        return Ok(new { Return = true });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return new StatusCodeResult(500);
      }
    }

    [HttpPost(Name = "SearchTest")]
    public IActionResult SearchTest([FromBody] SimpleExpressionTree tree) {
      try {
        return Ok(new { Return = tree });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return new StatusCodeResult(500);
      }
    }
  }
}