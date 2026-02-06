using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using TreeNodeWebApi.Models.Entities;
using TreeNodeWebApi.Services.Auth;

namespace Tests.Services
{
    /// <summary>
    /// Тесты для <see cref="TokenService"/>.
    /// </summary>
    public class TokenServiceTests
    {
        /// <summary>
        /// Экземпляр тестируемого сервиса токенов.
        /// </summary>
        private readonly TokenService _tokenService;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="TokenServiceTests"/>.
        /// </summary>
        public TokenServiceTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"] = "SuperLongTestSecretKeyForJWTSigning1234567890123456",
                    ["JwtSettings:Issuer"] = "TestIssuer",
                    ["JwtSettings:Audience"] = "TestAudience"
                })
                .Build();

            _tokenService = new TokenService(configuration);
        }

        /// <summary>
        /// Проверяет успешное создание JWT-токена для валидного пользователя.
        /// </summary>
        [Fact]
        public void GenerateToken_ValidUser_ReturnsTokenAndExpiration()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                UserName = "Иван Иванов",
                Email = "ivan@example.com",
                Role = "User"
            };

            // Act
            var (token, expiresAt) = _tokenService.GenerateToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();
            expiresAt.Should().BeAfter(DateTime.UtcNow);
            expiresAt.Should().BeBefore(DateTime.UtcNow.AddHours(1));
        }

        /// <summary>
        /// Проверяет, что токен для пользователя с ролью Admin содержит корректные claims.
        /// </summary>
        [Fact]
        public void GenerateToken_UserWithAdminRole_ContainsCorrectRoleClaim()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                UserName = "Админ",
                Email = "admin@example.com",
                Role = "Admin"
            };

            // Act
            var (token, _) = _tokenService.GenerateToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Проверяем role claim (используется полный URI)
            jwtToken.Claims.Should().Contain(c =>
                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                && c.Value == "Admin");

            // Проверяем email claim (используется полный URI)
            jwtToken.Claims.Should().Contain(c =>
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"
                && c.Value == "admin@example.com");
        }

        /// <summary>
        /// Проверяет, что при отсутствии секретного ключа выбрасывается исключение <see cref="InvalidOperationException"/>.
        /// </summary>
        [Fact]
        public void GenerateToken_MissingSecretKey_ThrowsException()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:Issuer"] = "Test",
                    ["JwtSettings:Audience"] = "Test"
                    // SecretKey отсутствует
                })
                .Build();

            var invalidService = new TokenService(configuration);
            var user = new User
            {
                Id = 1,
                UserName = "Test",
                Email = "test@test.com",
                Role = "User"
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                invalidService.GenerateToken(user));
        }

        /// <summary>
        /// Проверяет, что токен успешно создается при минимально допустимой длине секретного ключа (32 символа).
        /// </summary>
        [Fact]
        public void GenerateToken_MinimumLengthSecretKey_Works()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"] = "Exactly32CharactersLongSecretKey1234",
                    ["JwtSettings:Issuer"] = "Test",
                    ["JwtSettings:Audience"] = "Test"
                })
                .Build();

            var service = new TokenService(configuration);
            var user = new User
            {
                Id = 1,
                UserName = "Test",
                Email = "test@test.com",
                Role = "User"
            };

            // Act
            var (token, _) = service.GenerateToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Проверяет успешное создание валидного refresh-токена.
        /// </summary>
        [Fact]
        public void GenerateRefreshToken_ReturnsValidRefreshToken()
        {
            // Act
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Assert
            refreshToken.Should().NotBeNull();
            refreshToken.Token.Should().NotBeNullOrEmpty();
            refreshToken.Expires.Should().BeAfter(DateTime.UtcNow);
            refreshToken.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Проверяет, что при многократном вызове создаются разные refresh-токены.
        /// </summary>
        [Fact]
        public void GenerateRefreshToken_MultipleCalls_GenerateDifferentTokens()
        {
            // Act
            var token1 = _tokenService.GenerateRefreshToken();
            var token2 = _tokenService.GenerateRefreshToken();

            // Assert
            token1.Token.Should().NotBe(token2.Token);
        }

        /// <summary>
        /// Проверяет корректность времени истечения срока действия refresh-токена.
        /// </summary>
        [Fact]
        public void GenerateRefreshToken_TokenHasCorrectExpiration()
        {
            // Act
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Assert
            refreshToken.Expires.Should().BeCloseTo(
                DateTime.UtcNow.AddDays(30),
                TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Проверяет, что токен содержит идентификатор пользователя.
        /// </summary>
        [Fact]
        public void GenerateToken_ContainsUserIdClaim()
        {
            // Arrange
            var userId = 42;
            var user = new User
            {
                Id = userId,
                UserName = "Test User",
                Email = "test@example.com",
                Role = "User"
            };

            // Act
            var (token, _) = _tokenService.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Используем "sub" (subject) вместо "nameid"
            jwtToken.Claims.Should().Contain(c =>
                c.Type == "sub" && c.Value == userId.ToString());
        }
        /// <summary>
        /// Проверяет, что токен содержит имя пользователя.
        /// </summary>
        [Fact]
        public void GenerateToken_ContainsUserNameClaim()
        {
            // Arrange
            var userName = "Test User";
            var user = new User
            {
                Id = 1,
                UserName = userName,
                Email = "test@example.com",
                Role = "User"
            };

            // Act
            var (token, _) = _tokenService.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            jwtToken.Claims.Should().Contain(c =>
                c.Type == "unique_name" && c.Value == userName);
        }

        /// <summary>
        /// Проверяет, что при пустом Issuer все равно создается токен (если в сервисе не требуется валидация).
        /// </summary>
        [Fact]
        public void GenerateToken_EmptyIssuer_Works()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"] = "ValidSecretKeyThatIsAtLeast32CharactersLong!",
                    ["JwtSettings:Issuer"] = "", // Пустой Issuer
                    ["JwtSettings:Audience"] = "TestAudience"
                })
                .Build();

            var service = new TokenService(configuration);
            var user = new User
            {
                Id = 1,
                UserName = "Test",
                Email = "test@test.com",
                Role = "User"
            };

            // Act
            var (token, _) = service.GenerateToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Проверяет, что refresh-токен имеет корректную длину.
        /// </summary>
        [Fact]
        public void GenerateRefreshToken_TokenHasValidLength()
        {
            // Act
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Assert
            refreshToken.Token.Should().NotBeNullOrEmpty();
            refreshToken.Token.Length.Should().BeGreaterThan(20); // Минимальная длина
        }
    }
}