
namespace System.Data.Fuse.Convenience.Aggregation {

  public sealed class PrimaryEntity {
    public int Id { get; set; }
    public int SecondaryId { get; set; }
    public int PrimaryNumber { get; set; }
    public string PrimaryText { get; set; } = null!;
  }

  public sealed class SecondaryEntity {
    public int Id { get; set; }
    public string SecondaryText { get; set; } = null!;
    public int SecondaryNumber { get; set; }
  }

  public sealed class SampleAggregatedEntity {

    public string AlloverKey { get; set; } = null!;

    public int PrimaryId { get; set; }
    public int SecondaryId { get; set; }

    public int PrimaryNumber { get; set; }
    public string PrimaryText { get; set; } = null!;

    public int SecondaryNumber { get; set; }
    public string SecondaryText { get; set; } = null!;  

  }

  public static class TestDataFactory {

    public static PrimaryEntity[] CreatePrimary() {

      return new PrimaryEntity[] {
        new PrimaryEntity { Id = 1, SecondaryId = 10, PrimaryNumber = 10, PrimaryText = "alpha" },
        new PrimaryEntity { Id = 2, SecondaryId = 10, PrimaryNumber = 20, PrimaryText = "bravo" },
        new PrimaryEntity { Id = 3, SecondaryId = 11, PrimaryNumber = 10, PrimaryText = "charlie" },
        new PrimaryEntity { Id = 4, SecondaryId = 12, PrimaryNumber = 30, PrimaryText = "delta" },
        new PrimaryEntity { Id = 5, SecondaryId = 12, PrimaryNumber = 40, PrimaryText = "echo" }
      };

    }

    public static SecondaryEntity[] CreateSecondary() {

      return new SecondaryEntity[] {
        new SecondaryEntity { Id = 10, SecondaryText = "A", SecondaryNumber = 100 },
        new SecondaryEntity { Id = 11, SecondaryText = "B", SecondaryNumber = 200 },
        new SecondaryEntity { Id = 12, SecondaryText = "C", SecondaryNumber = 100 }
      };

    }

  }

}
