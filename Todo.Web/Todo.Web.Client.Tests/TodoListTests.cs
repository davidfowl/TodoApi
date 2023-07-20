using AutoFixture;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RichardSzalay.MockHttp;
using StrawberryShake;
using Todo.Web.Client.Components;
using Todo.Web.Client.State;

namespace Todo.Web.Client.Tests;

public class TodoListTests : TestContext
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IGetTodosQuery> _getTodosQuery = new Mock<IGetTodosQuery>();
    
    public TodoListTests()
    {
        var mockHttpHandler = new MockHttpMessageHandler();
        var httpClient = mockHttpHandler.ToHttpClient();
        var httpClientFactory = new Mock<IHttpClientFactory>();
        Services.AddSingleton(httpClientFactory.Object);
        Services.AddSingleton(httpClient);
        Services.AddTodoClient().ConfigureInMemoryClient();
        Services.AddSingleton(_getTodosQuery.Object);
    }
    
    
    [Fact]
    public void RendersTodoItems()
    {
        /*
        Services
            .AddTodoClient()
            .ConfigureHttpClient(
                client => client.BaseAddress = new Uri("http://local.test"),
                buider => buider.AddHttpMessageHandler<ProxyHttpMessageHandler>());

        var accessor = Services.GetRequiredService<TodoClientStoreAccessor>();
        accessor.EntityStore.Update(session =>
        {
            Console.WriteLine("sessions");
        });
        */

        var todos = _fixture.Build<GetTodos_Todos_Todo>().CreateMany().ToArray();
        var result = new GetTodosResult(todos);
        var resultFactory = new Mock<IOperationResultDataFactory<IGetTodosResult>>();

        Services.AddSingleton(resultFactory.Object);

        resultFactory.Setup(z => z.Create(It.IsAny<IOperationResultDataInfo>(), default)).Returns(result);
        var operationResult = new OperationResult<IGetTodosResult>(result, default, resultFactory.Object, default);

        _getTodosQuery.Setup(z => z.ExecuteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(operationResult);
        // observable push?

        var page = RenderComponent<TodoList>();

        var elements = page.FindAll(".list-group-item div");
        elements.Should().HaveCount(todos.Length);
        elements.Select(z => z.TextContent).Should().BeEquivalentTo(todos.Select(z => z.Title));
    }
}