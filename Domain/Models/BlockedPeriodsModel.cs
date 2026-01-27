using c___Api_Example.Models;

namespace c___Api_Example.Domain.Models;

public class BlockedPeriodModel
{
    public int Id { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool IsBlocked { get; set; }      
    public int BlockedById { get; set; }      

    public DateTime? BlockedDate { get; set; } 
    
    public UsuarioModel BlockedBy { get; set; }
    
    public ICollection<PendingUnlockModel> PendingUnlocks { get; set; }
}