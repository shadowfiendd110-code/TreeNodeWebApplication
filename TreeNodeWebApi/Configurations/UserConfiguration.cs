using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreeNodeWebApi.Models.Entities;

namespace TreeNodeWebApi.Configurations
{
    /// <summary>
    /// Конфигуратор сущности Пользователь.
    /// </summary>
    public class UserConfiguration: IEntityTypeConfiguration<User>
    {
        /// <summary>
        /// Конфигурирует сущность Пользователь.
        /// </summary>
        /// <param name="userBuilder">Конфигуратор пользователя.</param>
        public void Configure(EntityTypeBuilder<User> userBuilder)
        {
            userBuilder
                .ToTable("Users", t =>
                {
                    t.HasCheckConstraint("CK_Users_Email_Format", "[Email] LIKE '%@%.%'");
                    t.HasCheckConstraint("CK_Users_Role_Valid", "[Role] IN ('User', 'Admin', 'Moderator')");
                })
                .HasKey(u => u.Id);

            userBuilder
            .Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(50);

            userBuilder
            .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            userBuilder
            .Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(256);

            userBuilder
            .Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(256)
                .HasDefaultValue("User");

            userBuilder.HasMany(u => u.RefreshTokens)
                   .WithOne(rt => rt.User)
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            userBuilder
                .HasIndex(u => u.Email)
                .IsUnique();

            userBuilder
                .HasIndex(u => u.UserName);

            userBuilder
                .HasIndex(u => u.Role);
        }
    }
}
