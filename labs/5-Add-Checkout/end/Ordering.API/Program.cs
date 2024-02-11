using eShop.Ordering.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddDefaultOpenApi();
builder.AddDefaultAuthentication();
builder.AddNpgsqlDbContext<OrderingDbContext>("OrderingDB");

var app = builder.Build();

app.UseDefaultOpenApi();

app.MapDefaultEndpoints();

app.MapGroup("/api/v1/orders")
   .MapOrdersApi()
   .RequireAuthorization();

app.Run();
