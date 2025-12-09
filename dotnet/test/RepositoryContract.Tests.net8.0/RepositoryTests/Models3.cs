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
  [PropertyGroup(nameof(Leaf1Id), nameof(Leaf1Id))]
  [HasDependent(nameof(Children), nameof(ChildModel1.Root1Id), nameof(ChildModel1.Root1), null, nameof(ChildModel1))]
  [PropertyGroup("OtherKey", nameof(OtherField1), nameof(OtherField2))]
  [HasLookup(nameof(OtherLeaf), "OtherKey", null, null, nameof(LeafModelWithCompositeKey))]
  public class RootModel1 {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Leaf1Id { get; set; }
    public ChildModel1[] Children { get; set; } = Array.Empty<ChildModel1>();
    public LeafModel1 Leaf1 { get; set; } = null!;
    public int OtherField1 { get; set; }
    public string OtherField2 { get; set; } = string.Empty;
    public LeafModelWithCompositeKey? OtherLeaf { get; set; }
  }

  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  [PropertyGroup(nameof(Root1Id), nameof(Root1Id))]
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
  [PropertyGroup(nameof(Leaf2Id), nameof(Leaf2Id))]
  public class RootModel2 {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Leaf2Id { get; set; }
    public ChildModel2[] Children { get; set; } = Array.Empty<ChildModel2>();
    public LeafModel2 Leaf2 { get; set; } = null!;
  }

  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  [PropertyGroup(nameof(Root2Id), nameof(Root2Id))]
  public class ChildModel2 {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Root2Id { get; set; }
    public RootModel2 Root2 { get; set; } = null!;
  }

  [UniquePropertyGroup("PrimaryKey", nameof(Field1), nameof(Field2))]
  [PrimaryIdentity("PrimaryKey")]
  public class LeafModelWithCompositeKey {
    public int Field1 { get; set; }
    public string Field2 { get; set; } = string.Empty;
    public long LongValue { get; set; }
    public string StringValue { get; set; } = string.Empty;
    public DateTime DateValue { get; set; } = new DateTime(1900, 1, 1);
    public Guid GuidValue { get; set; }
    public bool BoolValue { get; set; }
    public float FloatValue { get; set; }
    public double DoubleValue { get; set; }
    public decimal DecimalValue { get; set; }
  }

  [UniquePropertyGroup("PrimaryKey", nameof(KeyField1), nameof(KeyField2))]
  [PrimaryIdentity("PrimaryKey")]
  [HasDependent(nameof(Children), "Root1Key", nameof(ChildModelOfRootModelWithCompositeKey.Parent), null, nameof(ChildModelOfRootModelWithCompositeKey))]
  public class RootModelWithCompositeKey {
    public int KeyField1 { get; set; }
    public string KeyField2 { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ChildModelOfRootModelWithCompositeKey[] Children { get; set; } = Array.Empty<ChildModelOfRootModelWithCompositeKey>();
  }

  [UniquePropertyGroup("PrimaryKey", nameof(Id))]
  [PrimaryIdentity("PrimaryKey")]
  [PropertyGroup("Root1Key", nameof(Root1KeyField1), nameof(Root1KeyField2))]
  public class ChildModelOfRootModelWithCompositeKey {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Root1KeyField1 { get; set; }
    public string Root1KeyField2 { get; set; } = string.Empty;
    public RootModelWithCompositeKey Parent { get; set; } = null!;
  }

}
