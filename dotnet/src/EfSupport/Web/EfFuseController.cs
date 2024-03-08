//#if NETCOREAPP
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections;
//using System.Data.Fuse.Ef;
//using System.Data.Fuse.Logic;
//using System.Data.Fuse.Persistence;
//using System.Data.ModelDescription;
//using System.Reflection;

//namespace System.Data.Fuse.Web {
//  [ApiController]
//  [Route("[controller]/[action]")]
//  public class EfFuseController<TDbContext>
//    : ControllerBase where TDbContext : DbContext {

//    private readonly ILogger<EfFuseController<TDbContext>> _Logger;
//    private readonly IUniversalRepository _Repo;
//    private readonly Assembly _ModelAssembly;
//    private readonly string _ModelNamespace;

//    public EfFuseController(
//      ILogger<EfFuseController<TDbContext>> logger,
//      TDbContext demoDbContext,
//      Assembly modelAssembly,
//      string modelNamespace
//    ) {
//      _Logger = logger;
//      _ModelAssembly = modelAssembly;
//      _ModelNamespace = modelNamespace;
//      _Repo = new  EfUniversalRepository(demoDbContext, modelAssembly);
//    }

//    [HttpPost]
//    public IActionResult GetSchemaRoot() {
//      try {
//        SchemaRoot result = ModelReader.GetSchema(_ModelAssembly, _ModelNamespace);
//        return Ok(new { Return = result });
//      } catch (Exception ex) {
//        _Logger.LogCritical(ex, ex.Message);
//        return null;
//      }
//    }

//    [HttpPost]
//    public IActionResult GetEntities([FromBody] GenericListSearchParams searchParams) {
//      try {
//        IList page = _Repo.GetEntities(searchParams.EntityName, searchParams.Filter, searchParams.PagingParams, searchParams.SortingParams);
//        int count = _Repo.GetCount(searchParams.EntityName, searchParams.Filter);
//        return Ok(new { Return = new PaginatedResponse() { Page = page, Total = count } });
//      } catch (Exception ex) {
//        _Logger.LogCritical(ex, ex.Message);
//        return null;
//      }
//    }   

//    //[HttpPost]
//    //public IActionResult GetEntityRefs([FromBody] GenericListSearchParams searchParams) {
//    //  try {
//    //    var page = _Repo.GetEntityRefs(
//    //      searchParams.EntityName, searchParams.Filter, searchParams.PagingParams, searchParams.SortingParams
//    //    );
//    //    int count = _Repo.GetCount(searchParams.EntityName, searchParams.Filter);
//    //    return Ok(new { Return = new PaginatedResponse<EntityRef>() { Page = page, Total = count } });
//    //  } catch (Exception ex) {
//    //    _Logger.LogCritical(ex, ex.Message);
//    //    return null;
//    //  }
//    //}

//    [HttpPost]
//    public IActionResult AddOrUpdate([FromBody] GenericAddOrUpdateParams addOrUpdateParams) {
//      try {
//        var result = _Repo.AddOrUpdateEntity(
//          addOrUpdateParams.EntityName, addOrUpdateParams.Entity
//        );
//        return Ok(new { Return = result });
//      } catch (Exception ex) {
//        _Logger.LogCritical(ex, ex.Message);
//        return null;
//      }
//    }

//    [HttpPost]
//    public IActionResult DeleteEntities([FromBody] DeletionParams deletionParams) {
//      try {
//        _Repo.DeleteEntities(
//          deletionParams.EntityName, deletionParams.IdsToDelete
//        );
//        return Ok(new { Return = true });
//      } catch (Exception ex) {
//        _Logger.LogCritical(ex, ex.Message);
//        return null;
//      }
//    }

//    [HttpPost]
//    public IActionResult SearchTest([FromBody] SimpleExpressionTree tree) {
//      try {
//        return Ok(new { Return = tree });
//      } catch (Exception ex) {
//        _Logger.LogCritical(ex, ex.Message);
//        return null;
//      }
//    }
//  }
//}
//#endif