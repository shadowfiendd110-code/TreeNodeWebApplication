using System.ComponentModel.DataAnnotations;

namespace TreeNodeWebApi.Models.DTOs.Tree
{
    /// <summary>
    /// DTO создания древовидной сущности.
    /// </summary>
    public class CreateTreeNodeRequest
    {
        /// <summary>
        /// Id родительской сущности.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Название сущности.
        /// </summary>
        [Required(ErrorMessage = "Название сущности обязательно.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Название сущности должно быть от 1 до 50 символов в длину.")]
        public string Name { get; set; } = string.Empty;
    }
}