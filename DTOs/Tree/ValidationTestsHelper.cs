using System.ComponentModel.DataAnnotations;

namespace Tests.DTOs.Tree
{
    /// <summary>
    /// Вспомогательные методы для валидации моделей.
    /// </summary>
    public static class ValidationTestsHelper
    {
        /// <summary>
        /// Вспомогательный метод для валидации модели.
        /// </summary>
        public static Dictionary<string, string[]> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
            return validationResults.ToDictionary(
                r => r.MemberNames.First(),
                r => new[] { r.ErrorMessage ?? string.Empty });
        }
    }
}
