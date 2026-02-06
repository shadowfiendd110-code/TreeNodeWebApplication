using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using TreeNodeWebApi.Interfaces;
using TreeNodeWebApi.Models.DTOs.Tree;
using TreeNodeWebApi.Services;

namespace TreeNodeWebApi.Controllers
{
    /// <summary>
    /// Контроллер для работы с древовидной структурой.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TreeController : ControllerBase
    {
        /// <summary>
        /// Сервис для работы с деревом.
        /// </summary>
        private readonly ITreeService _treeService;

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger<TreeController> _logger;

        /// <summary>
        /// Инициализация контроллера.
        /// </summary>
        /// <param name="treeService">Сервис для работы с деревом.</param>
        /// <param name="logger">Логгер.</param>
        public TreeController(ITreeService treeService, ILogger<TreeController> logger)
        {
            _treeService = treeService;
            _logger = logger;
        }

        /// <summary>
        /// Получает узел по ID.
        /// </summary>
        /// <param name="id">ID узла.</param>
        /// <returns>DTO узла.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TreeNodeDto>> GetNode(int id)
        {
            _logger.LogInformation("Запрос узла {NodeId}", id);

            var nodeDto = await _treeService.GetNodeAsync(id);

            return Ok(nodeDto);
        }

        /// <summary>
        /// Получает все корневые узлы.
        /// </summary>
        /// <returns>Список DTO корневых узлов.</returns>
        [HttpGet("roots")]
        public async Task<ActionResult<List<TreeNodeDto>>> GetRootNodes()
        {
            _logger.LogInformation("Запрос корневых узлов");

            var roots = await _treeService.GetRootNodesAsync();

            return Ok(roots);
        }

        /// <summary>
        /// Создает новый узел.
        /// </summary>
        /// <param name="request">Запрос на создание узла.</param>
        /// <returns>DTO созданного узла.</returns>
        [HttpPost]
        public async Task<ActionResult<TreeNodeDto>> CreateNode(CreateTreeNodeRequest request)
        {
            _logger.LogInformation("Создание нового узла");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newNode = await _treeService.CreateNodeAsync(request);

            return CreatedAtAction(
                nameof(GetNode),
                new { id = newNode.Id },
                newNode
            );
        }

        /// <summary>
        /// Обновляет существующий узел.
        /// </summary>
        /// <param name="id">ID узла.</param>
        /// <param name="request">Запрос на обновление узла.</param>
        /// <returns>DTO обновленного узла.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TreeNodeDto>> UpdateNode(
            int id,
            UpdateTreeNodeRequest request)
        {
            _logger.LogInformation("Обновление узла {NodeId}", id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedNode = await _treeService.UpdateNodeAsync(id, request);

            return Ok(updatedNode);
        }

        /// <summary>
        /// Удаляет узел.
        /// </summary>
        /// <param name="id">ID узла.</param>
        /// <returns>Результат операции.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteNode(int id)
        {
            _logger.LogInformation("Удаление узла {NodeId}", id);

            await _treeService.DeleteNodeAsync(id);

            return NoContent();
        }

        /// <summary>
        /// Перемещает узел.
        /// </summary>
        /// <param name="id">ID узла.</param>
        /// <param name="request">Запрос на перемещение узла.</param>
        /// <returns>DTO перемещенного узла.</returns>
        [HttpPost("{id}/move")]
        public async Task<ActionResult<TreeNodeDto>> MoveNode(
            int id,
            [FromBody] MoveTreeNodeRequest request)
        {
            _logger.LogInformation("Перемещение узла {NodeId}", id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var movedNode = await _treeService.MoveNodeAsync(id, request.NewParentId);

            return Ok(movedNode);
        }

        /// <summary>
        /// Экспортирует дерево в JSON файл.
        /// </summary>
        /// <returns>JSON файл с деревом.</returns>
        [HttpGet("export/json")]
        public async Task<IActionResult> ExportTreeToJson()
        {
            _logger.LogInformation("Экспорт дерева в JSON");

            var treeExport = await _treeService.ExportTreeAsync();

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string jsonString = JsonSerializer.Serialize(treeExport, jsonOptions);

            string fileName = $"tree_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

            byte[] fileBytes = Encoding.UTF8.GetBytes(jsonString);

            return File(fileBytes, "application/json", fileName);
        }

        /// <summary>
        /// Экспортирует дерево как JSON в теле ответа.
        /// </summary>
        /// <returns>DTO экспортированного дерева.</returns>
        [HttpGet("export")]
        public async Task<ActionResult<TreeExportDto>> ExportTree()
        {
            _logger.LogInformation("Экспорт дерева (JSON в теле ответа)");

            var treeExport = await _treeService.ExportTreeAsync();

            return Ok(treeExport);
        }
    }

    /// <summary>
    /// DTO для запроса на перемещение узла.
    /// </summary>
    public class MoveTreeNodeRequest
    {
        /// <summary>
        /// ID нового родительского узла.
        /// </summary>
        [Required]
        public int? NewParentId { get; set; }
    }
}