using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using TreeNodeWebApi.Data;
using TreeNodeWebApi.Interfaces;
using TreeNodeWebApi.Models.Entities;

namespace TreeNodeWebApi.Repositories
{   
    /// <summary>
    /// Репозиторий для работы с узлами.
    /// </summary>
    public class TreeNodeRepository : ITreeRepository
    {   
        /// <summary>
        /// Контекст для работы с БД.
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger<TreeNodeRepository> _logger;

        /// <summary>
        /// Инициализация репозитория.
        /// </summary>
        /// <param name="context">Контекст для работы с БД.</param>
        /// <param name="logger">Логгер.</param>
        public TreeNodeRepository(ApplicationDbContext context, ILogger<TreeNodeRepository> logger)
        {
            _context = context; 
            _logger = logger;   
        }

        /// <summary>
        /// Получает узел по Id.
        /// </summary>
        /// <param name="id">Id узла.</param>
        /// <returns>Узел.</returns>
        public async Task<TreeNode?> GetByIdAsync(int id)
        {
            return await _context.TreeNodes.FindAsync(id);
        }

        /// <summary>
        /// Получает всех прямых детей указанного родительского узла.
        /// </summary>
        /// <param name="parentId">Id родительского узла.</param>
        /// <returns>Всех прямых детей указанного родительского узла.</returns>
        public async Task<List<TreeNode>> GetChildrenAsync(int parentId)
        {
            return await _context.TreeNodes
                .Where(n => n.ParentId == parentId)
                .ToListAsync();
        }

        /// <summary>
        /// Получает все корневые узлы (без родителя).
        /// </summary>
        /// <returns>Все корневые узлы (без родителя).</returns>
        public async Task<List<TreeNode>> GetRootNodesAsync()
        {
            return await _context.TreeNodes
                .Where(n => n.ParentId == null)
                .ToListAsync();
        }

        /// <summary>
        /// Получает узел с его детьми (загружает коллекцию Children)
        /// </summary>
        /// <param name="id">Id узла.</param>
        /// <returns>Узел с его детьми (загружает коллекцию Children)</returns>
        public async Task<TreeNode?> GetWithChildrenAsync(int id)
        {
            // Используем Include для загрузки связанных данных (жадная загрузка)
            return await _context.TreeNodes
                .Include(n => n.Children) // Загружаем коллекцию Children
                .FirstOrDefaultAsync(n => n.Id == id); // Ищем по ID
        }

        /// <summary>
        /// Проверяет, существует ли узел с указанным Id.
        /// </summary>
        /// <param name="id">Id узла.</param>
        /// <returns>True, если узел существует/false, если не существует.</returns>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.TreeNodes.AnyAsync(n => n.Id == id);
        }

        /// <summary>
        /// Проверяет, есть ли у узла дети.
        /// </summary>
        /// <param name="id">Id сущности.</param>
        /// <returns>True, если дети есть/false, если детей нет.</returns>
        public async Task<bool> HasChildrenAsync(int id)
        {
            return await _context.TreeNodes.AnyAsync(n => n.ParentId == id);
        }

        /// <summary>
        /// Ищет узел по имени и ParentId (для проверки уникальности)
        /// </summary>
        /// <param name="name">Имя узла.</param>
        /// <param name="parentId">Id родительской узла.</param>
        /// <returns>Узел.</returns>
        public async Task<TreeNode?> GetByNameAndParentIdAsync(string name, int? parentId)
        {
            return await _context.TreeNodes
                .FirstOrDefaultAsync(n => n.Name == name && n.ParentId == parentId);
        }

        /// <summary>
        /// Получает Id всех предков узла.
        /// </summary>
        /// <param name="nodeId">Id узла.</param>
        /// <returns>Всех предков узла.</returns>
        public async Task<List<int>> GetAncestorIdsAsync(int nodeId)
        {
            var ancestorIds = new List<int>();

            var currentNode = await GetByIdAsync(nodeId);

            if (currentNode == null)
            {
                return ancestorIds;
            }

            int? currentParentId = currentNode.ParentId;

            while (currentParentId.HasValue)
            {
                ancestorIds.Add(currentParentId.Value);

                var parent = await GetByIdAsync(currentParentId.Value);

                currentParentId = parent?.ParentId;
            }

            return ancestorIds;
        }

        /// <summary>
        /// Добавляет новый узел в контекст
        /// </summary>
        /// <param name="node">Узел.</param>

        public async Task AddAsync(TreeNode node)
        {
            await _context.TreeNodes.AddAsync(node);

            _logger.LogDebug("Узел {NodeName} добавлен в контекст", node.Name);
        }

        /// <summary>
        /// Помечает узел как измененный.
        /// </summary>
        /// <param name="node">Узел.</param>
        public void Update(TreeNode node)
        {
            _context.TreeNodes.Update(node);

            _logger.LogDebug("Узел {NodeId} помечен как измененный", node.Id);
        }

        /// <summary>
        /// Помечает узел для удаления
        /// </summary>
        /// <param name="node">Узел.</param>
        public void Remove(TreeNode node)
        {
            _context.TreeNodes.Remove(node);

            _logger.LogDebug("Узел {NodeId} помечен для удаления", node.Id);
        }

        /// <summary>
        /// Сохраняет все изменения в базе данных.
        /// </summary>
        /// <returns>Все изменения в базе данных.</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Начинает новую транзакцию.
        /// </summary>
        /// <returns>Объект для управления транзакцией БД.</returns>
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}