using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UShell;

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

    public static PortfolioDescription GetPortfolio() {
      //string resoucreName = $"RepositoryContract.Demo.WebApi.PortfolioHandling.{portfolioName}";
      return new PortfolioDescription() {
        ApplicationTitle = "Demo Application",
        ModuleDescriptionUrls = new string[] {
          "Module/SchemaEditor.json"
        }
      };
    }

    public static ModuleDescription GetSchemaEditorModuleDescription() {
      ModuleDescription result = new ModuleDescription();
      result.ModuleTitle = "Schema Editor";
      result.AddUsecaseToWorkspaceWithCommand(
        "Schema Editor", "Schema Editor", "Schema", (ud, wd, cd) => {
          ud.WidgetClass = "SchemaEditor";
        }
      );
      return result;
    }
  }
}


