using c___Api_Example.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ContributionImageMap : IEntityTypeConfiguration<ContributionImageModel>
{
    public void Configure(EntityTypeBuilder<ContributionImageModel> builder)
    {
        builder.ToTable("ContributionImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(x => x.Contribution)
            .WithMany(c => c.Images)
            .HasForeignKey(x => x.ContributionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}