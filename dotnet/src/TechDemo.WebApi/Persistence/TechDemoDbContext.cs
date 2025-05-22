using Microsoft.EntityFrameworkCore;
using TechDemo.WebApi.Entities;

namespace TechDemo.WebApi.Persistence {
  public class TechDemoDbContext : DbContext {

    public DbSet<PersonEntity> People { get; set; } = null!;
    public DbSet<AddressEntity> Addresses { get; set; } = null!;
    public DbSet<NationEntity> Nations { get; set; } = null!;

    public TechDemoDbContext() : base() { }

    public TechDemoDbContext(DbContextOptions<TechDemoDbContext> options) : base(options) { }

    public TechDemoDbContext(string connectionString) : base(
      new DbContextOptionsBuilder<TechDemoDbContext>().UseSqlServer(connectionString).Options
    ) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=TechDemoDbContext");
    }

  }
}
