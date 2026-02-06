namespace TreeNodeWebApi.Models.Entities
{
    /// <summary>
    /// Древовидная сущность.
    /// </summary>
    public class TreeNode
    {
        /// <summary>
        /// Id сущности.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название сущности.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Id родительской сущности.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Время создания сущности.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Родительская сущность.
        /// </summary>
        public TreeNode? Parent { get; set; }

        /// <summary>
        /// Дочерние сущности.
        /// </summary>
        public ICollection<TreeNode> Children { get; set; } = new List<TreeNode>();
    }
}
