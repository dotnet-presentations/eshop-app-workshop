# Add an identity provider (IdP) and authentication

Before we can start adding traditional shopping capabilities to our web store, like adding items to a shopping basket and checking out to create an order, we need to allow shoppers to register as users on the site. Given that we're building a distributed system comprised of multiple services that will need to operate in the context of the currently logged-in user and facilitate appropriate access control, we'll use a separate dedicated identity provider (IdP) to handle user registration and access control. The individual services in our distributed application will integrate with the IdP using standard authentication and authorization protocols such as [OpenID Connect](https://openid.net/developers/discover-openid-and-openid-connect/).

There are many options available for implementing an IdP, including:

- Hosted services on cloud providers like [Microsoft Entra ID](https://www.microsoft.com/security/business/identity-access/microsoft-entra-id) and [AWS Identity Services](https://aws.amazon.com/identity/).
- Dedicated commercial hosted identity service providers like [Auth0](https://auth0.com/).
- Commercial .NET-based identity service products for on-premises hosting like [Duende IdentityServer](https://duendesoftware.com/products/identityserver).
- Free open-source .NET libraries for building and hosting your own identity service like [OpenIddict](https://documentation.openiddict.com/).
- Free open-source "IdP in a box" solutions that are easy to start with and customizable with plug-ins and code like [Keycloak](https://www.keycloak.org/).

[Keycloak](https://www.keycloak.org/) is available as a configurable container image that makes it very easy to get started with and is supported by a rich community ecosystem. We'll use it to create an IdP for our distributed application.

## Compose a Keycloak instance into the AppHost project via a custom resource type

.NET Aspire includes built-in support for a number of different container-based resources, but is also easy to extend with custom resources that encapsulate useful behavior.

1. The starting point for this lab includes code for a custom Aspire resource for Keycloak in the `KeycloakResource.cs` file of the `eShop.AppHost` project. Open this file and read through the code to get an understanding of how this custom resource builds upon the primitives provided by `Aspire.Hosting`.
1. Open the `Program.cs` file in the `eShop.AppHost` and add code to compose a Keycloak instance named `"idp"` into the app model and import the realm data JSON files in the `../Keycloak/data/import/` directory:

    ```csharp
    // Identity Providers

    var idp = builder.AddKeycloakContainer("idp", tag: "23.0")
        .ImportRealms("../Keycloak/data/import");
    ```

    By default, the Keycloak container added will use the tag `latest` but to protect ourselves against breaking changes in new versions, we specify a specific version tag of `23.0` here.

1. Run the AppHost project and notice that after a few moments the dashboard shows the status of the `idp` resource as "Exited" with a warning triangle icon. Click the **View** link in the **Logs** column of the row for the `idp` resource to read the logs for an indication of what the problem is:

    ![The dashboard showing the 'idp' resource exited](./img/dashboard-idp-exited.png)

    ![The dashboard showing the logs for the exited 'idp' resource](./img/dashboard-idp-logs-error.png)

    The logs should indicate that there was an error with the imported client `webapp`, specifically that the URLs/URIs configured are invalid. This is because the `eshop-realm.json` file that was imported contains processing tokens intended to inject values from environment variables which haven't been configured yet.
1.  We can use Aspire APIs to extract the runtime-assigned URLs for our `webapp` resource and inject them into the `idp` resource as environment variables using the [`WithEnvironment` method](https://learn.microsoft.com/dotnet/api/aspire.hosting.resourcebuilderextensions.withenvironment?view=dotnet-aspire-8.0), so that the processing tokens in the imported `eshop-realm.json` file will be replaced with valid values. Add the following lines to the `Program.cs` file, after the call defining the `webapp` resource. You will need to modify the `webapp` resource code to capture the resource in a variable named `webApp`:

    ```csharp
    // Inject the project URLs for Keycloak realm configuration
    idp.WithEnvironment("WEBAPP_HTTP", () => webApp.GetEndpoint("http").UriString);
    idp.WithEnvironment("WEBAPP_HTTPS", () => webApp.GetEndpoint("https").UriString);
    ```

1. Run the AppHost project again and verify that the container starts successfully. This can be confirmed by finding the following lines in the container's logs:

    ```log
    INFO  [org.keycloak.exportimport.dir.DirImportProvider] (main) Importing from directory /opt/keycloak/bin/../data/import
    INFO  [org.keycloak.services] (main) KC-SERVICES0030: Full model import requested. Strategy: IGNORE_EXISTING
    INFO  [org.keycloak.exportimport.util.ImportUtils] (main) Realm 'eShop' imported
    INFO  [org.keycloak.services] (main) KC-SERVICES0032: Import finished successfully
    ```

1. Go back to the dashboard **Resources** page and click on the link for the `idp` resource in the **Endpoints** column to launch the Keycloak instance's homepage, then click on the **Administration Console** link:

    ![The Keycloak instance homepage](./img/keycloak-home.png)

1. Sign in to the administration console using the following credentials:
    - Username: **admin**
    - Password: **admin**

    ![Signing in to Keycloak as the admin user](./img/keycloak-admin-login.png)

1. Once signed in, select the **eShop** realm from the drop-down in the top-left corner:

    ![Selecting the 'eShop' realm in the Keycloak adminstration console](./img/keycloak-eshop-realm-select.png)

1. Visit the **Clients** and **Users** pages of the administration console and see that the realm is already configured with a client app named **webapp** and a user named **test@example.com**. Note that the **Home URL** for the **webapp** client matches the endpoint URL of our `WebApp` project as that value was injected by the code we added to the `eShop.AppHost` project:

    ![Details of the 'webapp' client in the 'eShop' realm in Keycloak](./img/keycloak-eshop-realm-details.png)

1. Now that we've confirmed that our Keycloak instance is successfully configured, update the `Program.cs` file of the AppHost project so that the `webapp` resource references the `idp` Keycloak resource, using the `WithReference` method. This will ensure that the `webapp` resource will have configuration values injected via its environment variables so that it can resovle calls to `http://idp` with the actual address assigned when the project is launched. Additionally, use the `WithLaunchProfile` method to ensure the `webapp` resource is always launched using the `"https"` launch profile (defined in its `Properties/launchSettings.json` file) as OIDC-based authentication flows typically require HTTPS to be used:

    ```csharp
    var webApp = builder.AddProject<WebApp>("webapp")
        .WithReference(catalogApi)
        .WithReference(idp)
        // Force HTTPS profile for web app (required for OIDC operations)
        .WithLaunchProfile("https");
    ```

1. Launch the AppHost project again and use the dashboard to verify that the address of the `idp` resource was injected into the `webapp` resource via environment variables:

    ![Dashboard showing the environment variables for the 'webapp' resource include those required to configure service discovery for the 'idp' resource](./img/dashboard-webapp-idp-address-injected.png)