## Todo REST API with ASP.NET Core

[![CI](https://github.com/davidfowl/TodoApi/actions/workflows/ci.yaml/badge.svg)](https://github.com/davidfowl/TodoApi/actions/workflows/ci.yaml)

Todo REST API samples using ASP.NET Core minimal APIs. It showcases:
- Using EntityFramework and SQLite for data access
- JWT authentication
- OpenAPI support
- Rate Limiting
- Writing tests for your REST API

## Prerequisites

### .NET
1. [Install .NET 7](https://dotnet.microsoft.com/en-us/download)

### Database
1. Install the **dotnet-ef** tool: `dotnet tool install dotnet-ef -g`
1. Navigate to the TodoApi folder and run `dotnet ef database update` to create the database.
1. Learn more about [dotnet-ef](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

### Authentication
1. Run `dotnet user-jwts create --claim id=myid`
1. You should be able to use this JWT token to make requests to the endpoint
1. Learn more about [user-jwts](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-7.0#using-dotnet-user-jwts-to-improve-development-time-testing)
