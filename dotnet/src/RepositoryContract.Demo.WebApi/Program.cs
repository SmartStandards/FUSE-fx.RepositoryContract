using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RepositoryContract.Demo.Model;
using RepositoryContract.Demo.WebApi.Persistence;
using System;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.Fuse.Ef;
using System.Data.Fuse.Ef.InstanceManagement;
using System.Data.Fuse.SchemaResolving;
using System.Web.UJMW;

var builder = WebApplication.CreateBuilder(args);

string[] allowdOrigins = { "http://localhost:3000", "http://localhost:3001" };
builder.Services.AddCors(
  opt => {
    opt.AddPolicy(
      "CorsPolicy",
      c => c
        .WithOrigins(allowdOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
      );
  }
);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DemoDbContext>(
  options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddScoped<IUniversalRepository>((s) => {
  DemoDbContext dbContext = s.GetService<DemoDbContext>()!;
  return new EfDictUniversalRepository(
    new ShortLivingDbContextInstanceProvider(
      () => new DemoDbContext(connectionString)
    ),
    new DbContextRuntimeEntityResolver(() => s.GetService<DemoDbContext>()!, true)
  );
});

builder.Services.AddScoped<IEfDataStore>((s) => {
  Func<DemoDbContext> factory = () => {
    return s.GetService<DemoDbContext>()!;
  };
  IDbContextInstanceProvider dbContextProvider = new ShortLivingDbContextInstanceProvider<DemoDbContext>(
    factory
  );
  return new EfDataStore<DemoDbContext>(dbContextProvider);
});

builder.Services.AddScoped<RepositoryCollection>((s) => {
  RepositoryCollection? dataStore = new RepositoryCollection(
    new ListBasedEntityResolver(
      new Type[] { typeof(Employee) }
    )
  );
  IEfDataStore efDataStore = s.GetService<IEfDataStore>()!;
  dataStore.RegisterRepository(
    ConversionHelper.CreateModelVsEntityRepositry<Employee, Employee, int>(
      efDataStore, dataStore
    )
  );
  return dataStore;
});

builder.Services.AddDynamicUjmwControllers(r => {
  r.AddControllerFor<IUniversalRepository>("DemoStore");
});

var app = builder.Build();

using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
  var x = serviceScope.ServiceProvider.GetService<RepositoryCollection>()!;
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI();
  app.UseCors("CorsPolicy");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
