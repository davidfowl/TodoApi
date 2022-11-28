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
1. Navigate to the `TodoApi` folder and run `dotnet ef database update` to create the database.
1. Learn more about [dotnet-ef](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

### Running the application
To run the application, run both the [Todo.Web/Server](Todo.Web/Server) and [TodoApi](TodoApi). Below are different ways to run both applications:
   - **Visual Studio** - Setup multiple startup projects by right clicking on the solution and selecting Properties. Select `TodoApi` and `Todo.Web.Server` as startup projects.

      <img width="591" alt="image" src="https://user-images.githubusercontent.com/95136/204311327-479445c8-4f73-4845-b146-d56be8ceb9ab.png">
   - **Visual Studio Code** - Open up 2 terminal windows, one in [Todo.Web.Server](Todo.Web/Server/) and the other in [TodoApi](TodoApi/) run `dotnet watch run -lp https`.
   - **Tye** - Install the global tool using the following command: 
   
      ```
      dotnet tool install --global Microsoft.Tye --version 0.11.0-alpha.22111.1
      ```

      Run `tye run` in the repository root and navigate to the tye dashboard (usually http://localhost:8080) to see both applications running.


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

1. Uncomment `.AddOtlpExporter` below `builder.Services.AddOpenTelemetryTracing`, in the `TodoApi/OpenTelemetryExtensions.cs` file
1. Run Jaeger with Docker:

```
docker run -d --name jaeger -e COLLECTOR_ZIPKIN_HOST_PORT=:9411 -e COLLECTOR_OTLP_ENABLED=true -p 6831:6831/udp -p 6832:6832/udp -p 5778:5778 -p 16686:16686 -p 4317:4317 -p 4318:4318 -p 14250:14250 -p 14268:14268 -p 14269:14269 -p 9411:9411 jaegertracing/all-in-one:latest
```

1. Open [Jaeger in your browser](http://localhost:16686/)
1. View the collected spans

#### Logs

1. Uncomment `.AddOtlpExporter` below `builder.Logging.AddOpenTelemetry`, in the `TodoApi/Extensions/OpenTelemetryExtensions.cs` file
1. Find a Vendor that supports OpenTelemetry-based logging.

Vendor support for OpenTelemetry-based logging is currently very limited.
