using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http;

internal sealed class ProducesResponseTypeMetadata : IProducesResponseTypeMetadata
{
    private readonly IEnumerable<string> _contentTypes;

    public ProducesResponseTypeMetadata(int statusCode)
        : this(typeof(void), statusCode, Enumerable.Empty<string>())
    {
    }

    public ProducesResponseTypeMetadata(Type type, int statusCode)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        StatusCode = statusCode;
        _contentTypes = Enumerable.Empty<string>();
    }

    public ProducesResponseTypeMetadata(int statusCode, string contentType)
    {
        StatusCode = statusCode;
        MediaTypeHeaderValue.Parse(contentType);
        _contentTypes = GetContentTypes(contentType, Array.Empty<string>());
    }

    public ProducesResponseTypeMetadata(Type type, int statusCode, string contentType, params string[] additionalContentTypes)
    {
        if (contentType == null)
        {
            throw new ArgumentNullException(nameof(contentType));
        }

        Type = type ?? throw new ArgumentNullException(nameof(type));
        StatusCode = statusCode;

        MediaTypeHeaderValue.Parse(contentType);
        for (var i = 0; i < additionalContentTypes.Length; i++)
        {
            MediaTypeHeaderValue.Parse(additionalContentTypes[i]);
        }

        _contentTypes = GetContentTypes(contentType, additionalContentTypes);
    }

    // Only for internal use where validation is unnecessary.
    private ProducesResponseTypeMetadata(Type? type, int statusCode, IEnumerable<string> contentTypes)
    {

        Type = type;
        StatusCode = statusCode;
        _contentTypes = contentTypes;
    }

    /// <summary>
    /// Gets or sets the type of the value returned by an action.
    /// </summary>
    public Type? Type { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    public int StatusCode { get; set; }

    public IEnumerable<string> ContentTypes => _contentTypes;

    //internal static ProducesResponseTypeMetadata CreateUnvalidated(Type? type, int statusCode, IEnumerable<string> contentTypes) => new(type, statusCode, contentTypes);

    private static List<string> GetContentTypes(string contentType, string[] additionalContentTypes)
    {
        var contentTypes = new List<string>(additionalContentTypes.Length + 1);
        ValidateContentType(contentType);
        contentTypes.Add(contentType);
        foreach (var type in additionalContentTypes)
        {
            ValidateContentType(type);
            contentTypes.Add(type);
        }

        return contentTypes;

        static void ValidateContentType(string type)
        {
            if (type.Contains('*', StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Could not parse '{type}'. Content types with wildcards are not supported.");
            }
        }
    }
}