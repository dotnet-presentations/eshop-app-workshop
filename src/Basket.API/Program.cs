using eShop.Basket.API.Grpc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationServices();

builder.Services.AddGrpc();

var app = builder.Build();

app.UseDefaultExceptionHandler();

app.MapDefaultEndpoints();

app.MapGrpcService<BasketService>();

app.Run();
