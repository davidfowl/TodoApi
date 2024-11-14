var builder = DistributedApplication.CreateBuilder(args);

var migrateOperation = builder.AddTodoDbMigration();

var todoapi = builder.AddProject<Projects.TodoApi>("todoapi");

if (migrateOperation is not null)
{
    // Wait for the migration to complete before running the api
    todoapi.WaitForCompletion(migrateOperation);
}

builder.AddProject<Projects.Todo_Web_Server>("todo-web-server")
       .WithReference(todoapi);

builder.Build().Run();
