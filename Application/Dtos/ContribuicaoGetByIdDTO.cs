using System.ComponentModel.DataAnnotations;
using IbpvDtos.personvalidations;

namespace IbpvDtos;

public class ContribuicaoGetByIdDTO
{
    public int Id {get;set;}
    public decimal Valor {get;set;}
    public string? Descricao {get;set;}
    public DateTime? Data {get;set;}
    public string? UrlEnvelope {get;set;}
    public int? IdCaixa {get;set;}
    public int? IdMembro {get; set;}
    public string? TokenMembro {get;set;}
    
    public List<ContribuicaoImageDTO> Images { get; set; } = new List<ContribuicaoImageDTO>();
}