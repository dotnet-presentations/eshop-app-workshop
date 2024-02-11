using eShop.Ordering.Data;
using eShop.Ordering.Data.Manager;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<OrderingDbContext>("OrderingDB", null,
    optionsBuilder => optionsBuilder.UseNpgsql(npgsqlBuilder =>
        npgsqlBuilder.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)));

builder.Services.AddMigration<OrderingDbContext, OrderingDbContextSeed>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
