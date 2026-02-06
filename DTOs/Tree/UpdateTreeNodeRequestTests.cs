using FluentAssertions;
using Tests.DTOs.Tree;
using TreeNodeWebApi.Models.DTOs.Tree;


namespace Tests.Models.DTOs.Tree
{
    /// <summary>
    /// Тесты валидации для <see cref="UpdateTreeNodeRequest"/>.
    /// </summary>
    public class UpdateTreeNodeRequestTests
    {
        /// <summary>
        /// Проверяет валидацию при пустом Name.
        /// </summary>
        [Fact]
        public void Validation_NameEmpty_HasError()
        {
            var dto = new UpdateTreeNodeRequest { Name = "" };

            var result = ValidationTestsHelper.ValidateModel(dto);

            result.Should().HaveCountGreaterThan(0);
            result["Name"]!.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Проверяет валидацию при null Name.
        /// </summary>
        [Fact]
        public void Validation_NameNull_HasError()
        {
            var dto = new UpdateTreeNodeRequest { Name = null! };

            var result = ValidationTestsHelper.ValidateModel(dto);

            result.Should().HaveCountGreaterThan(0);
            result["Name"]!.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Проверяет валидацию при слишком длинном Name.
        /// </summary>
        [Fact]
        public void Validation_NameTooLong_HasError()
        {
            var dto = new UpdateTreeNodeRequest
            {
                Name = new string('a', 51)
            };

            var result = ValidationTestsHelper.ValidateModel(dto);

            result.Should().HaveCountGreaterThan(0);
            result["Name"]!.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Проверяет валидацию при корректных данных.
        /// </summary>
        [Fact]
        public void Validation_ValidData_NoErrors()
        {
            var dto = new UpdateTreeNodeRequest
            {
                Name = "Valid Name",
                NewParentId = 1
            };

            var result = ValidationTestsHelper.ValidateModel(dto);

            result.Should().BeEmpty();
        }
    }
}
