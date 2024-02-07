var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
                      .WithPgAdmin();
var catalogDb = postgres.AddDatabase("CatalogDB");

builder.AddProject<Projects.Catalog_Data_Manager>("catalog-db-mgr")
    .WithReference(catalogDb);

builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb);

builder.Build().Run();
