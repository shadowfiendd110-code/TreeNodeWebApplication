using Microsoft.EntityFrameworkCore;
using TreeNodeWebApi.Data;
using TreeNodeWebApi.Models.Entities;
using FluentAssertions;
using TreeNodeWebApi.Repositories;

namespace Tests.Repositories
{
    /// <summary>
    /// Тесты для <see cref="UserRepository"/>.
    /// </summary>
    public class UserRepositoryTests : IDisposable
    {
        /// <summary>
        /// Контекст базы данных для тестирования.
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Экземпляр тестируемого репозитория пользователей.
        /// </summary>
        private readonly UserRepository _repository;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="UserRepositoryTests"/>.
        /// </summary>
        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new UserRepository(_context);
        }

        /// <summary>
        /// Освобождает ресурсы тестового класса.
        /// </summary>
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        /// <summary>
        /// Проверяет успешное нахождение пользователя по существующему email.
        /// </summary>
        [Fact]
        public async Task FindByEmail_ExistingEmail_ReturnsUser()
        {
            var user = new User
            {
                UserName = "Test User",
                Email = "test@example.com"
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var result = await _repository.FindByEmail("test@example.com");

            result.Should().NotBeNull();
            result!.Email.Should().Be("test@example.com");
            result.UserName.Should().Be("Test User");
        }

        /// <summary>
        /// Проверяет, что при поиске по несуществующему email возвращается null.
        /// </summary>
        [Fact]
        public async Task FindByEmail_NonExistingEmail_ReturnsNull()
        {
            var result = await _repository.FindByEmail("nonexistent@example.com");

            result.Should().BeNull();
        }

        /// <summary>
        /// Проверяет чувствительность к регистру при поиске пользователя по email.
        /// </summary>
        [Fact]
        public async Task FindByEmail_CaseSensitiveEmail_ReturnsNullWhenCaseDiffers()
        {
            var user = new User
            {
                UserName = "Test",
                Email = "TEST@example.com"
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var resultLowercase = await _repository.FindByEmail("test@example.com");
            var resultUppercase = await _repository.FindByEmail("TEST@example.com");

            resultUppercase.Should().NotBeNull();
            resultLowercase.Should().BeNull();
        }

        /// <summary>
        /// Проверяет, что метод FindByEmail корректно работает при пустом email.
        /// </summary>
        [Fact]
        public async Task FindByEmail_EmptyEmail_ReturnsNull()
        {
            var result = await _repository.FindByEmail("");

            result.Should().BeNull();
        }

        /// <summary>
        /// Проверяет успешное добавление нового пользователя в базу данных.
        /// </summary>
        [Fact]
        public async Task AddUser_ValidUser_AddsToDatabase()
        {
            var user = new User
            {
                UserName = "New User",
                Email = "new@example.com"
            };

            var result = await _repository.AddUser(user);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.UserName.Should().Be("New User");
            result.Email.Should().Be("new@example.com");

            var dbUser = await _context.Users.FindAsync(result.Id);
            dbUser.Should().NotBeNull();
            dbUser!.UserName.Should().Be("New User");
            dbUser.Email.Should().Be("new@example.com");
        }

        /// <summary>
        /// Проверяет добавление пользователя с минимальными данными.
        /// </summary>
        [Fact]
        public async Task AddUser_MinimalUserData_AddsToDatabase()
        {
            var user = new User
            {
                Email = "minimal@example.com"
            };

            var result = await _repository.AddUser(user);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Email.Should().Be("minimal@example.com");
        }

        /// <summary>
        /// Проверяет добавление нескольких пользователей.
        /// </summary>
        [Fact]
        public async Task AddUser_MultipleUsers_AllAddedSuccessfully()
        {
            var user1 = new User { UserName = "User1", Email = "user1@example.com" };
            var user2 = new User { UserName = "User2", Email = "user2@example.com" };

            var result1 = await _repository.AddUser(user1);
            var result2 = await _repository.AddUser(user2);

            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            result1.Id.Should().NotBe(result2.Id);

            var allUsers = await _context.Users.ToListAsync();
            allUsers.Should().HaveCount(2);
        }

        /// <summary>
        /// Проверяет, что добавление пользователя с дублирующимся email возможно (если нет уникального ограничения).
        /// </summary>
        [Fact]
        public async Task AddUser_DuplicateEmail_AddsBothUsers()
        {
            var user1 = new User { UserName = "First", Email = "duplicate@example.com" };
            var user2 = new User { UserName = "Second", Email = "duplicate@example.com" };

            var result1 = await _repository.AddUser(user1);
            var result2 = await _repository.AddUser(user2);

            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            result1.Id.Should().NotBe(result2.Id);

            var usersWithEmail = await _context.Users
                .Where(u => u.Email == "duplicate@example.com")
                .ToListAsync();
            usersWithEmail.Should().HaveCount(2);
        }

        /// <summary>
        /// Проверяет, что FindByEmail возвращает null для базы без пользователей.
        /// </summary>
        [Fact]
        public async Task FindByEmail_EmptyDatabase_ReturnsNull()
        {
            var result = await _repository.FindByEmail("any@example.com");

            result.Should().BeNull();
        }

        /// <summary>
        /// Проверяет интеграцию между AddUser и FindByEmail.
        /// </summary>
        [Fact]
        public async Task Integration_AddUserThenFindByEmail_ReturnsSameUser()
        {
            var user = new User
            {
                UserName = "Integration Test User",
                Email = "integration@example.com"
            };

            var addedUser = await _repository.AddUser(user);
            var foundUser = await _repository.FindByEmail("integration@example.com");

            foundUser.Should().NotBeNull();
            foundUser!.Id.Should().Be(addedUser.Id);
            foundUser.UserName.Should().Be(addedUser.UserName);
            foundUser.Email.Should().Be(addedUser.Email);
        }

        /// <summary>
        /// Проверяет, что контекст базы данных не равен null.
        /// </summary>
        [Fact]
        public void Constructor_ValidContext_RepositoryInitialized()
        {
            _repository.Should().NotBeNull();
            _context.Should().NotBeNull();
        }

        /// <summary>
        /// Проверяет обработку null значения для email в FindByEmail.
        /// </summary>
        [Fact]
        public async Task FindByEmail_NullEmail_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.FindByEmail(null!));
        }

        /// <summary>
        /// Проверяет добавление пользователя с null значением.
        /// </summary>
        [Fact]
        public async Task AddUser_NullUser_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.AddUser(null!));
        }
    }
}
