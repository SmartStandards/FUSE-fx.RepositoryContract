using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RepositoryContract.Demo.WebApi.Persistence;

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

var app = builder.Build();

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
