using c___Api_Example.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace c___Api_Example.data;

public class PendingUnlockMap : IEntityTypeConfiguration<PendingUnlockModel>
{
    public void Configure(EntityTypeBuilder<PendingUnlockModel> builder)
    {
        builder.ToTable("PendingUnlocks");
        builder.HasKey(p => p.Id);
       
        builder.Property(p => p.DateUnlocked).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(p => p.IsActive).HasDefaultValue(true);
        
        builder.HasOne(p => p.BlockedUser)
            .WithMany(u => u.PendingUnlocks)
            .HasForeignKey(p => p.BlockUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(p => p.BlockedPeriod)
            .WithMany(u => u.PendingUnlocks)
            .HasForeignKey(p => p.BlockPeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}