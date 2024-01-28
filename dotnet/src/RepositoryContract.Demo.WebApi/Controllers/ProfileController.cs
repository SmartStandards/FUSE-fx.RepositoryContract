//using Jose;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using RepositoryContract.Demo.Model;
//using RepositoryContract.Demo.WebApi.Persistence;
//using RepositoryContract.Demo.WebApi.PortfolioHandling;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data.Fuse;
//using System.Data.Fuse.Logic;
//using System.Data.ModelDescription;
//using System.Reflection;

//namespace RepositoryConrract.Demo.WebApi.Controllers {
//  [ApiController]
//  public class ProfileController : ControllerBase {

//    private readonly ILogger<ApiController> _Logger;

//    public ProfileController(ILogger<ApiController> logger, DemoDbContext demoDbContext) {
//      _Logger = logger;
//    }

//    [HttpGet()]
//    [Route("Profile/Portfolio/{portfolioName}")]
//    public ActionResult<PortfolioDescriptionResponse> GetPortfolio([FromRoute] string portfolioName) {
//      try {
//        return Ok(PortfolioFactory.GetPortfolio(portfolioName));
//      } catch (Exception ex) {
//        _Logger.LogCritical(ex, ex.Message);
//        return null;
//      }
//    }

//    [HttpGet()]
//    [Route("Profile/PortfolioIndex.json")]
//    public ActionResult<List<PortfolioEntry>> PortfolioIndex() {
//      try {
//        List<PortfolioEntry> list = new List<PortfolioEntry>();
//        PortfolioEntry p1 = new PortfolioEntry() {
//          Label = "Validation App Client C",
//          PortfolioUrl = "ValidationProfileClientC",
//        };
//        p1.Tags.Add("Tenant", "ClientC");
//        p1.Tags.Add("Environment", "Test");
//        p1.Tags.Add("Product", "Validation App");
//        PortfolioEntry p2 = new PortfolioEntry() {
//          Label = "Validation App Client D",
//          PortfolioUrl = "ValidationProfileClientD",
//        };
//        p2.Tags.Add("Tenant", "ClientD");
//        p2.Tags.Add("Environment", "Test");
//        p2.Tags.Add("Product", "Validation App");
//        list.Add(p1);
//        list.Add(p2);

//        return Ok(list);
//      } catch (Exception ex) {
//        _Logger.LogCritical(ex, ex.Message);
//        return null;
//      }
//    }

//  }
//}