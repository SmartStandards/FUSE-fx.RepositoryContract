using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContract.Tests._Models_ {
  
  [HasDependent(nameof(Students))]
  public class Principal {
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Student> Students { get; set; } = new List<Student>();
  }

  [HasPrincipal(nameof(Principal), "")]
  public class Student {
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Principal Principal { get; set; } = null!;
  }
}
