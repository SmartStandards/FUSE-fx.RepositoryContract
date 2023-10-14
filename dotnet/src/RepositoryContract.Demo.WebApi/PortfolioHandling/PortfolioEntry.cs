using System.Collections.Generic;

namespace RepositoryContract.Demo.WebApi.PortfolioHandling {
  public class PortfolioEntry {
    public string Label { get; set; } = string.Empty;
    public string PortfolioUrl { get; set; } = string.Empty;
    public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
  }
}
