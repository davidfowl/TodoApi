internal static class Extensions
{
    public static IResourceBuilder<ExecutableResource>? AddTodoDbMigration(this IDistributedApplicationBuilder builder)
    {
        IResourceBuilder<ExecutableResource>? migrateOperation = default;

        if (builder.ExecutionContext.IsRunMode)
        {
            var projectDirectory = Path.GetDirectoryName(new Projects.Todo_Api().ProjectPath)!;
            var dbDirectory = Path.Combine(projectDirectory, ".db");

            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);

                migrateOperation = builder.AddEfMigration<Projects.Todo_Api>("todo-db-migration");
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

    public static string GetProjectDirectory(this IResourceBuilder<ProjectResource> project) =>
        Path.GetDirectoryName(project.Resource.GetProjectMetadata().ProjectPath)!;
}
