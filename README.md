## Todo REST API with ASP.NET Core

[![CI](https://github.com/davidfowl/TodoApi/actions/workflows/ci.yaml/badge.svg)](https://github.com/davidfowl/TodoApi/actions/workflows/ci.yaml)

Todo REST API samples using ASP.NET Core minimal APIs. It showcases:
- Using EntityFramework and SQLite for data access
- JWT authentication
- User management with ASP.NET Core Identity
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

### Users and Authentication
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

## Optional

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
