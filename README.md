# Spry.Identity

Identity server for Petra and Spry systems based on OpenIdConnect

## Usage
Application configuration can be found in the docker.compose file.

OpenIdConnect configuration can found at **http://[applicationUrl]/.well-known/openid-configuration**

To access the database use this link [PgAdmin](http://host.docker.internal:5103/browser/) using the credentials found in the compose file

For first time access, Setup access to the database with connection details

- hostname/address: **postgres.data**
- port: 5432
- username: postgres
- password: spry-password

To run the project without compose, navigate to **Spry.AuthServer** in the root folder of project, use **dotnet run** and **dotnet build**
 to run and build the project respectively.

 Dependencies
 - RabbitMq
 - Redis
 - Postgres

## Framework 
- Asp.NetCore Razor pages and Api
- Dotnet SDK version - v8

## Useful resources
[OpenId connect specification](https://openid.net/specs/openid-connect-core-1_0.html)

[OAuth 2.0 and OpenID Connect (in plain English)](https://youtu.be/996OiexHze0?si=wrv7HVSygI7aJtnr)