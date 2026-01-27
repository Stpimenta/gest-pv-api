namespace IbpvDtos;

public class PendingUnlockGetDto
{
    public int Id { get; set; }
    public int BlockPeriodId { get; set; }
    public int BlockUserId { get; set; }
    public DateTime? DateUnlocked { get; set; }
    public bool IsActive { get; set; }
    
  
}