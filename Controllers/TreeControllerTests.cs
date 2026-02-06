using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TreeNodeWebApi.Controllers;
using TreeNodeWebApi.Exceptions.Tree;
using TreeNodeWebApi.Interfaces;
using TreeNodeWebApi.Models.DTOs.Tree;
using TreeNodeWebApi.Services;
using Xunit;

namespace Tests.Controllers
{
    /// <summary>
    /// Тесты для <see cref="TreeController"/>.
    /// </summary>
    public class TreeControllerTests
    {
        /// <summary>
        /// Мок сервиса дерева.
        /// </summary>
        private readonly Mock<ITreeService> _treeServiceMock;

        /// <summary>
        /// Мок логгера.
        /// </summary>
        private readonly Mock<ILogger<TreeController>> _loggerMock;

        /// <summary>
        /// Тестируемый контроллер.
        /// </summary>
        private readonly TreeController _controller;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="TreeControllerTests"/>.
        /// </summary>
        public TreeControllerTests()
        {
            _treeServiceMock = new Mock<ITreeService>();
            _loggerMock = new Mock<ILogger<TreeController>>();
            _controller = new TreeController(_treeServiceMock.Object, _loggerMock.Object);
        }

        /// <summary>
        /// Проверяет получение узла при успешном вызове сервиса.
        /// </summary>
        [Fact]
        public async Task GetNode_ValidId_ReturnsOkWithNode()
        {
            var nodeId = 1;
            var nodeDto = new TreeNodeDto { Id = nodeId, Name = "Test Node" };
            _treeServiceMock.Setup(s => s.GetNodeAsync(nodeId)).ReturnsAsync(nodeDto);

            var result = await _controller.GetNode(nodeId);

            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().Be(nodeDto);
        }

        /// <summary>
        /// Проверяет получение корневых узлов при успешном вызове сервиса.
        /// </summary>
        [Fact]
        public async Task GetRootNodes_ValidRequest_ReturnsOkWithRoots()
        {
            var rootDtos = new List<TreeNodeDto>
            {
                new TreeNodeDto { Id = 1, Name = "Root 1" },
                new TreeNodeDto { Id = 2, Name = "Root 2" }
            };
            _treeServiceMock.Setup(s => s.GetRootNodesAsync()).ReturnsAsync(rootDtos);

            var result = await _controller.GetRootNodes();

            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(rootDtos);
        }

        /// <summary>
        /// Проверяет создание узла при валидной модели.
        /// </summary>
        [Fact]
        public async Task CreateNode_ValidRequest_ReturnsCreatedAtAction()
        {
            var request = new CreateTreeNodeRequest { Name = "New Node" };
            var createdNode = new TreeNodeDto { Id = 1, Name = "New Node" };
            _treeServiceMock.Setup(s => s.CreateNodeAsync(request)).ReturnsAsync(createdNode);

            var result = await _controller.CreateNode(request) as ActionResult<TreeNodeDto>;

            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult!.Value.Should().Be(createdNode);
        }



        /// <summary>
        /// Проверяет ошибку валидации модели при создании узла.
        /// </summary>
        [Fact]
        public async Task CreateNode_InvalidModel_ReturnsBadRequest()
        {
            var request = new CreateTreeNodeRequest();
            _controller.ModelState.AddModelError("Name", "Name is required");

            var result = await _controller.CreateNode(request);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        /// <summary>
        /// Проверяет обновление узла при успешном вызове сервиса.
        /// </summary>
        [Fact]
        public async Task UpdateNode_ValidRequest_ReturnsOk()
        {
            var nodeId = 1;
            var request = new UpdateTreeNodeRequest { Name = "Updated Name" };
            var updatedNode = new TreeNodeDto { Id = nodeId, Name = "Updated Name" };
            _treeServiceMock.Setup(s => s.UpdateNodeAsync(nodeId, request)).ReturnsAsync(updatedNode);

            var result = await _controller.UpdateNode(nodeId, request);

            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().Be(updatedNode);
        }

        /// <summary>
        /// Проверяет удаление узла при успешном вызове сервиса.
        /// </summary>
        [Fact]
        public async Task DeleteNode_ValidId_ReturnsNoContent()
        {
            var nodeId = 1;
            _treeServiceMock.Setup(s => s.DeleteNodeAsync(nodeId));

            var result = await _controller.DeleteNode(nodeId);

            result.Should().BeOfType<NoContentResult>();
        }

        /// <summary>
        /// Проверяет перемещение узла при успешном вызове сервиса.
        /// </summary>
        [Fact]
        public async Task MoveNode_ValidRequest_ReturnsOk()
        {
            var nodeId = 1;
            var request = new MoveTreeNodeRequest { NewParentId = 2 };
            var movedNode = new TreeNodeDto { Id = 1, ParentId = 2 };
            _treeServiceMock.Setup(s => s.MoveNodeAsync(nodeId, request.NewParentId)).ReturnsAsync(movedNode);

            var result = await _controller.MoveNode(nodeId, request);

            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().Be(movedNode);
        }

        /// <summary>
        /// Проверяет экспорт дерева в JSON файл.
        /// </summary>
        [Fact]
        public async Task ExportTreeToJson_ValidRequest_ReturnsFile()
        {
            var exportDto = new TreeExportDto
            {
                Roots = new List<TreeNodeDto>(),
                ExportDate = DateTime.UtcNow,
                TotalNodes = 0
            };
            _treeServiceMock.Setup(s => s.ExportTreeAsync()).ReturnsAsync(exportDto);

            var result = await _controller.ExportTreeToJson();

            result.Should().BeOfType<FileContentResult>();
            var fileResult = result as FileContentResult;
            fileResult!.ContentType.Should().Be("application/json");
        }

        /// <summary>
        /// Проверяет экспорт дерева в теле ответа.
        /// </summary>
        [Fact]
        public async Task ExportTree_ValidRequest_ReturnsOk()
        {
            var exportDto = new TreeExportDto
            {
                Roots = new List<TreeNodeDto>(),
                ExportDate = DateTime.UtcNow,
                TotalNodes = 0
            };
            _treeServiceMock.Setup(s => s.ExportTreeAsync()).ReturnsAsync(exportDto);

            var result = await _controller.ExportTree();

            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(exportDto);
        }


        /// <summary>
        /// Проверяет обработку исключений сервиса в GetNode.
        /// </summary>
        [Fact]
        public async Task GetNode_ServiceThrowsNotFound_ThrowsException()
        {
            var nodeId = 999;
            _treeServiceMock.Setup(s => s.GetNodeAsync(nodeId))
                          .ThrowsAsync(new NotFoundException("Node not found"));

            var act = async () => await _controller.GetNode(nodeId);

            await act.Should().ThrowExactlyAsync<NotFoundException>();
        }


        /// <summary>
        /// Проверяет обработку исключений сервиса в CreateNode.
        /// </summary>
        [Fact]
        public async Task CreateNode_ServiceThrowsDuplicateName_ThrowsException()
        {
            var request = new CreateTreeNodeRequest { Name = "Duplicate" };
            _treeServiceMock.Setup(s => s.CreateNodeAsync(request))
                          .ThrowsAsync(new DuplicateNameException("Name exists"));

            var act = async () => await _controller.CreateNode(request);

            await act.Should().ThrowExactlyAsync<DuplicateNameException>();
        }


        /// <summary>
        /// Проверяет логирование при получении узла.
        /// </summary>
        [Fact]
        public async Task GetNode_ValidRequest_LogsInformation()
        {
            var nodeId = 1;
            var nodeDto = new TreeNodeDto { Id = 1, Name = "Test" };
            _treeServiceMock.Setup(s => s.GetNodeAsync(nodeId)).ReturnsAsync(nodeDto);

            await _controller.GetNode(nodeId);

            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Запрос узла {nodeId}")),
                    null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Проверяет валидацию ModelState в UpdateNode.
        /// </summary>
        [Fact]
        public async Task UpdateNode_InvalidModel_ReturnsBadRequest()
        {
            var nodeId = 1;
            var request = new UpdateTreeNodeRequest();
            _controller.ModelState.AddModelError("Name", "Invalid name");

            var result = await _controller.UpdateNode(nodeId, request);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
