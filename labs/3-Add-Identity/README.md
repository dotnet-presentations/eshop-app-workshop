# Add an identity provider (IdP) and authentication

Before we can start adding traditional shopping capabilities to our web store, like adding items to a shopping basket and checking out to create an order, we need to allow shoppers to register as users on the site. Given that we're building a distributed system comprised of multiple services that will need to operate in the context of the currently logged-in user and facilitate appropriate access control, we'll use a separate dedicated identity provider (IdP) to handle user registration and access control. The individual services in our distributed application will integrate with the IdP using standard authentication and authorization protocols such as [OpenID Connect](https://openid.net/developers/discover-openid-and-openid-connect/).

There are many options available for implementing an IdP, including:

- Hosted services on cloud providers like [Microsoft Entra ID](https://www.microsoft.com/security/business/identity-access/microsoft-entra-id) and [AWS Identity Services](https://aws.amazon.com/identity/).
- Dedicated commercial hosted identity service providers like [Auth0](https://auth0.com/).
- Commercial .NET-based identity service products for on-premises hosting like [Duende IdentityServer](https://duendesoftware.com/products/identityserver).
- Free open-source .NET libraries for building and hosting your own identity service like [OpenIddict](https://documentation.openiddict.com/).
- Free open-source "IdP in a box" solutions that are easy to start with and customizable with plug-ins and code like [Keycloak](https://www.keycloak.org/).

[Keycloak](https://www.keycloak.org/) is available as a configurable container image that makes it very easy to get started with and is supported by a rich community ecosystem. We'll use it to create an IdP for our distributed application.

