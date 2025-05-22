using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContract.Tests.OverviewTests {

  public class AddressEntity {
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
  }

  public class NationEntity {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
  }

  public class PersonEntity {

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

  }
}
