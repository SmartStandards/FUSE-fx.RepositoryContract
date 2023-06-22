using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RepositoryContract.Demo.Model;
using RepositoryContract.Demo.WebApi.Persistence;
using System;
using System.Data.Fuse;
using System.Data.ModelDescription;
using System.Reflection;

namespace RepositoryConrract.Demo.WebApi.Controllers {
  [ApiController]
  [Route("[controller]/[action]")]
  public class ApiController : ControllerBase {

    private readonly ILogger<ApiController> _Logger;
    private readonly IGenericEntityRepository _GenericEntityRepository;

    public ApiController(ILogger<ApiController> logger, DemoDbContext demoDbContext) {
      _Logger = logger;
      _GenericEntityRepository = new GenericEntityRepositoryEf(demoDbContext, typeof(Employee).Assembly);
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
        //var result = _GenericEntityRepository.GetEntities(searchParams.EntityName);
        var result2 = _GenericEntityRepository.GetDtos(searchParams.EntityName);
        return Ok(new { Return = result2 });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }

    [HttpPost(Name = "AddOrUpdate")]
    public IActionResult AddOrUpdate([FromBody] GenericAddOrUpdateParams addOrUpdateParams) {
      try {
        var result = _GenericEntityRepository.AddOrUpdateEntity(
          addOrUpdateParams.EntityName, addOrUpdateParams.Entity
        );
        return Ok(new { Return = result });
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }
  }
}