using Microsoft.EntityFrameworkCore.Storage;
using TreeNodeWebApi.Models.Entities;

namespace TreeNodeWebApi.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с узлами.
    /// </summary>
    public interface ITreeRepository
    {
        /// <summary>
        /// Получает узел по Id.
        /// </summary>
        /// <param name="id">Id узла.</param>
        /// <returns>Узел.</returns>
        Task<TreeNode?> GetByIdAsync(int id);

        /// <summary>
        /// Получает всех прямых детей указанного родительского узла.
        /// </summary>
        /// <param name="parentId">Id родительского узла.</param>
        /// <returns>Всех прямых детей указанного родительского узла.</returns>
        Task<List<TreeNode>> GetChildrenAsync(int parentId);

        /// <summary>
        /// Получает все корневые узлы (без родителя).
        /// </summary>
        /// <returns>Все корневые узлы (без родителя).</returns>
        Task<List<TreeNode>> GetRootNodesAsync();

        /// <summary>
        /// Получает узел с его детьми (загружает коллекцию Children).
        /// </summary>
        /// <param name="id">Id узла.</param>
        /// <returns>Узел с его детьми.</returns>
        Task<TreeNode?> GetWithChildrenAsync(int id);

        /// <summary>
        /// Проверяет, существует ли узел с указанным Id.
        /// </summary>
        /// <param name="id">Id узла.</param>
        /// <returns>True, если узел существует/false, если не существует.</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Проверяет, есть ли у узла дети.
        /// </summary>
        /// <param name="id">Id узла.</param>
        /// <returns>True, если дети есть/false, если детей нет.</returns>
        Task<bool> HasChildrenAsync(int id);

        /// <summary>
        /// Ищет узел по имени и ParentId.
        /// </summary>
        /// <param name="name">Имя узла.</param>
        /// <param name="parentId">Id родительской узла.</param>
        /// <returns>Узел.</returns>
        Task<TreeNode?> GetByNameAndParentIdAsync(string name, int? parentId);

        /// <summary>
        /// Получает Id всех предков узла.
        /// </summary>
        /// <param name="nodeId">Id узла.</param>
        /// <returns>Список Id всех предков узла.</returns>
        Task<List<int>> GetAncestorIdsAsync(int nodeId);

        /// <summary>
        /// Добавляет новый узел в контекст.
        /// </summary>
        /// <param name="node">Узел.</param>
        Task AddAsync(TreeNode node);

        /// <summary>
        /// Помечает узел как измененный.
        /// </summary>
        /// <param name="node">Узел.</param>
        void Update(TreeNode node);

        /// <summary>
        /// Помечает узел для удаления.
        /// </summary>
        /// <param name="node">Узел.</param>
        void Remove(TreeNode node);

        /// <summary>
        /// Сохраняет все изменения в базе данных.
        /// </summary>
        /// <returns>Количество измененных записей.</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Начинает новую транзакцию.
        /// </summary>
        /// <returns>Объект для управления транзакцией БД.</returns>
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}