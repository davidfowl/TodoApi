public static class Results
{
    public static IResult NotFound() => new StatusCodeResult(404);
    public static IResult Ok() => new StatusCodeResult(200);
    public static IResult Status(int statusCode) => new StatusCodeResult(statusCode);
    public static IResult Ok(object value) => new JsonResult(value);
    public static IResult CreatedAt<T>(T value, string endpointName, object values) => new CreatedAtRouteResult<T>(value, endpointName, values);

    public class CreatedAtRouteResult<T> : IResult
    {
        private readonly T _value;
        private readonly string _endpointName;
        private readonly object _values;

        public CreatedAtRouteResult(T value, string endpointName, object values)
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