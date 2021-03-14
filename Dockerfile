FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine
USER root

WORKDIR /app
EXPOSE 80
COPY . .

ENTRYPOINT dotnet IdentityOAuthSpaExtensions.Example.dll
