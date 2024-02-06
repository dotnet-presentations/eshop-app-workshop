var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddDefaultOpenApi();
builder.AddApplicationServices();

var app = builder.Build();

app.UseDefaultExceptionHandler();

app.UseDefaultOpenApi();

app.MapDefaultEndpoints();

app.MapGroup("/api/v1/orders")
   .MapOrdersApi()
   .RequireAuthorization();

app.Run();
