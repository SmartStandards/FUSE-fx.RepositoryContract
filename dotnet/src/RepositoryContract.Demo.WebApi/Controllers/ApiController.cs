using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RepositoryContract.Demo.Model;
using RepositoryContract.Demo.WebApi.Persistence;
using System;
using System.Data.Fuse;
using System.Data.Fuse.Logic;
using System.Data.ModelDescription;
using System.Reflection;

namespace RepositoryConrract.Demo.WebApi.Controllers {
  [ApiController]
  [Route("[controller]/[action]")]
  public class ApiController : ControllerBase {

    private readonly ILogger<ApiController> _Logger;
    private readonly IRepository _Repo;

    public ApiController(ILogger<ApiController> logger, DemoDbContext demoDbContext) {
      _Logger = logger;
      _Repo = new EfRepository(demoDbContext, typeof(Employee).Assembly);
    }

    [HttpPost(Name = "GetSchemaRoot")]
    public IActionResult GetSchemaRoot() {
      try {
        SchemaRoot result = ModelReader.ModelReader.GetSchema(typeof(Employee).Assembly, "RepositoryContract.Demo.Model");
        return Ok(new { Return = result });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }

    [HttpPost(Name = "GetEntities")]
    public IActionResult GetEntities([FromBody] GenericListSearchParams searchParams) {
      try {
        var result = _Repo.GetDbEntities(searchParams.EntityName, searchParams.Filter);
        return Ok(new { Return = result });
      }
      catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }

    [HttpPost(Name = "GetDtos")]
    public IActionResult GetDtos([FromBody] GenericListSearchParams searchParams) {
      try {
        var result = _Repo.GetBusinessModels(searchParams.EntityName, searchParams.Filter);
        return Ok(new { Return = result });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }

    [HttpPost(Name = "GetEntityRefs")]
    public IActionResult GetEntityRefs([FromBody] GenericListSearchParams searchParams) {
      try {
        var result2 = _Repo.GetEntityRefs(searchParams.EntityName);
        return Ok(new { Return = result2 });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
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
        return null;
      }
    }

    [HttpPost(Name = "DeleteEntities")]
    public IActionResult DeleteEntities([FromBody] DeletionParams deletionParams) {
      try {
        _Repo.DeleteEntities(
          deletionParams.EntityName, deletionParams.IdsToDelete
        );
        return Ok();
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }

    [HttpPost(Name = "SearchTest")]
    public IActionResult SearchTest([FromBody] SimpleExpressionTree tree) {
      try {        
        return Ok(new { Return = tree });
      }
      catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }
  }
}