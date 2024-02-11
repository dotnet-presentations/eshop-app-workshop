using eShop.Catalog.API;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddDefaultOpenApi();
builder.AddApplicationServices();

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseDefaultOpenApi();

app.MapDefaultEndpoints();

app.MapGroup(app.GetOptions<CatalogOptions>().ApiBasePath)
    .WithTags("Catalog API")
    .MapCatalogApi();

app.Run();
