using Assessment_M6.Domain.Entities;
using Assessment_M6.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySqlConnector;

namespace Assessment_M6.Infrastructure.Data.Configurations;

public class EmployeeConfigurations : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(e => e.DocNumber).IsUnique();
        builder.Property(e => e.DocNumber)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.PhoneNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.State).IsRequired();
        builder.Property(e => e.RegisteredAt).IsRequired();
        
        builder
            .HasOne(e => e.Department)          
            .WithMany(d => d.Employees)         
            .HasForeignKey(e => e.DepartmentId) // FK
            .OnDelete(DeleteBehavior.Restrict); 
    }
}