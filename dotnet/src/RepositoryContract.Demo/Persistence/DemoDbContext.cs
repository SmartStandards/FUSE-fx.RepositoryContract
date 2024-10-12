using Microsoft.EntityFrameworkCore;
using RepositoryContract.Demo.Model;

namespace RepositoryContract.Demo.WebApi.Persistence {
  public class DemoDbContext : DbContext {

    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<BusinessUnit> BusinessUnits { get; set; } = null!;
    public DbSet<BusinessProject> BusinessProjects { get; set; } = null!;
    public DbSet<ContractDetails> ContractDetailsList { get; set; } = null!;

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
