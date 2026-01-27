using System.ComponentModel.DataAnnotations;

namespace IbpvDtos;

public class BlockedPeriodsPostDTO
{
    
    [Required(ErrorMessage = "Campo obrigatório")]
    public DateTime? StartDate { get; set; }
    
    [Required(ErrorMessage = "Campo obrigatório")]
    [CustomValidation(typeof(BlockedPeriodsPostDTO),nameof(ValidateEndDate))]
    public DateTime? EndDate { get; set; }

    public static ValidationResult? ValidateEndDate(DateTime endDate, ValidationContext validationContext)
    {
        if (endDate > DateTime.Today)
        {
            return new ValidationResult("Data final não pode ser uma data futura");
        }
        
        return ValidationResult.Success;
    }
    
}