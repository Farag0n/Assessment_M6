using Assessment_M6.Domain.Entities;
using Assessment_M6.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySqlConnector;

namespace Assessment_M6.Infrastructure.Data.Configurations;

public class UserConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        //Se le especifica a la db que la tabla tiene una llave y es u. Id
        builder.HasKey(u => u.Id);
        
        //valor único
        builder.HasIndex(u => u.Username).IsUnique();
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100);
        
        //valor único
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .IsRequired();
        
        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);
        
        //Usuario admin de prueba 
        //No es una buena practica en entor de produccion pero para pruebas en desarrollo creo que es valido
        builder.HasData(new User
        {
            Id = 1,
            Username = "test",
            Email = "test@qwe.com",
            Role = UserRole.Admin,
            PasswordHash = "123"
        });
    }
}