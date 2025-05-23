using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using TechDemo.WebApi.DomainObjects;
using TechDemo.WebApi.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<TechDemoEfDataStore>();
builder.Services.AddSingleton<TechDemoMveDataStore>(
  (s) => new TechDemoMveDataStore(new Tuple<Type, Type>[] { 
    new Tuple<Type, Type>(typeof(Person), typeof(int)),
    new Tuple<Type, Type>(typeof(Nation), typeof(int)),
    new Tuple<Type, Type>(typeof(Address), typeof(int))
  })
);
builder.Services.AddSingleton<TechDemoSqlDataStore>(
  (s) => new TechDemoSqlDataStore(new Tuple<Type, Type>[] {
    new Tuple<Type, Type>(typeof(Person), typeof(int)),
    new Tuple<Type, Type>(typeof(Nation), typeof(int)),
    new Tuple<Type, Type>(typeof(Address), typeof(int))
  })
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
