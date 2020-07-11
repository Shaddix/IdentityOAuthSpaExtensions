# IdentityOAuthSpaExtensions
**.Net Core 3.1** library that allows easy integration of external OAuth providers into your SPA. It has even more perks if you use IdentityServer. Here's the [demo](https://oauth.arturdr.ru) if you care.
![Example workflow](example.gif)

# What you can do with this library?
- On SPA side you could receive AuthCode from OAuth provider ([Authorization Code Flow](https://oauth.net/2/grant-types/authorization-code/))
- On backend you could verify AuthCode (passed from your SPA) and get user information from oAuth provider
- If you're using IdentityServer, you could plug-in an [extension grant](http://docs.identityserver.io/en/latest/topics/extension_grants.html) that will allow you to issue your own JWT tokens in exchange for AuthCode (and optionally create new users).


# Goal
The project goal is to allow easy integration of external OAuth providers (e.g. Google, Facebook, etc.) into your SPA applications (React, Angular, plain-old-js, whatever), with the minimum amount of needed code.
This is a backend library, that integrates with Asp.Net Core 2.2+.
The library is kept minimal, as we reuse all [official](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-2.2) and [non-official](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/other-logins?view=aspnetcore-2.2) authentication providers (i.e. library doesn't need to be updated when those external providers change).

# How to
Just install nuget to add the library to your project.

```dotnet add package IdentityOAuthSpaExtensions```

You could also take a look at [IdentityOAuthSpaExtensions.Example](IdentityOAuthSpaExtensions.Example) for example usage (keep in mind, that there are hardcoded ClientId/ClientSecret for FB and Google within Example app. They are for demo purposes and everyone can use them, so beware).

## Backend
From `ConfigureServices` call `services.ConfigureExternalAuth(Configuration)`.

That's it. Just `.AddAuthentication.AddGoogle()` or `.AddFacebook()` to make it work. Follow instructions on how to set up applications on OAuth provider side. You should specify `https://YOUR_BACKEND/external-auth/callback-{providerName}` as a return URL (`https://YOUR_BACKEND/external-auth/callback-google`, `https://YOUR_BACKEND/external-auth/callback-facebook`, etc.)

After that you will be able to request AuthCode from SPA (instructions below), and manually verify AuthCode on backend:
`this.HttpContext.RequestServices.GetService<ExternalAuthService>().GetExternalUserId(providerName, authCode)`
or
`this.HttpContext.RequestServices.GetService<ExternalAuthService>().GetExternalUserInfo(providerName, authCode)`

# Frontend
## To get AuthCode:
- Create oAuthCode handlers, e.g.
  ```
    function externalAuthSuccess(provider, code) {
        alert(`Provider: ${provider}, code: ${code}`);
    }
    function externalAuthError(provider, error, errorDescription) {
        alert(`Provider: ${provider}, error: ${error}, ${errorDescription}`);
    }
	```
- Subscribe to messages on a window: ```window.addEventListener("message", this.oAuthCodeReceived, false);``` and provide oAuthCodeReceived implementation like:
```
    function oAuthCodeReceived(message) {
        if (message.data && message.data.type === 'oauth-result') {
            if (data.code) {
                externalAuthSuccess(data.provider, data.code);
            } else {
                externalAuthError(data.provider, data.error, data.errorDescription);
            }
        }
    }
```

- Open authentication dialog in new window pointing to `http://YOUR_BACKEND_HOST/external-auth/challenge?provider=${provider}`. E.g.:
```window.open(`${window.location.protocol}//${window.location.hostname}:${window.location.port}/external-auth/challenge?provider=${provider}`, undefined, 'toolbar=no,menubar=no,directories=no,status=no,width=800,height=600');```
- When authentication succeeds/errors, your callback functions (externalAuthSuccess/externalAuthError) will be called.

## To authenticate (get access_token) using IdentityServer
- Get AuthCode (see above)
- Call 
 ```
    fetch(`/connect/token`,
                {
                    method: 'POST',
                    body: `grant_type=external&scope=api1&provider=${provider}&code=${code}`,
                    headers: {
                        'Authorization': `Basic Y2xpZW50OnNlY3JldA==`, //base64 encoded 'client:secret'
                        'Content-Type': 'application/x-www-form-urlencoded',
                    }
                })
```
to obtain access_token, that you could later use in Authorization header.

# Identity Server integration
## Adding external grant (validate Auth Code and issue own JWT)
Typical scenario is that you use oAuth for authentication only, and then create the user in your local DB (via e.g. IdentityServer) and issue your own JWT with custom claims for later authorization.
This library perfectly supports this scenario in combination with [IdentityServer](https://docs.identityserver.io) using extension grants (https://docs.identityserver.io/en/latest/topics/grant_types.html#extension-grants).
To integrate with IdentityServer all you need to do is call
```services.AddIdentityServer().AddExtensionGrantValidator<ExternalAuthenticationGrantValidator<IdentityUser, string>>()```.
That will register an extension grant named `external` and you could authenticate from JS as [described above](#to-authenticate-get-access_token-using-identityserver)

### Configuration
By default, `ExternalAuthenticationGrantValidator` is configured to create new users automatically, when they successfully authenticate via oAuth. You could change that behavior by setting:
```services.ConfigureExternalAuth(Configuration, options => { options.CreateUserIfNotFound = false; });```.

### Customization
You could inherit from `ExternalAuthenticationGrantValidator<IdentityUser, string>` and provide your custom logic for any of the following methods:
- `CreateNewUser` - fill-in the fields of new User based on your business requirements and/or information received from oAuth provider
- `CreateResultForLocallyNotFoundUser` - here you could write your own business logic, regarding what to do when the user is logging in for the first time. You could write custom logic for user creation, or deny some users (based on email/id) from logging in.
- `GetUserName` - most useful if you don't override `CreateNewUser`. You could provide Username for newly created users based on oAuth provider info

## External user storage
We use standard Asp.Net Identity mechanism to store external logins (namely, `AspNetUserLogins` table). To find a user by external OAuth id you need to use `_userManager.FindByLoginAsync(providerName, externalUserId)`
