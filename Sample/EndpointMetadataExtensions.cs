namespace Microsoft.AspNetCore.Builder
{
    public static class EndpointMetadataExtensions
    {
        public static TBuilder WithName<TBuilder>(this TBuilder builder, string endpointName) where TBuilder : IEndpointConventionBuilder
        {
            return builder.WithMetadata(new EndpointNameMetadata(endpointName));
        }
    }
}
