namespace IbpvDtos;

public class GastoDTO
{
    public int Id { get; set; }
    public decimal Valor { get; set; }
    public string? Descricao { get; set; }
    public DateTime Data { get; set; }

    public List<ExpenseImageDTO> Images { get; set; } = new();

}