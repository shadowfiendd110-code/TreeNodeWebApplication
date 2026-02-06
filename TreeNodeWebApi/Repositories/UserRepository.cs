using Microsoft.EntityFrameworkCore;
using TreeNodeWebApi.Data;
using TreeNodeWebApi.Exceptions;
using TreeNodeWebApi.Models.Entities;

namespace TreeNodeWebApi.Repositories
{
    /// <summary>
    /// Репозиторий для работы с пользователями.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Добавляет пользователя в БД.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <returns>Пользователя.</returns>
        Task<User> AddUser(User user);

        /// <summary>
        /// Ищет пользователя по имени.
        /// </summary>
        /// <param name="email">Имя пользователя.</param>
        /// <returns>Пользователя.</returns>
        Task<User?> FindByEmail(string email);
    }

    /// <summary>
    /// Репозиторий для работы с пользователями.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        /// <summary>
        /// Контекст для работы с БД.
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Создание репозитория.
        /// </summary>
        /// <param name="context">Контекст для работы с БД.</param>
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ищет пользователя по email.
        /// </summary>
        /// <param name="email">Email пользователя.</param>
        /// <returns>Пользователя или null, если не найден.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если email равен null.</exception>
        public async Task<User?> FindByEmail(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }

            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Добавляет пользователя в БД.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <returns>Пользователя.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если user равен null.</exception>
        public async Task<User> AddUser(User user)
        {
            // ВАЖНО: Добавьте эту проверку
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }
    }
}
