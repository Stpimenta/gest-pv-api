using c___Api_Example.Models;

namespace IbpvDtos;

public class PendingUnlockGetDto
{
    public int Id { get; set; }
    public int BlockPeriodId { get; set; }
    public int BlockUserId { get; set; }
    public DateTime? DateUnlocked { get; set; }
    public bool IsActive { get; set; }
    public UsuarioGetByIdDTO BlockedUser { get; set; }

}