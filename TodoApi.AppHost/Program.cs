var builder = DistributedApplication.CreateBuilder(args);

var migrateOperation = builder.AddTodoDbMigration();

var todoapi = builder.AddProject<Projects.Todo_Api>("todoapi");

builder.AddProject<Projects.Todo_Web_Server>("todo-web-server")
       .WithReference(todoapi);

var dbDirectory = Path.Combine(todoapi.GetProjectDirectory(), ".db");

// Add sqlite-web to view the Todos.db database
var sqliteWeb = builder.AddContainer("sqliteweb", "tomdesinto/sqliteweb")
       .WithHttpEndpoint(targetPort: 8080)
       .WithBindMount(dbDirectory, "/db")
       .WithArgs("Todos.db")
       .ExcludeFromManifest();

if (migrateOperation is not null)
{
    // Wait for the migration to complete before running the api and ui
    todoapi.WaitForCompletion(migrateOperation);

    sqliteWeb.WaitForCompletion(migrateOperation);
}


builder.Build().Run();
