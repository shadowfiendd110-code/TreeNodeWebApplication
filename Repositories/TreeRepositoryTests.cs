using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using TreeNodeWebApi.Data;
using TreeNodeWebApi.Interfaces;
using TreeNodeWebApi.Models.Entities;
using Xunit;

namespace TreeNodeWebApi.Repositories.Tests
{
    /// <summary>
    /// Тесты для репозитория узлов дерева.
    /// </summary>
    public class TreeNodeRepositoryTests : IDisposable
    {
        /// <summary>
        /// Контекст для тестирования.
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Репозиторий для тестирования.
        /// </summary>
        private readonly TreeNodeRepository _repository;

        /// <summary>
        /// Мок логгера.
        /// </summary>
        private readonly Mock<ILogger<TreeNodeRepository>> _loggerMock;

        /// <summary>
        /// Инициализация тестов.
        /// </summary>
        public TreeNodeRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<TreeNodeRepository>>();
            _repository = new TreeNodeRepository(_context, _loggerMock.Object);
        }

        /// <summary>
        /// Освобождает ресурсы после тестов.
        /// </summary>
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        /// <summary>
        /// Тестирует получение узла по Id - существующий узел.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsNode()
        {
            var node = new TreeNode { Name = "Test Node", ParentId = null };
            await _context.TreeNodes.AddAsync(node);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(node.Id);

            Assert.NotNull(result);
            Assert.Equal(node.Id, result.Id);
            Assert.Equal("Test Node", result.Name);
        }

        /// <summary>
        /// Тестирует получение узла по Id - несуществующий узел.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            var result = await _repository.GetByIdAsync(999);

            Assert.Null(result);
        }

        /// <summary>
        /// Тестирует получение прямых детей узла.
        /// </summary>
        [Fact]
        public async Task GetChildrenAsync_WithChildren_ReturnsChildren()
        {
            var parent = new TreeNode { Name = "Parent", ParentId = null };
            await _context.TreeNodes.AddAsync(parent);
            await _context.SaveChangesAsync();

            var child1 = new TreeNode { Name = "Child 1", ParentId = parent.Id };
            var child2 = new TreeNode { Name = "Child 2", ParentId = parent.Id };

            await _context.TreeNodes.AddRangeAsync(child1, child2);
            await _context.SaveChangesAsync();

            var result = await _repository.GetChildrenAsync(parent.Id);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, n => n.Name == "Child 1");
            Assert.Contains(result, n => n.Name == "Child 2");
        }


        /// <summary>
        /// Тестирует получение прямых детей узла - без детей.
        /// </summary>
        [Fact]
        public async Task GetChildrenAsync_WithoutChildren_ReturnsEmptyList()
        {
            var parent = new TreeNode { Name = "Parent", ParentId = null };
            await _context.TreeNodes.AddAsync(parent);
            await _context.SaveChangesAsync();

            var result = await _repository.GetChildrenAsync(parent.Id);

            Assert.Empty(result);
        }

        /// <summary>
        /// Тестирует получение корневых узлов.
        /// </summary>
        [Fact]
        public async Task GetRootNodesAsync_WithRootNodes_ReturnsRootNodes()
        {
            var root1 = new TreeNode { Name = "Root 1", ParentId = null };
            var root2 = new TreeNode { Name = "Root 2", ParentId = null };
            var child = new TreeNode { Name = "Child", ParentId = root1.Id };

            await _context.TreeNodes.AddRangeAsync(root1, root2, child);
            await _context.SaveChangesAsync();

            var result = await _repository.GetRootNodesAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, n => n.Name == "Root 1");
            Assert.Contains(result, n => n.Name == "Root 2");
        }

        /// <summary>
        /// Тестирует получение узла с детьми.
        /// </summary>
        [Fact]
        public async Task GetWithChildrenAsync_WithChildren_ReturnsNodeWithChildren()
        {
            var parent = new TreeNode { Name = "Parent", ParentId = null };
            await _context.TreeNodes.AddAsync(parent);
            await _context.SaveChangesAsync();

            var child = new TreeNode { Name = "Child", ParentId = parent.Id };
            await _context.TreeNodes.AddAsync(child);
            await _context.SaveChangesAsync();

            var result = await _repository.GetWithChildrenAsync(parent.Id);

            Assert.NotNull(result);
            Assert.Single(result.Children);
            Assert.Equal("Child", result.Children.First().Name);
        }


        /// <summary>
        /// Тестирует проверку существования узла.
        /// </summary>
        [Fact]
        public async Task ExistsAsync_ExistingNode_ReturnsTrue()
        {
            var node = new TreeNode { Name = "Test", ParentId = null };
            await _context.TreeNodes.AddAsync(node);
            await _context.SaveChangesAsync();

            var result = await _repository.ExistsAsync(node.Id);

            Assert.True(result);
        }

        /// <summary>
        /// Тестирует проверку существования узла - несуществующий.
        /// </summary>
        [Fact]
        public async Task ExistsAsync_NonExistingNode_ReturnsFalse()
        {
            var result = await _repository.ExistsAsync(999);

            Assert.False(result);
        }

        /// <summary>
        /// Тестирует проверку наличия детей.
        /// </summary>
        [Fact]
        public async Task HasChildrenAsync_NodeWithChildren_ReturnsTrue()
        {
            var parent = new TreeNode { Name = "Parent", ParentId = null };
            await _context.TreeNodes.AddAsync(parent);
            await _context.SaveChangesAsync();

            var child = new TreeNode { Name = "Child", ParentId = parent.Id };
            await _context.TreeNodes.AddAsync(child);
            await _context.SaveChangesAsync();

            var result = await _repository.HasChildrenAsync(parent.Id);

            Assert.True(result);
        }


        /// <summary>
        /// Тестирует проверку наличия детей - без детей.
        /// </summary>
        [Fact]
        public async Task HasChildrenAsync_NodeWithoutChildren_ReturnsFalse()
        {
            var node = new TreeNode { Name = "Leaf", ParentId = null };
            await _context.TreeNodes.AddAsync(node);
            await _context.SaveChangesAsync();

            var result = await _repository.HasChildrenAsync(node.Id);

            Assert.False(result);
        }

        /// <summary>
        /// Тестирует поиск узла по имени и родителю.
        /// </summary>
        [Fact]
        public async Task GetByNameAndParentIdAsync_ExistingNode_ReturnsNode()
        {
            var parent = new TreeNode { Name = "Parent", ParentId = null };
            await _context.TreeNodes.AddAsync(parent);
            await _context.SaveChangesAsync();

            var child = new TreeNode { Name = "Unique Child", ParentId = parent.Id };
            await _context.TreeNodes.AddAsync(child);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByNameAndParentIdAsync("Unique Child", parent.Id);

            Assert.NotNull(result);
            Assert.Equal("Unique Child", result.Name);
        }


        /// <summary>
        /// Тестирует поиск узла по имени и родителю - не найден.
        /// </summary>
        [Fact]
        public async Task GetByNameAndParentIdAsync_NonExistingNode_ReturnsNull()
        {
            var result = await _repository.GetByNameAndParentIdAsync("NonExisting", 1);

            Assert.Null(result);
        }

        /// <summary>
        /// Тестирует получение Id предков узла.
        /// </summary>
        [Fact]
        public async Task GetAncestorIdsAsync_NodeWithAncestors_ReturnsAncestorIds()
        {
            var root = new TreeNode { Name = "Root", ParentId = null };
            await _context.TreeNodes.AddAsync(root);
            await _context.SaveChangesAsync();

            var child = new TreeNode { Name = "Child", ParentId = root.Id };
            await _context.TreeNodes.AddAsync(child);
            await _context.SaveChangesAsync();

            var grandchild = new TreeNode { Name = "Grandchild", ParentId = child.Id };
            await _context.TreeNodes.AddAsync(grandchild);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAncestorIdsAsync(grandchild.Id);

            Assert.Equal(2, result.Count);
            Assert.Contains(root.Id, result);
            Assert.Contains(child.Id, result);
        }


        /// <summary>
        /// Тестирует получение Id предков узла - корневой узел.
        /// </summary>
        [Fact]
        public async Task GetAncestorIdsAsync_RootNode_ReturnsEmptyList()
        {
            var root = new TreeNode { Name = "Root", ParentId = null };
            await _context.TreeNodes.AddAsync(root);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAncestorIdsAsync(root.Id);

            Assert.Empty(result);
        }

        /// <summary>
        /// Тестирует получение Id предков узла - несуществующий узел.
        /// </summary>
        [Fact]
        public async Task GetAncestorIdsAsync_NonExistingNode_ReturnsEmptyList()
        {
            var result = await _repository.GetAncestorIdsAsync(999);

            Assert.Empty(result);
        }

        /// <summary>
        /// Тестирует добавление узла.
        /// </summary>
        [Fact]
        public async Task AddAsync_ValidNode_AddsToContext()
        {
            var node = new TreeNode { Name = "New Node", ParentId = null };

            await _repository.AddAsync(node);
            await _repository.SaveChangesAsync();

            var savedNode = await _context.TreeNodes.FindAsync(node.Id);
            Assert.NotNull(savedNode);
            Assert.Equal("New Node", savedNode.Name);
            _loggerMock.Verify(
                x => x.Log(LogLevel.Debug, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Узел New Node добавлен в контекст")),
                    null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Тестирует обновление узла.
        /// </summary>
        [Fact]
        public async Task Update_ValidNode_MarksAsModified()
        {
            var node = new TreeNode { Name = "Original", ParentId = null };
            await _context.TreeNodes.AddAsync(node);
            await _context.SaveChangesAsync();

            node.Name = "Updated";

            _repository.Update(node);
            await _repository.SaveChangesAsync();

            _context.Entry(node).State = EntityState.Detached;
            var updatedNode = await _context.TreeNodes.FindAsync(node.Id);
            Assert.Equal("Updated", updatedNode.Name);
            _loggerMock.Verify(
                x => x.Log(LogLevel.Debug, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Узел {node.Id} помечен как измененный")),
                    null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Тестирует удаление узла.
        /// </summary>
        [Fact]
        public async Task Remove_ValidNode_MarksForDeletion()
        {
            var node = new TreeNode { Name = "To Delete", ParentId = null };
            await _context.TreeNodes.AddAsync(node);
            await _context.SaveChangesAsync();

            _repository.Remove(node);
            await _repository.SaveChangesAsync();

            var deletedNode = await _context.TreeNodes.FindAsync(node.Id);
            Assert.Null(deletedNode);
            _loggerMock.Verify(
                x => x.Log(LogLevel.Debug, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Узел {node.Id} помечен для удаления")),
                    null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Тестирует сохранение изменений.
        /// </summary>
        [Fact]
        public async Task SaveChangesAsync_WithChanges_ReturnsChangeCount()
        {
            var node = new TreeNode { Name = "Test Save", ParentId = null };

            await _repository.AddAsync(node);
            var result = await _repository.SaveChangesAsync();

            Assert.Equal(1, result);
        }

        /// <summary>
        /// Тестирует начало транзакции.
        /// </summary>
        [Fact]
        public async Task BeginTransactionAsync_CreatesTransaction()
        {
            var transaction = await _repository.BeginTransactionAsync();

            Assert.NotNull(transaction);
            await transaction.DisposeAsync();
        }
    }
}
