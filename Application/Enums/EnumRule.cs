using System.ComponentModel;

namespace IbpvDtos.Enums;

public enum EnumRole
{
    [Description ("root")] 
    root = 0, 
    [Description ("admin")] 
    admin = 1,  
    [Description ("tesouraria")]  
    tesouraria = 2,  
    [Description ("membro")]  
    membro = 3,
    
    [Description ("pending")]  
    pending = 4,
    
    [Description ("inativo")]  
    inativo = 5,
}
