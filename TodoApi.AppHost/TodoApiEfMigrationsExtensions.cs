internal static class TodoApiEfMigrationsExtensions
{
    public static IResourceBuilder<ExecutableResource>? AddTodoDbMigration(this IDistributedApplicationBuilder builder)
    {
        IResourceBuilder<ExecutableResource>? migrateOperation = default;

        if (builder.ExecutionContext.IsRunMode)
        {
            var projectDirectory = Path.GetDirectoryName(new Projects.TodoApi().ProjectPath)!;
            var dbDirectory = Path.Combine(projectDirectory, ".db");

            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);

                migrateOperation = builder.AddEfMigration<Projects.TodoApi>("todo-db-migration");
            }
        }

        return migrateOperation;
    }

    public static IResourceBuilder<ExecutableResource> AddEfMigration<TProject>(this IDistributedApplicationBuilder builder, string name)
        where TProject : IProjectMetadata, new()
    {
        var projectDirectory = Path.GetDirectoryName(new TProject().ProjectPath)!;

        // TODO: Support passing a connection string
        return builder.AddExecutable(name, "dotnet", projectDirectory, "ef", "database", "update", "--no-build");
    }
}
