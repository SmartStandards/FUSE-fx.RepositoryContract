using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryTests {

  public class LeafEntity1 {
    public int Id { get; set; }
    public long LongValue { get; set; }
    public string StringValue { get; set; } = string.Empty;
    public DateTime DateValue { get; set; }
    public Guid GuidValue { get; set; }
    public bool BoolValue { get; set; }
    public float FloatValue { get; set; }
    public double DoubleValue { get; set; }
    public decimal DecimalValue { get; set; }
  }

}
