using Microsoft.EntityFrameworkCore;
using RepositoryContract.Demo.Model;

namespace RepositoryContract.Demo.WebApi.Persistence {
  public class DemoDbContext : DbContext {

    DbSet<Employee> Employees { get; set; } = null!;
    DbSet<Address> Addresses { get; set; } = null!;
    DbSet<BusinessUnit> BusinessUnits { get; set; } = null!;
    DbSet<BusinessProject> BusinessProjects { get; set; } = null!;
    DbSet<ContractDetails> ContractDetailsList { get; set; } = null!;

    public DemoDbContext() : base() { }

    public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options) { }

    public DemoDbContext(string connectionString) : base(
      new DbContextOptionsBuilder<DemoDbContext>().UseSqlServer(connectionString).Options
    ) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=DemoDbContext");
    }

  }
}
