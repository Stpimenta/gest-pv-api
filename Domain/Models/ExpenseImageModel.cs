using c___Api_Example.Models;

namespace c___Api_Example.Domain.Models;

public class ExpenseImageModel
{
    public int Id { get; set; }
    public int ExpenseId { get; set; }

    public string Url { get; set; }
    public DateTime CreatedAt { get; set; }

    public GastoModel Expense { get; set; }
}