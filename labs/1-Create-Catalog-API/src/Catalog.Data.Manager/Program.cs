using eShop.Catalog.Data;
using eShop.Catalog.Data.Manager;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<CatalogDbContext>("CatalogDB", null,
    optionsBuilder => optionsBuilder.UseNpgsql(npgsqlBuilder =>
        npgsqlBuilder.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)));

builder.Services.AddMigration<CatalogDbContext, CatalogContextSeed>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
