using System.Xml.Linq;

namespace TreeNodeWebApi.Models.Entities
{
    /// <summary>
    /// Пользователь.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Id пользователя.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Хэш пароля пользователя.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Email пользователя.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Роль пользователя.
        /// </summary>
        public string Role { get; set; } = "User";

        /// <summary>
        /// Время создания пользователя.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Рефреш токены пользователя.
        /// </summary>
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        /// <summary>
        /// Создание пользователя.
        /// </summary>
        /// <param name="userName">Имя пользователя.</param>
        /// <param name="email">E-mail пользователя.</param>
        /// <param name="role">Роль пользователя.</param>
        /// <param name="passwordHash">Хэш пароля.</param>
        public User(string userName, string email, string role, string passwordHash)
        {
            UserName = userName;
            Email = email;
            Role = role;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public User() { }

    }
}
