using Microsoft.EntityFrameworkCore.Storage;
using TreeNodeWebApi.Repositories;
using TreeNodeWebApi.Exceptions;
using TreeNodeWebApi.Interfaces;
using TreeNodeWebApi.Models.DTOs.Tree;
using TreeNodeWebApi.Models.Entities;
using TreeNodeWebApi.Exceptions.Tree;

namespace TreeNodeWebApi.Services
{
    /// <summary>
    /// Сервис для работы с древовидной структурой.
    /// </summary>
    public class TreeService : ITreeService
    {
        /// <summary>
        /// Репозиторий для работы с узлами.
        /// </summary>
        private readonly ITreeRepository _repository;

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger<TreeService> _logger;

        /// <summary>
        /// Инициализация сервиса.
        /// </summary>
        /// <param name="repository">Репозиторий для работы с узлами.</param>
        /// <param name="logger">Логгер.</param>
        public TreeService(ITreeRepository repository, ILogger<TreeService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Получает узел по ID и преобразует в DTO.
        /// </summary>
        /// <param name="id">ID узла.</param>
        /// <returns>DTO узла.</returns>
        public async Task<TreeNodeDto> GetNodeAsync(int id)
        {
            _logger.LogInformation("Получение узла {NodeId}", id);

            var node = await _repository.GetWithChildrenAsync(id);

            if (node == null)
            {
                throw new NotFoundException($"Узел с ID {id} не найден");
            }

            return ConvertToDtoRecursive(node);
        }

        /// <summary>
        /// Получает все корневые узлы.
        /// </summary>
        /// <returns>Список DTO корневых узлов.</returns>
        public async Task<List<TreeNodeDto>> GetRootNodesAsync()
        {
            _logger.LogInformation("Получение корневых узлов");

            var rootNodes = await _repository.GetRootNodesAsync();

            return rootNodes.Select(node => ConvertToDtoSimple(node)).ToList();
        }

        /// <summary>
        /// Создает новый узел.
        /// </summary>
        /// <param name="request">Запрос на создание узла.</param>
        /// <returns>DTO созданного узла.</returns>
        public async Task<TreeNodeDto> CreateNodeAsync(CreateTreeNodeRequest request)
        {
            _logger.LogInformation("Создание узла {NodeName} с родителем {ParentId}",
                request.Name, request.ParentId);

            var existingNode = await _repository.GetByNameAndParentIdAsync(
                request.Name,
                request.ParentId);

            if (existingNode != null)
                throw new DuplicateNameException(
                    $"Узел с именем '{request.Name}' уже существует у указанного родителя");

            if (request.ParentId.HasValue)
            {
                var parentExists = await _repository.ExistsAsync(request.ParentId.Value);
                if (!parentExists)
                    throw new NotFoundException($"Родительский узел {request.ParentId} не найден");
            }

            var newNode = new TreeNode
            {
                Name = request.Name,
                ParentId = request.ParentId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(newNode);

            await _repository.SaveChangesAsync();

            _logger.LogInformation("Узел {NodeId} создан успешно", newNode.Id);

            return ConvertToDtoSimple(newNode);
        }

        /// <summary>
        /// Обновляет существующий узел.
        /// </summary>
        /// <param name="id">ID узла.</param>
        /// <param name="request">Запрос на обновление узла.</param>
        /// <returns>DTO обновленного узла.</returns>
        public async Task<TreeNodeDto> UpdateNodeAsync(int id, UpdateTreeNodeRequest request)
        {
            _logger.LogInformation("Обновление узла {NodeId}", id);

            using var transaction = await _repository.BeginTransactionAsync();

            var node = await _repository.GetByIdAsync(id);

            if (node == null)
            {
                throw new NotFoundException($"Узел с ID {id} не найден");
            }

            if (node.Name != request.Name)
            {
                var existingNode = await _repository.GetByNameAndParentIdAsync(
                    request.Name,
                    node.ParentId);

                if (existingNode != null && existingNode.Id != id)
                    throw new DuplicateNameException(
                        $"Узел с именем '{request.Name}' уже существует у этого родителя");

                node.Name = request.Name;
            }

            if (request.NewParentId != node.ParentId)
            {
                await ValidateAndMoveNodeAsync(node, request.NewParentId);
            }

            await _repository.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("Узел {NodeId} обновлен успешно", id);

            return ConvertToDtoSimple(node);
        }

        /// <summary>
        /// Удаляет узел и всех его детей.
        /// </summary>
        /// <param name="id">ID узла.</param>
        public async Task DeleteNodeAsync(int id)
        {
            _logger.LogInformation("Удаление узла {NodeId} с детьми", id);

            using var transaction = await _repository.BeginTransactionAsync();

            var nodeExists = await _repository.GetByIdAsync(id);
            if (nodeExists == null)
            {
                throw new NotFoundException($"Узел с ID {id} не найден");
            }

            _repository.Remove(nodeExists);

            await _repository.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("Узел {NodeId} и его дети удалены успешно", id);
        }

        /// <summary>
        /// Перемещает узел к новому родителю.
        /// </summary>
        /// <param name="nodeId">ID узла.</param>
        /// <param name="newParentId">ID нового родительского узла.</param>
        /// <returns>DTO перемещенного узла.</returns>
        public async Task<TreeNodeDto> MoveNodeAsync(int nodeId, int? newParentId)
        {
            _logger.LogInformation("Перемещение узла {NodeId} к родителю {NewParentId}",
                nodeId, newParentId);

            using var transaction = await _repository.BeginTransactionAsync();

            var node = await _repository.GetByIdAsync(nodeId);
            if (node == null)
            {
                throw new NotFoundException($"Узел с ID {nodeId} не найден");
            }

            await ValidateAndMoveNodeAsync(node, newParentId);

            await _repository.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("Узел {NodeId} перемещен успешно", nodeId);

            return ConvertToDtoSimple(node);
        }

        /// <summary>
        /// Экспортирует все дерево в DTO.
        /// </summary>
        /// <returns>DTO экспортированного дерева.</returns>
        public async Task<TreeExportDto> ExportTreeAsync()
        {
            _logger.LogInformation("Экспорт дерева");

            var rootNodes = await _repository.GetRootNodesAsync();

            var rootDtos = new List<TreeNodeDto>();
            int totalNodes = 0;

            foreach (var rootNode in rootNodes)
            {
                var fullNode = await _repository.GetWithChildrenAsync(rootNode.Id);
                if (fullNode != null)
                {
                    var rootDto = ConvertToDtoRecursive(fullNode, ref totalNodes);
                    rootDtos.Add(rootDto);
                }
            }

            return new TreeExportDto
            {
                Roots = rootDtos,
                ExportDate = DateTime.UtcNow,
                TotalNodes = totalNodes
            };
        }

        /// <summary>
        /// Проверяет наличие циклических ссылок при перемещении узла.
        /// </summary>
        /// <param name="nodeId">ID узла.</param>
        /// <param name="newParentId">ID нового родительского узла.</param>
        /// <returns>True, если обнаружена циклическая ссылка/false, если циклической ссылки нет.</returns>
        public async Task<bool> CheckForCyclesAsync(int nodeId, int? newParentId)
        {
            if (newParentId == null)
                return false;

            if (nodeId == newParentId)
                return true;

            var ancestorIds = await _repository.GetAncestorIdsAsync(newParentId.Value);

            return ancestorIds.Contains(nodeId);
        }

        /// <summary>
        /// Валидирует и выполняет перемещение узла.
        /// </summary>
        /// <param name="node">Узел.</param>
        /// <param name="newParentId">ID нового родительского узла.</param>
        private async Task ValidateAndMoveNodeAsync(TreeNode node, int? newParentId)
        {
            var hasCycle = await CheckForCyclesAsync(node.Id, newParentId);
            if (hasCycle)
                throw new CyclicReferenceException(
                    "Невозможно переместить узел: обнаружена циклическая ссылка");

            if (newParentId.HasValue)
            {
                var parentExists = await _repository.ExistsAsync(newParentId.Value);
                if (!parentExists)
                    throw new NotFoundException($"Родительский узел {newParentId} не найден");
            }

            var existingNode = await _repository.GetByNameAndParentIdAsync(
                node.Name,
                newParentId);

            if (existingNode != null && existingNode.Id != node.Id)
                throw new DuplicateNameException(
                    $"Узел с именем '{node.Name}' уже существует у нового родителя");

            node.ParentId = newParentId;

            _repository.Update(node);
        }

        /// <summary>
        /// Преобразует TreeNode в TreeNodeDto без детей.
        /// </summary>
        /// <param name="node">Узел.</param>
        /// <returns>DTO узла.</returns>
        private TreeNodeDto ConvertToDtoSimple(TreeNode node)
        {
            return new TreeNodeDto
            {
                Id = node.Id,
                Name = node.Name,
                ParentId = node.ParentId,
                CreatedAt = node.CreatedAt
            };
        }

        /// <summary>
        /// Рекурсивно преобразует TreeNode в TreeNodeDto со всеми детьми.
        /// </summary>
        /// <param name="node">Узел.</param>
        /// <returns>DTO узла со всеми детьми.</returns>
        private TreeNodeDto ConvertToDtoRecursive(TreeNode node)
        {
            var dto = ConvertToDtoSimple(node);

            dto.Children = new List<TreeNodeDto>();

            foreach (var child in node.Children)
            {
                var childDto = ConvertToDtoRecursive(child);
                dto.Children.Add(childDto);
            }

            return dto;
        }

        /// <summary>
        /// Рекурсивно преобразует TreeNode в TreeNodeDto со счетчиком узлов.
        /// </summary>
        /// <param name="node">Узел.</param>
        /// <param name="nodeCount">Счетчик узлов.</param>
        /// <returns>DTO узла.</returns>
        private TreeNodeDto ConvertToDtoRecursive(TreeNode node, ref int nodeCount)
        {
            nodeCount++;

            var dto = ConvertToDtoSimple(node);

            dto.Children = new List<TreeNodeDto>();

            foreach (var child in node.Children)
            {
                var childDto = ConvertToDtoRecursive(child, ref nodeCount);
                dto.Children.Add(childDto);
            }

            return dto;
        }
    }
}