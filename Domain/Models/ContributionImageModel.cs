using c___Api_Example.Models;

namespace c___Api_Example.Domain.Models;

public class ContributionImageModel
{
    public int Id { get; set; }
    public int ContributionId { get; set; }

    public string Url { get; set; }
    public DateTime CreatedAt { get; set; }

    public ContribuicaoModel Contribution { get; set; }
}

