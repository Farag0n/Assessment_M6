using Assessment_M6.Domain.Entities;
using Assessment_M6.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySqlConnector;

namespace Assessment_M6.Infrastructure.Data.Configurations;

public class DepartmentConfigurations : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(d => d.Description)
            .IsRequired()
            .HasMaxLength(200);
        
        builder
            .HasMany(d => d.Employees)      // Muchos empleados
            .WithOne(e => e.Department);    // Un departamento
    }
}