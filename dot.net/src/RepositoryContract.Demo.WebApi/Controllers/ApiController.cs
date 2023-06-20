//using Microsoft.AspNetCore.Mvc;
//using System.Data.EntitySchema;
//using System.Reflection;
//using ModelReader.Persistence;

//namespace ModelReader.Demo.Controllers {
//  [ApiController]
//  [Route("[controller]/[action]")]
//  public class ApiController : ControllerBase {

//    private readonly ILogger<ApiController> _logger;

//    public ApiController(ILogger<ApiController> logger) {
//      _logger = logger;
//    }

//    [HttpPost(Name = "GetSchemaRoot")]
//    public IActionResult GetSchemaRoot() {
//      try {
//        GenericEntityRepositoryEf test = new GenericEntityRepositoryEf();
//        SchemaRoot result = ModelReader.GetSchema(Assembly.GetExecutingAssembly(), "ModelReader.Demo.Model");
//        return Ok(new {test = result });
//      } catch (Exception ex) {
//        _logger.LogCritical(ex, ex.Message);
//        return null;
//      }
//    }
//  }
//}