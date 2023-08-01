using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using AutoFixture;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Todo.Web.Client.Components;
using Todo.Web.Client.Tests.Extensions;

namespace Todo.Web.Client.Tests.Components;

public sealed class TodoListTests : TestContext
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public async Task RendersTodoItems()
    {
        var todos = _fixture.Build<GetTodos_Todos_Todo>().CreateMany().ToArray();

        List<JsonNode> nodes = new();
        foreach (var todo in todos)
        {
            var template = $"{{ " +
                $"\"__typename\": \"Todo\",  " +
                $"\"id\": {todo.Id}, " +
                $"\"isComplete\": {todo.IsComplete.ToString().ToLowerInvariant()}, " +
                $"\"title\": \"{todo.Title}\" " +
                "}";

            var node = JsonNode.Parse(template)!;
            nodes.Add(node);
        }
        var array = new JsonArray(nodes.ToArray());
        var document = JsonDocument.Parse($"{{\"data\": {{ \"todos\": {array} }} }}");
        var paylaod = document.RootElement.GetRawText();

        Services
            .AddTodoClient()
            .ConfigureHttpClient(
                client => client.BaseAddress = new Uri("http://test/graphql"),
                builder => builder.ConfigurePrimaryHttpMessageHandler(
                    ProxyHttpMessageHandler.Create(request =>
                    {
                        return Task.FromResult<HttpResponseMessage?>(
                            new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(paylaod)
                            });
                    })));

        var page = RenderComponent<TodoList>();

        var client = Services.GetRequiredService<TodoClient>();
        await client.GetTodos.ExecuteAsync();

        var elements = page.FindAll(".list-group-item div");
        elements.Should().HaveCount(3);
    }
}