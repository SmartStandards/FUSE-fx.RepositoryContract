using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContract.Tests {

  [HasDependent(nameof(Students),nameof(StudentEntity.PrincipalId),nameof(StudentEntity.Principal))]
  public class PrincipalEntity {
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<StudentEntity> Students { get; set; } = new List<StudentEntity>();
  }

  public class StudentEntity {
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int PrincipalId { get; set; }
    public PrincipalEntity Principal { get; set; } = null!;
  }
}
