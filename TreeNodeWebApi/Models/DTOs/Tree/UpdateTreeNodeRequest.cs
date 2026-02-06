using System.ComponentModel.DataAnnotations;

namespace TreeNodeWebApi.Models.DTOs.Tree
{   
    /// <summary>
    /// DTO обновления древовидной сущности.
    /// </summary>
    public class UpdateTreeNodeRequest
    {
        /// <summary>
        /// Название сущности.
        /// </summary>
        [Required(ErrorMessage = "Название сущности обязательно.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Название сущности должно быть от 1 до 50 символов в длину.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Id новой родительской сущности.
        /// </summary>
        public int? NewParentId { get; set; }
    }
}