using System;
using System.ComponentModel.DataAnnotations;

namespace RepositoryTests {

  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  public class LeafModel1 {
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

  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  [HasLookup(nameof(Leaf1), nameof(Leaf1Id), null, null, nameof(LeafModel1))]
  [HasDependent(nameof(Children), nameof(ChildModel1.Root1Id), nameof(ChildModel1.Root1), null, nameof(ChildModel1))]
  public class RootModel1 {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Leaf1Id { get; set; }
    public ChildModel1[] Children { get; set; } = Array.Empty<ChildModel1>();
    public LeafModel1 Leaf1 { get; set; } = null!;
  }

  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  public class ChildModel1 {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Root1Id { get; set; }
    public RootModel1 Root1 { get; set; } = null!;
  }







  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  public class LeafModel2 {
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

  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  [HasLookup(nameof(Leaf2), nameof(Leaf2Id), null, null, nameof(LeafModel2))]
  [HasDependent(nameof(Children), nameof(ChildModel2.Root2Id), nameof(ChildModel2.Root2), null, nameof(ChildModel2))]
  public class RootModel2 {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Leaf2Id { get; set; }
    public ChildModel2[] Children { get; set; } = Array.Empty<ChildModel2>();
    public LeafModel2 Leaf2 { get; set; } = null!;
  }

  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  public class ChildModel2 {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Root2Id { get; set; }
    public RootModel2 Root2 { get; set; } = null!;
  }

}
