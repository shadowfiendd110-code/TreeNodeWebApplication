using System.ComponentModel.DataAnnotations;
using TreeNodeWebApi.Models.Entities;

namespace TreeNodeWebApi.Models.DTOs.Tree
{
    /// <summary>
    /// DTO древовидной сущности.
    /// </summary>
    public class TreeNodeDto
    {
        /// <summary>
        /// Id сущности.
        /// </summary>
        [Required(ErrorMessage = "Id сущности обязательно.")]
        public int Id { get; set; }

        /// <summary>
        /// Название сущности.
        /// </summary>
        [Required(ErrorMessage = "Название сущности обязательно.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Название сущности должно быть от 1 до 50 символов в длину.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Id родительской сущности.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Время создания сущности.
        /// </summary>
        [Required(ErrorMessage = "Время создания сущности обязательно.")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Имя родительской сущности.
        /// </summary>
        [StringLength(50, ErrorMessage = "Имя родительской сущности должно быть до 50 символов в длину.")]
        public string? ParentName { get; set; }

        /// <summary>
        /// Дочерние сущности.
        /// </summary>
        public ICollection<TreeNodeDto> Children { get; set; } = new List<TreeNodeDto>();
    }
}