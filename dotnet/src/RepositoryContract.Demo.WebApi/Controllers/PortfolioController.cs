using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RepositoryContract.Demo.WebApi.PortfolioHandling;
using System;
using UShell;

namespace RepositoryConrract.Demo.WebApi.Controllers {
  [ApiController]
  public class PortfolioController : ControllerBase {

    private readonly ILogger<PortfolioController> _Logger;

    public PortfolioController(ILogger<PortfolioController> logger) {
      _Logger = logger;
    }

    [HttpGet()]
    [Route("Portfolio/{portfolioName}")]
    public IActionResult GetPortfolio([FromRoute] string portfolioName) {
      try {
        return Ok(PortfolioFactory.GetPortfolio());
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }

    [HttpGet()]
    [Route("Module/{moduleName}")]
    [Route("Portfolio/Module/{moduleName}")]
    public IActionResult GetModule([FromRoute] string moduleName) {
      try {
        return Ok(PortfolioFactory.GetSchemaEditorModuleDescription());
      } catch (Exception ex) {
        _Logger.LogCritical(ex, ex.Message);
        return null;
      }
    }

  }
}