using Microsoft.EntityFrameworkCore;
using TreeNodeWebApi.Configurations;
using TreeNodeWebApi.Models.Entities;

namespace TreeNodeWebApi.Data
{
    /// <summary>
    /// Контекст для работы с базой данных.
    /// </summary>
    public class ApplicationDbContext: DbContext
    {
        /// <summary>
        /// Древовидные сущности.
        /// </summary>
        public DbSet<TreeNode> TreeNodes { get; set; }

        /// <summary>
        /// Пользователи.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Рефреш токены.
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Создание контекста.
        /// </summary>
        /// <param name="options">Настройки контекста.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        /// <summary>
        /// Выполняет настройку базы данных при создании контекста.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new TreeNodeConfiguration());
        }
    }
}
