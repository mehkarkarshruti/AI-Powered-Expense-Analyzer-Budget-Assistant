using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseAnalyzer.API.Models
{
    public class Budget
    {
        [Key]
        public int BudgetId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 12)]
        public byte Month { get; set; }

        [Required]
        [Range(2000, 2100)]
        public short Year { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal BudgetAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}