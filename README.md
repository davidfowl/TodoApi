## Todo application with ASP.NET Core

[![CI](https://github.com/davidfowl/TodoApi/actions/workflows/ci.yaml/badge.svg)](https://github.com/davidfowl/TodoApi/actions/workflows/ci.yaml)

This is a Todo application that features:
- [**Todo.Web**](Todo.Web) - An ASP.NET Core hosted Blazor WASM front end application
- [**TodoApi**](TodoApi) - An ASP.NET Core REST API backend using minimal APIs

![image](https://user-images.githubusercontent.com/95136/204161352-bc54ccb7-32cf-49ba-a6f7-f46d0f2d204f.png)

It showcases:
- Blazor WebAssembly
- Minimal APIs
- Using EntityFramework and SQLite for data access
- OpenAPI
- User management with ASP.NET Core Identity
- Cookie authentication
- JWT authentication
- Proxying requests from the front end application server using YARP's IHttpForwarder
- Rate Limiting
- Writing integration tests for your REST API

## Prerequisites

### .NET
1. [Install .NET 7](https://dotnet.microsoft.com/en-us/download)

### Database
1. Install the **dotnet-ef** tool: `dotnet tool install dotnet-ef -g`
1. Navigate to the `TodoApi` folder.
    1. Run `mkdir .db` to create the local database folder.
    1. Run `dotnet ef database update` to create the database.
1. Learn more about [dotnet-ef](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

### JWT 

1. To initialize the keys for JWT generation, run `dotnet user-jwts` in to [TodoApi](TodoApi) folder:

    ```
    dotnet user-jwts create
    ```

### Running the application
To run the application, run both the [Todo.Web/Server](Todo.Web/Server) and [TodoApi](TodoApi). Below are different ways to run both applications:
   - **Visual Studio** - Setup multiple startup projects by right clicking on the solution and selecting Properties. Select `TodoApi` and `Todo.Web.Server` as startup projects.

      <img width="591" alt="image" src="https://user-images.githubusercontent.com/95136/204311327-479445c8-4f73-4845-b146-d56be8ceb9ab.png">

   - **Visual Studio Code** - Open up 2 terminal windows, one in [Todo.Web.Server](Todo.Web/Server/) and the other in [TodoApi](TodoApi/) run: 
   
      ```
      dotnet watch run -lp https
      ```

      This will run both applications with the `https` profile.

   - **Tye** - Install the global tool using the following command: 
   
      ```
      dotnet tool install --global Microsoft.Tye --version 0.11.0-alpha.22111.1
      ```

      Run `tye run` in the repository root and navigate to the tye dashboard (usually http://localhost:8000) to see both applications running.

   - **Docker Compose** - Open your terminal, navigate to the root folder of this project and run the following commands:
      1. Build a docker image for the `TodoApi` directly from dotnet publish.
         ```
         dotnet publish ./TodoApi/TodoApi.csproj --os linux --arch x64 /t:PublishContainer -c Release
         ```

      1. Build a docker image for the `Todo.Web.Server` directly from dotnet publish.
         ```
         dotnet publish ./Todo.Web/Server/Todo.Web.Server.csproj --os linux --arch x64 /t:PublishContainer -c Release --self-contained true
         ```

      1. Generate certificate and configure local machine so we can start our apps with https support using docker compose.

         Windows using Linux containers
         ```
         set PASSWORD YourPasswordHere
         dotnet dev-certs https -ep ${HOME}/.aspnet/https/todoapps.pfx -p $PASSWORD --trust
         ```
         macOS or Linux
         ```
         export PASSWORD=YourPasswordHere
         dotnet dev-certs https -ep ~/.aspnet/https/todoapps.pfx -p $PASSWORD --trust
         ```

      1. Change these variables below in the `docker-compose.yml` file to match your https certificate and password.
         - `ASPNETCORE_Kestrel__Certificates__Default__Password`
         - `ASPNETCORE_Kestrel__Certificates__Default__Path`

      1. Run `docker-compose up -d` to spin up both apps todo-api and todo-web-server plus jaeger and prometheus.

      1. Navigate to the Todo Web app https://localhost:5003.


## Optional

### Using the API standalone
The Todo REST API can run standalone as well. You can run the [TodoApi](TodoApi) project and make requests to various endpoints using the Swagger UI (or a client of your choice):

<img width="1200" alt="image" src="https://user-images.githubusercontent.com/95136/204315486-86d25a5f-1164-467a-9891-827343b9f0e8.png">

Before executing any requests, you need to create a user and get an auth token.

1. To create a new user, run the application and POST a JSON payload to `/users` endpoint:

    ```json
    {
      "username": "myuser",
      "password": "<put a password here>"
    }
    ```
1. To get a token for the above user run `dotnet user-jwts` to create a JWT token with the same user name specified above e.g:

    ```
    dotnet user-jwts create -n myuser
    ```
1. You should be able to use this token to make authenticated requests to the todo endpoints.
1. Learn more about [user-jwts](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-7.0#using-dotnet-user-jwts-to-improve-development-time-testing)

### Social authentication

In addition to username and password, social authentication providers can be configured to work with this todo application. By default 
it supports Github, Google, and Microsoft accounts.

Instructions for setting up each of these providers can be found at:
- [Github](https://docs.github.com/en/developers/apps/building-oauth-apps)
- [Microsoft](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/microsoft-logins)
- [Google](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins)

Once you obtain the client id and client secret, the configuration for these providers must be added with the following schema:

```JSON
{
    "Authentication": {
        "Schemes": {
            "<scheme>": {
                "ClientId": "xxx",
                "ClientSecret": "xxxx"
            }
        }
    }
}
```

Or using environment variables:

```
Authentication__Schemes__<scheme>__ClientId=xxx
Authentication__Schemes__<scheme>__ClientSecret=xxx
```

Or using user secrets:

```
dotnet user-secrets set Authentication:Schemes:<scheme>:ClientId xxx
dotnet user-secrets set Authentication:Schemes:<scheme>:ClientSecret xxx
```

Other providers can be found [here](https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers#providers). 
These must be added to [AuthenticationExtensions](Todo.Web/Server/Authentication/AuthenticationExtensions.cs) as well.

**NOTE: Don't store client secrets in configuration!**

### External Authentication servers

Much like social authentication, this application also supports external [Open ID connect (OIDC) servers](https://en.wikipedia.org/wiki/OpenID). External authentication
is treated like social authentication provider but that also produce access tokens that can be used with the [TodoAPI](TodoAPI). This
needs to be configured like a social provider in the [Todo.Web.Server](Todo.Web/Server) application and an additional authentication scheme
needs to be configured in the [TodoAPI](TodoAPI) to accept JWT tokens issued by the auth server.

Here's what the flow looks like:

![image](https://user-images.githubusercontent.com/95136/208310479-808ea1ed-db48-49d1-b466-3ba33a08bcbc.png)

Here's how you would configure authentication:

```JSON
{
  "Authentication": {
    "Schemes": {
      "<scheme>": {
      }
    }
  }
}
```

**NOTE: Don't store client secrets in configuration!**

#### Auth0

This sample has **Auth0** configured as an OIDC server. It can be configured with the following schema:

```JSON
{
  "Authentication": {
    "Schemes": {
      "Auth0": {
        "Audience": "<audience>",
        "Domain": "<domain>",
        "ClientId": "<client id>",
        "ClientSecret": "<client secret>"
      }
    }
  }
}
```

Here's an example of configuring the [TodoAPI](TodoAPI):

```JSON
{
  "Authentication": {
    "Schemes": {
      "Auth0": {
        "ValidAudiences": [ "<your audience here>" ],
        "Authority": "<your authority here>"
      }
    }
  }
}
```

Learn more about the Auth0 .NET SDK [here](https://github.com/auth0/auth0-aspnetcore-authentication).

### OpenTelemetry
TodoApi uses OpenTelemetry to collect logs, metrics and spans.

If you wish to view the collected telemetry, follow the steps below.

#### Metrics
1. Run Prometheus with Docker:
```
docker run -d -p 9090:9090 --name prometheus -v $PWD/prometheus.yml:/etc/prometheus/prometheus.yml prom/prometheus
```
1. Open [Prometheus in your browser](http://localhost:9090/)
1. Query the collected metrics

#### Spans

1. Configure environment variable `OTEL_EXPORTER_OTLP_ENDPOINT` with the right endpoint URL to enable `.AddOtlpExporter` below `builder.Services.AddOpenTelemetryTracing`, in the `TodoApi/OpenTelemetryExtensions.cs` file
1. Run Jaeger with Docker:

```
docker run -d --name jaeger -e COLLECTOR_ZIPKIN_HOST_PORT=:9411 -e COLLECTOR_OTLP_ENABLED=true -p 6831:6831/udp -p 6832:6832/udp -p 5778:5778 -p 16686:16686 -p 4317:4317 -p 4318:4318 -p 14250:14250 -p 14268:14268 -p 14269:14269 -p 9411:9411 jaegertracing/all-in-one:latest
```

1. Open [Jaeger in your browser](http://localhost:16686/)
1. View the collected spans

### Logs

This app using `structured logging` and for this purpose we use [Serilog](https://github.com/serilog/serilog-aspnetcore)

For setting up Serilog you should call `AddSerilog` on [SerilogExtensions](TodoApi/Extensions/SerilogExtensions.cs) class and Add `Serilog` section with appropriate [Options](TodoApi/Extensions/SerilogExtensions.cs#L95)

```json
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    }
  }
```

For collecting and searching logs there are 2 `optional` way, based on `your needs` in the production environment: 
- Seq (Not Free in commercial use)
- Elasticsearch and Kibana (Free)

#### Seq
For using seq, we should enable it with setting `SeqUrl` value in the `Serilog` section of [appsettings.json](TodoApi/appsettings.json):

``` json
  "Serilog": {
     ...
    "SeqUrl": "http://localhost:5341",
     ...
  },
```
Also we should run [seq server](docker-compose.yml#47) on docker-compose file, now seq is available on [http://localhost:8081](http://localhost:8081) and we can see logs out there.

#### Elasticsearch and Kibana
For using elasticsearch and kibana, we should enable it with setting `ElasticSearchUrl` value in the `Serilog` section of [appsettings.json](TodoApi/appsettings.json):

```json
  "Serilog": {
     ...
    "ElasticSearchUrl": "http://localhost:9200",
     ...
  }
```
Also we should run [Elasticsearch](docker-compose.yml#84) and [Kibana](docker-compose.yml#104) on docker-compose file, now we can see our logs on kibana url [http://localhost:5601](http://localhost:5601) and 
index name `todoapi`.