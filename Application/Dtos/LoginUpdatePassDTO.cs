using System.ComponentModel.DataAnnotations;

namespace IbpvDtos;

public class LoginUpdatePassDTO
{
    [Required(ErrorMessage = "Campo Obrigatório")]
    [RegularExpression(@"^(?=.*[!@#$%^&*()_+{}[\]:;<>,.?/~])[\w!@#$%^&*()_+{}[\]:;<>,.?/~]{6,}$", ErrorMessage = "senha muito fraca, utilize caracteres especiais")]
    public string? newPassword { get; set;}
    
}