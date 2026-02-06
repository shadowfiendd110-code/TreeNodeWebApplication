using System.ComponentModel.DataAnnotations;

namespace TreeNodeWebApi.Models.DTOs.Tree
{
    /// <summary>
    /// DTO для экспорта всего дерева.
    /// </summary>
    public class TreeExportDto
    {
        /// <summary>
        /// Список корневых узлов.
        /// </summary>
        public List<TreeNodeDto> Roots { get; set; } = new();

        /// <summary>
        /// Дата и время экспорта.
        /// </summary>
        public DateTime ExportDate { get; set; }

        /// <summary>
        /// Общее количество узлов.
        /// </summary>
        public int TotalNodes { get; set; }

        /// <summary>
        /// Версия формата экспорта.
        /// </summary>
        [Required(ErrorMessage = "Версия формата экспорта обязательна.")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "Версия формата экспорта должна быть от 1 до 10 символов в длину.")]
        public string ExportVersion { get; set; } = "1.0";
    }
}