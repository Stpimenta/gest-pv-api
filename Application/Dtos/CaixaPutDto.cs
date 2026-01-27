using System.ComponentModel.DataAnnotations;

namespace IbpvDtos;

public class CaixaPutDto
{
    [Required(ErrorMessage = "nome é obrigatório")]
    public string? Nome {get; set;}
}