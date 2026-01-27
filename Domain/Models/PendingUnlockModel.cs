using System.Text.Json.Serialization;
using c___Api_Example.Models;

namespace c___Api_Example.Domain.Models;

public class PendingUnlockModel
{
    public int Id { get; set; }
    public int BlockPeriodId { get; set; }
    public int BlockUserId { get; set; }
    public DateTime? DateUnlocked { get; set; }
    public bool IsActive { get; set; }
    
    [JsonIgnore]
    public BlockedPeriodModel BlockedPeriod { get; set; }
    public UsuarioModel BlockedUser { get; set; }
    
  
}