using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RepositoryContract.Demo.WebApi.PortfolioHandling {

  public static class PortfolioFactory {

    private static string ReadResource(string resourceName) {
      Assembly assembly = Assembly.GetExecutingAssembly();
      if (!assembly.GetManifestResourceNames().Contains(resourceName)) { return ""; }
      string result;
      using (Stream stream = assembly.GetManifestResourceStream(resourceName))
      using (StreamReader reader = new StreamReader(stream)) {
        result = reader.ReadToEnd();
      }
      return result;
    }

    public static PortfolioDescriptionResponse GetPortfolio(string portfolioName) {
      string resoucreName = $"RepositoryContract.Demo.WebApi.PortfolioHandling.{portfolioName}";
      return new PortfolioDescriptionResponse() {
        PortfolioDescriptionJson = ReadResource(resoucreName)
      };
    }
  }
}



