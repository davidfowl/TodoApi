using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Todo.Web.Client.Components;
using Todo.Web.Client.Tests.Extensions;

namespace Todo.Web.Client.Tests.Components;

public sealed class TodoListTests : TestContext
{
    [Fact]
    public async Task RendersTodoItems()
    {
        var getResponse = this.WithSnapshot();

        Services
            .AddTodoClient()
            .ConfigureHttpClient(
                client => client.BaseAddress = new Uri("http://test/graphql"),
                builder => builder.ConfigurePrimaryHttpMessageHandler(
                    ProxyHttpMessageHandler.Create(async _ => await getResponse())));

        var page = RenderComponent<TodoList>();

        var client = Services.GetRequiredService<TodoClient>();
        await client.GetTodos.ExecuteAsync();

        var elements = page.FindAll(".list-group-item div");
        elements.Should().HaveCount(3);
    }
}