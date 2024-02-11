# Add checkout functionality to the web site

Similar to [lab 1](../1-Create-Catalog-API/README.md), a database has already been defined to store order details for eShop, along with an Entity Framework Core model, and a web app that ensures the database is created and updated to the latest schema by running migrations on startup.

![ERD for the Ordering database](./img/ordering-db-erd.png)

## Getting familiar with the Ordering Database

1. Open the [`eShop.sln`](./src/eShop.sln) in Visual Studio or VS Code.
1. An Entity Framework Core model is already defined for this database in the `Ordering.Data` project. Open the `OrderingDbContext.cs` file in this project and look at the code to see that the the various tables are defined via properties and [classes implementing `IEntityTypeConfiguration<TEntity>`](https://learn.microsoft.com/ef/core/modeling/#grouping-configuration).
1. The `Ordering.Data` project only defines the `DbContext` and entity types. The [EF Core migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/) are defined and managed in the `Ordering.Data.Manager` project. This is a web project that includes some custom code to facilitate creating and seeding the database when the application starts.
1. The AppHost has already been configured to create a PostgreSQL container resource named `OrderingDB` and had the `Ordering.Data.Manager` project added to it as a resource named `ordering-db-mgr` with a reference to the `OrderingDB` database.
1. Run the AppHost project and verify using the dashboard and the **pgAdmin** tool that the `OrderingDB` database has been created and contains the tables defined by the Entity Framework Core migrations.

    ![Ordering database tables shown in pgAdmin](./img/pgadmin-ordering-db-tables.png)

## Create the Ordering API project

The AppHost project has already been configured to host, let's add an HTTP API that provides the catalog details stored in the database.