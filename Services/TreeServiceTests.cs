using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TreeNodeWebApi.Data;
using TreeNodeWebApi.Exceptions;
using TreeNodeWebApi.Exceptions.Tree;
using TreeNodeWebApi.Interfaces;
using TreeNodeWebApi.Models.DTOs.Tree;
using TreeNodeWebApi.Models.Entities;
using TreeNodeWebApi.Repositories;
using TreeNodeWebApi.Services;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Tests.Services
{
    /// <summary>
    /// Тесты для <see cref="TreeService"/>.
    /// </summary>
    public class TreeServiceTests : IDisposable
    {
        /// <summary>
        /// Контекст базы данных для тестирования.
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Репозиторий для тестирования.
        /// </summary>
        private readonly TreeNodeRepository _repository;

        /// <summary>
        /// Мок логгера.
        /// </summary>
        private readonly Mock<ILogger<TreeService>> _loggerMock;

        /// <summary>
        /// Тестируемый сервис.
        /// </summary>
        private readonly TreeService _service;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="TreeServiceTests"/>.
        /// </summary>
        public TreeServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TreeServiceTestDb_{Guid.NewGuid()}")
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new TreeNodeRepository(_context, new Mock<ILogger<TreeNodeRepository>>().Object);
            _loggerMock = new Mock<ILogger<TreeService>>();
            _service = new TreeService(_repository, _loggerMock.Object);
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
        /// Проверяет получение существующего узла.
        /// </summary>
        [Fact]
        public async Task GetNodeAsync_ExistingNode_ReturnsNodeDto()
        {
            var node = new TreeNode { Name = "Test Node", ParentId = null };
            await _context.TreeNodes.AddAsync(node);
            await _context.SaveChangesAsync();

            var result = await _service.GetNodeAsync(node.Id);

            result.Should().NotBeNull();
            result.Name.Should().Be("Test Node");
            result.Id.Should().Be(node.Id);
        }

        /// <summary>
        /// Проверяет получение корневых узлов.
        /// </summary>
        [Fact]
        public async Task GetRootNodesAsync_WithRootNodes_ReturnsRootNodes()
        {
            var root1 = new TreeNode { Name = "Root 1", ParentId = null };
            var root2 = new TreeNode { Name = "Root 2", ParentId = null };
            await _context.TreeNodes.AddRangeAsync(root1, root2);
            await _context.SaveChangesAsync();

            var result = await _service.GetRootNodesAsync();

            result.Should().HaveCount(2);
            result.Should().ContainSingle(n => n.Name == "Root 1");
            result.Should().ContainSingle(n => n.Name == "Root 2");
        }

        /// <summary>
        /// Проверяет создание узла без родителя.
        /// </summary>
        [Fact]
        public async Task CreateNodeAsync_ValidRootNode_CreatesNode()
        {
            var request = new CreateTreeNodeRequest
            {
                Name = "New Root Node",
                ParentId = null
            };

            var result = await _service.CreateNodeAsync(request);

            result.Should().NotBeNull();
            result.Name.Should().Be("New Root Node");
            result.Id.Should().BeGreaterThan(0);

            var savedNode = await _context.TreeNodes.FindAsync(result.Id);
            savedNode.Should().NotBeNull();
            savedNode.Name.Should().Be("New Root Node");
        }

        /// <summary>
        /// Проверяет создание узла с родителем.
        /// </summary>
        [Fact]
        public async Task CreateNodeAsync_ValidChildNode_CreatesNode()
        {
            var parent = new TreeNode { Name = "Parent", ParentId = null };
            await _context.TreeNodes.AddAsync(parent);
            await _context.SaveChangesAsync();

            var request = new CreateTreeNodeRequest
            {
                Name = "Child Node",
                ParentId = parent.Id
            };

            var result = await _service.CreateNodeAsync(request);

            result.Should().NotBeNull();
            result.Name.Should().Be("Child Node");
            result.ParentId.Should().Be(parent.Id);
        }

        /// <summary>
        /// Проверяет выброс исключения при дублирующемся имени.
        /// </summary>
        [Fact]
        public async Task CreateNodeAsync_DuplicateName_ThrowsDuplicateNameException()
        {
            var existingNode = new TreeNode { Name = "Duplicate", ParentId = null };
            await _context.TreeNodes.AddAsync(existingNode);
            await _context.SaveChangesAsync();

            var request = new CreateTreeNodeRequest
            {
                Name = "Duplicate",
                ParentId = null
            };

            var act = () => _service.CreateNodeAsync(request);

            await act.Should().ThrowExactlyAsync<DuplicateNameException>();
        }

        /// <summary>
        /// Проверяет выброс исключения при несуществующем родителе.
        /// </summary>
        [Fact]
        public async Task CreateNodeAsync_NonExistingParent_ThrowsNotFoundException()
        {
            var request = new CreateTreeNodeRequest
            {
                Name = "Orphan Node",
                ParentId = 999
            };

            var act = () => _service.CreateNodeAsync(request);

            await act.Should().ThrowExactlyAsync<NotFoundException>();
        }

        /// <summary>
        /// Проверяет обновление имени узла.
        /// </summary>
        [Fact]
        public async Task UpdateNodeAsync_UpdateName_UpdatesSuccessfully()
        {
            var node = new TreeNode { Name = "Old Name", ParentId = null };
            await _context.TreeNodes.AddAsync(node);
            await _context.SaveChangesAsync();

            var request = new UpdateTreeNodeRequest
            {
                Name = "New Name"
            };

            var result = await _service.UpdateNodeAsync(node.Id, request);

            result.Should().NotBeNull();
            result.Name.Should().Be("New Name");

            var updatedNode = await _context.TreeNodes.FindAsync(node.Id);
            updatedNode.Name.Should().Be("New Name");
        }

        /// <summary>
        /// Проверяет перемещение узла к новому родителю.
        /// </summary>
        [Fact]
        public async Task UpdateNodeAsync_ChangeParent_MovesSuccessfully()
        {
            var parent1 = new TreeNode { Name = "Parent 1", ParentId = null };
            var parent2 = new TreeNode { Name = "Parent 2", ParentId = null };
            var child = new TreeNode { Name = "Child", ParentId = parent1.Id };

            await _context.TreeNodes.AddAsync(parent1);
            await _context.SaveChangesAsync();
            child.ParentId = parent1.Id;
            await _context.TreeNodes.AddAsync(child);
            await _context.SaveChangesAsync();

            await _context.TreeNodes.AddAsync(parent2);
            await _context.SaveChangesAsync();

            var request = new UpdateTreeNodeRequest
            {
                NewParentId = parent2.Id
            };

            var result = await _service.UpdateNodeAsync(child.Id, request);

            result.Should().NotBeNull();
            result.ParentId.Should().Be(parent2.Id);
        }

        /// <summary>
        /// Проверяет удаление узла.
        /// </summary>
        [Fact]
        public async Task DeleteNodeAsync_ExistingNode_DeletesSuccessfully()
        {
            var node = new TreeNode { Name = "To Delete", ParentId = null };
            await _context.TreeNodes.AddAsync(node);
            await _context.SaveChangesAsync();

            var act = async () => await _service.DeleteNodeAsync(node.Id);
            await act.Should().NotThrowAsync();

            var deletedNode = await _context.TreeNodes.FindAsync(node.Id);
            deletedNode.Should().BeNull();
        }

        /// <summary>
        /// Проверяет перемещение узла методом MoveNodeAsync.
        /// </summary>
        [Fact]
        public async Task MoveNodeAsync_ValidMove_MovesSuccessfully()
        {
            var parent1 = new TreeNode { Name = "Old Parent", ParentId = null };
            var parent2 = new TreeNode { Name = "New Parent", ParentId = null };
            var child = new TreeNode { Name = "Movable", ParentId = parent1.Id };

            await _context.TreeNodes.AddAsync(parent1);
            await _context.SaveChangesAsync();
            child.ParentId = parent1.Id;
            await _context.TreeNodes.AddAsync(child);
            await _context.SaveChangesAsync();

            await _context.TreeNodes.AddAsync(parent2);
            await _context.SaveChangesAsync();

            var result = await _service.MoveNodeAsync(child.Id, parent2.Id);

            result.Should().NotBeNull();
            result.ParentId.Should().Be(parent2.Id);
        }

        /// <summary>
        /// Проверяет экспорт дерева.
        /// </summary>
        [Fact]
        public async Task ExportTreeAsync_WithHierarchy_ReturnsFullTree()
        {
            var root = new TreeNode { Name = "Root", ParentId = null };
            var child = new TreeNode { Name = "Child", ParentId = root.Id };

            await _context.TreeNodes.AddAsync(root);
            await _context.SaveChangesAsync();
            child.ParentId = root.Id;
            await _context.TreeNodes.AddAsync(child);
            await _context.SaveChangesAsync();

            var result = await _service.ExportTreeAsync();

            result.Should().NotBeNull();
            result.Roots.Should().HaveCount(1);
            result.Roots[0].Children.Should().HaveCount(1);
            result.TotalNodes.Should().Be(2);
        }

        /// <summary>
        /// Проверяет обнаружение циклических ссылок.
        /// </summary>
        [Fact]
        public async Task CheckForCyclesAsync_CycleDetected_ReturnsTrue()
        {
            var parent = new TreeNode { Name = "Parent", ParentId = null };
            var child = new TreeNode { Name = "Child", ParentId = parent.Id };

            await _context.TreeNodes.AddAsync(parent);
            await _context.SaveChangesAsync();
            child.ParentId = parent.Id;
            await _context.TreeNodes.AddAsync(child);
            await _context.SaveChangesAsync();

            var hasCycle = await _service.CheckForCyclesAsync(child.Id, child.Id);

            hasCycle.Should().BeTrue();
        }

        /// <summary>
        /// Проверяет выброс исключения при циклической ссылке.
        /// </summary>
        [Fact]
        public async Task MoveNodeAsync_CycleDetected_ThrowsCyclicReferenceException()
        {
            var parent = new TreeNode { Name = "Parent", ParentId = null };
            var child = new TreeNode { Name = "Child", ParentId = parent.Id };

            await _context.TreeNodes.AddAsync(parent);
            await _context.SaveChangesAsync();
            child.ParentId = parent.Id;
            await _context.TreeNodes.AddAsync(child);
            await _context.SaveChangesAsync();

            var act = () => _service.MoveNodeAsync(child.Id, child.Id);

            await act.Should().ThrowExactlyAsync<CyclicReferenceException>();
        }

        /// <summary>
        /// Проверяет выброс NotFoundException при несуществующем узле.
        /// </summary>
        [Fact]
        public async Task GetNodeAsync_NonExistingNode_ThrowsNotFoundException()
        {
            var act = () => _service.GetNodeAsync(999);

            await act.Should().ThrowExactlyAsync<NotFoundException>();
        }

        /// <summary>
        /// Проверяет логирование при создании узла.
        /// </summary>
        [Fact]
        public async Task CreateNodeAsync_ValidRequest_LogsInformation()
        {
            var mockRepository = new Mock<ITreeRepository>();
            var mockLogger = new Mock<ILogger<TreeService>>();
            var service = new TreeService(mockRepository.Object, mockLogger.Object);

            var request = new CreateTreeNodeRequest { Name = "Test Node", ParentId = null };
            mockRepository.Setup(r => r.GetByNameAndParentIdAsync(It.IsAny<string>(), It.IsAny<int?>()))
                         .ReturnsAsync((TreeNode)null);
            mockRepository.Setup(r => r.AddAsync(It.IsAny<TreeNode>()));
            mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            await service.CreateNodeAsync(request);

            mockLogger.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Создание узла Test Node")),
                    null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
