using c___Api_Example.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExpenseImageMap : IEntityTypeConfiguration<ExpenseImageModel>
{
    public void Configure(EntityTypeBuilder<ExpenseImageModel> builder)
    {
        builder.ToTable("ExpenseImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(x => x.Expense)
            .WithMany(e => e.Images)
            .HasForeignKey(x => x.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}