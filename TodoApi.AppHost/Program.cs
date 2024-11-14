var builder = DistributedApplication.CreateBuilder(args);

var todoapi = builder.AddProject<Projects.TodoApi>("todoapi");

builder.AddProject<Projects.Todo_Web_Server>("todo-web-server")
       .WithReference(todoapi);

builder.Build().Run();
