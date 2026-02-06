using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreeNodeWebApi.Models.Entities;

namespace TreeNodeWebApi.Configurations
{
    /// <summary>
    /// Конфигурация древовидной сущности.
    /// </summary>
    public class TreeNodeConfiguration: IEntityTypeConfiguration<TreeNode>
    {
        /// <summary>
        /// Конфигурирует древовидную сущность.
        /// </summary>
        /// <param name="treeBuilder">Конфигуратор древовидной сущности.</param>
        public void Configure(EntityTypeBuilder<TreeNode> treeBuilder)
        {
            treeBuilder.ToTable("TreeNodes");

            treeBuilder.HasKey(tb => tb.Id);

            treeBuilder
            .Property(tb => tb.Name)
                .IsRequired()
                .HasMaxLength(50);
            
            treeBuilder.HasMany(tb => tb.Children)
                .WithOne(tb => tb.Parent)
                .HasForeignKey(tb => tb.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            treeBuilder
                .Property(t => t.ParentId)
                .IsRequired(false);

            treeBuilder
                .HasIndex(t => new { t.Name, t.ParentId })
                .IsUnique();
        }
    }
}
