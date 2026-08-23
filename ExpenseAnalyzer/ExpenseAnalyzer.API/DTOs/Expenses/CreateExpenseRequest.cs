using System.ComponentModel.DataAnnotations;

namespace ExpenseAnalyzer.API.DTOs
{
    public class CreateExpenseRequest
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(0.01, 99999999.99)]
        public decimal Amount { get; set; }

        [Required]
        public DateOnly ExpenseDate { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}
