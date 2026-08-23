using System.ComponentModel.DataAnnotations;

namespace ExpenseAnalyzer.API.DTOs
{
    public class CreateCategoryRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
