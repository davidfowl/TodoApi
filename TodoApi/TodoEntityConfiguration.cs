using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TodoEntityConfiguration:IEntityTypeConfiguration<Todo>
{
    public void Configure(EntityTypeBuilder<Todo> builder)
    {
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Title).IsRequired();
        builder.Property(q => q.OwnerId).IsRequired();
    }
}