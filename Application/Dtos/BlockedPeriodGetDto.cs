namespace IbpvDtos;

public class BlockedPeriodGetDto
{
    public int Id { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool IsBlocked { get; set; }      
    public int BlockedById { get; set; }      

    public DateTime? BlockedDate { get; set; } 
    
    public UsuarioGetByIdDTO BlockedBy { get; set; }
    
    public ICollection<PendingUnlockGetDto> PendingUnlocks { get; set; }

}