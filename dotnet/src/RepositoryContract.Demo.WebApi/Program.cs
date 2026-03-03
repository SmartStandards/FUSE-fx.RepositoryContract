using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RepositoryContract.Demo.Model;
using RepositoryContract.Demo.WebApi.DynamicSqlRepoDemo;
using RepositoryContract.Demo.WebApi.Persistence;
using System;
using System.Data.Common;
using System.Data.Fuse;
using System.Data.Fuse.Convenience;
using System.Data.Fuse.Ef;
using System.Data.Fuse.Ef.InstanceManagement;
using System.Data.Fuse.SchemaResolving;
using System.Data.Fuse.Sql.InstanceManagement;
using System.Data.ModelDescription;
using System.Data.SqlClient;
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


IDataStore sqlDataStore = new DemoSqlDataStore();
builder.Services.AddSingleton<IDataStore>(sqlDataStore);
RegisterRepo<BavPerson, int>(builder.Services);

DynamicUjmwControllerOptions options1 = new DynamicUjmwControllerOptions();
//options1.ApiGroupName = "Demo";
options1.ControllerRoute = "DemoStore";
DynamicUjmwControllerOptions options2 = new DynamicUjmwControllerOptions();
//options2.ApiGroupName = "DemoSqlDataStore";
options2.ControllerRoute = "DemoSqlDataStore";


builder.Services.AddDynamicUjmwControllers(r => {
  r.AddControllerFor<IUniversalRepository>(options1);
  //r.AddControllerFor<DemoSqlDataStore>(options2);
});
//builder.Services.AddUjmwStandardSwaggerGen("Fileaccess-Demo");

var app = builder.Build();

//using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
//  var x = serviceScope.ServiceProvider.GetService<RepositoryCollection>()!;
//}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI();
  app.UseCors("CorsPolicy");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//app.UseUjmwStandardSwagger(app.Configuration, "Fuse-Demo");

app.ConfigureUShellSpaHosting("Portfolio", "UShell", "/");

app.Run();

static void  RegisterRepo<TEntity, TKey>(IServiceCollection s) where TEntity : class {
  s.AddSingleton<IRepository<TEntity, TKey>>((sp => {
    var dataStore = sp.GetService<IDataStore>()!;
    return dataStore.GetRepository<TEntity, TKey>();
  }));
  s.AddDynamicUjmwControllers((c) => {
    c.AddControllerFor<IRepository<TEntity, TKey>>(new DynamicUjmwControllerOptions() {
      ControllerRoute = typeof(TEntity).Name
    });
  });
}
