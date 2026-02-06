using FluentAssertions;
using Tests.DTOs.Tree;
using TreeNodeWebApi.Models.DTOs.Tree;


namespace Tests.Models.DTOs.Tree
{
    /// <summary>
    /// Тесты валидации для <see cref="CreateTreeNodeRequest"/>.
    /// </summary>
    public class CreateTreeNodeRequestTests
    {
        /// <summary>
        /// Проверяет валидацию при пустом Name.
        /// </summary>
        [Fact]
        public void Validation_NameEmpty_HasError()
        {
            var dto = new CreateTreeNodeRequest { Name = "" };

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
            var dto = new CreateTreeNodeRequest { Name = null! };

            var result = ValidationTestsHelper.ValidateModel(dto);

            result.Should().HaveCountGreaterThan(0);
            result["Name"]!.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Проверяет валидацию при невалидной длине Name.
        /// </summary>
        [Theory]
        [InlineData("")]                  
        [InlineData("123456789012345678901234567890123456789012345678901")] 
        public void Validation_NameInvalidLength_HasError(string name)
        {
            var dto = new CreateTreeNodeRequest { Name = name };

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
            var dto = new CreateTreeNodeRequest
            {
                Name = "Valid Node Name",
                ParentId = 1
            };

            var result = ValidationTestsHelper.ValidateModel(dto);

            result.Should().BeEmpty();
        }
    }
}
