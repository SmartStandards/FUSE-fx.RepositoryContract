using System;
using System.ComponentModel.DataAnnotations;

namespace RepositoryTests {

  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  public class LeafEntity1 {
    public int Id { get; set; }
    public long LongValue { get; set; }
    public string StringValue { get; set; } = string.Empty;
    public DateTime DateValue { get; set; } = new DateTime(1900, 1, 1);
    public Guid GuidValue { get; set; }
    public bool BoolValue { get; set; }
    public float FloatValue { get; set; }
    public double DoubleValue { get; set; }
    public decimal DecimalValue { get; set; }
  }

  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  [HasLookup(null, nameof(Leaf1Id), null, null, nameof(LeafEntity1))]
  [HasDependent(null, nameof(ChildEntity1.Root1Id), null, null, nameof(ChildEntity1))]
  [PropertyGroup(nameof(Leaf1Id), nameof(Leaf1Id))]
  public class RootEntity1 {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Leaf1Id { get; set; }
  }

  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  [PropertyGroup(nameof(Root1Id), nameof(Root1Id))]
  public class ChildEntity1 {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Root1Id { get; set; }
  }

  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  public class LeafEntity2 {
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
  [HasLookup(nameof(Leaf2), nameof(Leaf2Id), null, null, nameof(LeafEntity2))]
  [HasDependent(nameof(Children2), nameof(ChildEntity2.Root2Id), null, null, nameof(ChildEntity2))]
  public class RootEntity2 {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Leaf2Id { get; set; }
    public LeafEntity2 Leaf2 { get; set; } = null!;
    public ChildEntity2[] Children2 { get; set; } = Array.Empty<ChildEntity2>();
  }

  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  public class ChildEntity2 {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Root2Id { get; set; }
    public RootEntity2 Root2 { get; set; } = null!;
  }

}
