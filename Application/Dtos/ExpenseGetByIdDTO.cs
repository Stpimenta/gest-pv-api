namespace IbpvDtos;

public class ExpenseGetByIdDTO
{
    public int Id {get;set;}
    public decimal Valor {get;set;}
    public string? Descricao {get;set;}
    public DateTime Data {get;set;}
    public string? UrlComprovante {get;set;}
    public string? NumeroFiscal {get;set;}
    public int IdCaixa {get;set;}
    
    public ICollection<ExpenseImageDTO> Images { get; set; } = new List<ExpenseImageDTO>();
}