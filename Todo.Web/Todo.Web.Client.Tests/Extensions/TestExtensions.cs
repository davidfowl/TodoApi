using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using Bunit;

namespace Todo.Web.Client.Tests.Extensions;

internal static class TestExtensions
{
    public static async Task WithSnapshot(
            this HttpResponseMessage httpResponseMessage,
            int? index = null,
            [CallerFilePath] string testFilePath = "",
            [CallerMemberName] string testName = "")
    {
        var content = await GetContent(index, testFilePath, testName);
        httpResponseMessage.Content = new StringContent(content);
    }

    public static Func<Task<HttpResponseMessage>> WithSnapshot(
        this TestContext _, // Need to refection
        HttpStatusCode? statusCode = null,
        int? index = null,
        [CallerFilePath] string testFilePath = "",
        [CallerMemberName] string testName = "")
    {
        var value = async () =>
        {
            var httpResponse = new HttpResponseMessage(statusCode ?? HttpStatusCode.OK);
            var content = await GetContent(index, testFilePath, testName);
            httpResponse.Content = new StringContent(content);
            return httpResponse;
        };

        return value;
    }

    private static async Task<string> GetContent(int? index, string testFilePath, string testName)
    {
        var directoryName = Path.GetDirectoryName(testFilePath);
        Debug.Assert(directoryName != null);

        var fileName = Path.GetFileNameWithoutExtension(testFilePath);
        var path = $"{fileName}-{testName}";
        if (index.HasValue)
        {
            path = $"{path}-{index.Value}";
        }
        var fullPath = Path.Combine(directoryName, "Snapshots", $"{path}.json");

        await using var stream = new FileStream(fullPath, FileMode.Open);
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        return content;
    }
}

