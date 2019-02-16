# IdentityOAuthSpaExtensions
.Net Core library that allows easy integration of external OAuth providers into your SPA. It has even more perks if you use IdentityServer.

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

You could also take a look at IdentityOAuthSpaExtensions.Example for example usage (keep in mind, that there are hardcoded ClientId/ClientSecret for FB and Google within Example app. They are for demo purposes and everyone can use them, so beware).

## Backend
From `ConfigureServices` call `services.ConfigureExternalAuth(Configuration)`.

That's it.

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
- Subscribe to messages on a window: `window.addEventListener("message", this.oAuthCodeReceived, false);` and provide oAuthCodeReceived implementation like:
    ```function oAuthCodeReceived(message) {
        if (message.data) {
            let data = JSON.parse(message.data);
            if (data.type === 'oauth-result') {
                if (data.code) {
                    externalAuthSuccess(data.provider, data.code);
                } else {
                    externalAuthError(data.provider, data.error, data.errorDescription);
                }
            }
        }
    }```

- Open authentication dialog in new window pointing to `http://YOUR_BACKEND_HOST/external-auth/challenge?provider=${provider}. E.g.:
```window.open(`${window.location.protocol}//${window.location.hostname}:${window.location.port}/external-auth/challenge?provider=${provider}`, undefined, 'width=800,height=600');```
- When authentication succeeds/errors, your callback functions (externalAuthSuccess/externalAuthError) will be called.

## To authenticate (get access_token) using IdentityServer
- Get AuthCode (see above)
- Call `/connect/token?provider=${provider}&code=${code}&grant=external`. 


## External user storage
We use standard Asp.Net Identity mechanism to store external logins (namely, `AspNetUserLogins` table). To find a user by external OAuth id you need to use `_userManager.FindByLoginAsync(providerName, externalUserId)`
