using c___Api_Example.Domain.Models;
using Microsoft.EntityFrameworkCore;
using c___Api_Example.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace c___Api_Example.Infrastructure.Repository.Data.Map;

public class BlockedPeriodsMap : IEntityTypeConfiguration<BlockedPeriodModel>
{
    public void Configure(EntityTypeBuilder<BlockedPeriodModel> builder)
    {
        builder.ToTable("BlockedPeriods");
        builder.HasKey(x => x.Id);

        builder.Property(p => p.StartDate).IsRequired();
        builder.Property(p => p.EndDate).IsRequired();

        builder.Property(p => p.IsBlocked)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(p => p.BlockedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.HasOne(p => p.BlockedBy)
            .WithMany(u => u.BlockedPeriods)
            .HasForeignKey(p => p.BlockedById)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}

