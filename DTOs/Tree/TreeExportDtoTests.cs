using FluentAssertions;
using Tests.DTOs.Tree;
using TreeNodeWebApi.Models.DTOs.Tree;


namespace Tests.Models.DTOs.Tree
{
    /// <summary>
    /// Тесты валидации для <see cref="TreeExportDto"/>.
    /// </summary>
    public class TreeExportDtoTests
    {
        /// <summary>
        /// Проверяет валидацию при пустой ExportVersion.
        /// </summary>
        [Fact]
        public void Validation_ExportVersionEmpty_HasError()
        {
            var dto = new TreeExportDto { ExportVersion = "" };

            var result = ValidationTestsHelper.ValidateModel(dto);

            result.Should().HaveCountGreaterThan(0);
            result["ExportVersion"]!.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Проверяет валидацию при null ExportVersion.
        /// </summary>
        [Fact]
        public void Validation_ExportVersionNull_HasError()
        {
            var dto = new TreeExportDto { ExportVersion = null! };

            var result = ValidationTestsHelper.ValidateModel(dto);

            result.Should().HaveCountGreaterThan(0);
            result["ExportVersion"]!.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Проверяет валидацию при слишком длинной ExportVersion.
        /// </summary>
        [Fact]
        public void Validation_ExportVersionTooLong_HasError()
        {
            var dto = new TreeExportDto
            {
                ExportVersion = new string('v', 11)
            };

            var result = ValidationTestsHelper.ValidateModel(dto);

            result.Should().HaveCountGreaterThan(0);
            result["ExportVersion"]!.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Проверяет валидацию при корректных данных.
        /// </summary>
        [Fact]
        public void Validation_ValidData_NoErrors()
        {
            var dto = new TreeExportDto
            {
                ExportVersion = "1.0",
                ExportDate = DateTime.UtcNow,
                TotalNodes = 5,
                Roots = new List<TreeNodeDto>()
            };

            var result = ValidationTestsHelper.ValidateModel(dto);

            result.Should().BeEmpty();
        }
    }
}
