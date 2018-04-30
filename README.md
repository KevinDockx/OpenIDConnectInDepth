# OpenID Connect in Depth
OpenID Connect code sample (Angular 5, ASP.NET Core 2.0, IdentityServer4) containing SSOn/Out, reference tokens, custom grants and multi-tenancy.  

## Single Sign On / Out 
Run Angular & MVC clients - signing in is only required once.   When signing out of the IDP the other client is notified and signed out of.

## Reference tokens
Sample.API expects a reference token and validates this with the IDP on each call (default caching applies).

## Custom grant
When calling Sample.SecondAPI from Sample.API a new access token is requested (keepin the identity of the current user) with the required scope for the second API.  

The custom grant is an "on behalf of" grant.  This allows the user's identity to flow through a set of APIs, avoids access tokens that are too permissive (audience too large) and allows checking user rights to a certain API (scope) when exchanging the token.

## Multi-tenancy
Test by launching on the correct host.  Tenant 1 = https://localhost:44318, Tenant 2 = https://localhost:44319

Implemented features are:
- the tenant information can be used at client level to adjust the client accordingly (eg: to change colours)
- the tenant id is passed through to the IDP.  This allows separating out user stores depending on the tenant.  In this case, localhost:44318 will allow local login (with test users), while localhost:44319 allows Google authentication.  
This approach keeps the client clean: client only needs to know about one IDP, and it's the IDP that's responsible for using the correct user store.
- the tenant id is also added to the access token.  This can be used to diversify between tenants at level of the API.
