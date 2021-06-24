// TODO: https://github.com/dotnet/aspnetcore/issues/33729

namespace Microsoft.AspNetCore.Http
{
    public static class Results
    {
        public static IResult NotFound() => new StatusCodeResult(404);
        public static IResult Ok() => new StatusCodeResult(200);
        public static IResult Status(int statusCode) => new StatusCodeResult(statusCode);
        public static IResult Ok(object value) => new JsonResult(value);
        public static IResult CreatedAt(object value, string endpointName, object values) => new CreatedAtRouteResult(value, endpointName, values);
        public static IResult Created(string location, object value) => new CreatedResult(location, value);

        private class CreatedResult : IResult
        {
            private readonly object _value;
            private readonly string _location;

            public CreatedResult(string location, object value)
            {
                _location = location;
                _value = value;
            }

            public Task ExecuteAsync(HttpContext httpContext)
            {
                httpContext.Response.StatusCode = StatusCodes.Status201Created;
                httpContext.Response.Headers.Location = _location;

                return httpContext.Response.WriteAsJsonAsync(_value);
            }
        }

        private class CreatedAtRouteResult : IResult
        {
            private readonly object _value;
            private readonly string _endpointName;
            private readonly object _values;

            public CreatedAtRouteResult(object value, string endpointName, object values)
            {
                _value = value;
                _endpointName = endpointName;
                _values = values;
            }

            public Task ExecuteAsync(HttpContext httpContext)
            {
                var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();

                httpContext.Response.StatusCode = StatusCodes.Status201Created;
                httpContext.Response.Headers.Location = linkGenerator.GetPathByName(_endpointName, _values);

                return httpContext.Response.WriteAsJsonAsync(_value);
            }
        }
    }
}