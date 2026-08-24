using System.ComponentModel.DataAnnotations;

namespace ExpenseAnalyzer.API.Models;

public class Alert
{
    [Key]
    public int AlertId { get; set; }

    [Required]
    public int UserId { get; set; }

    public int? BudgetId { get; set; }

    [Required]
    public AlertType AlertType { get; set; }

    [Required]
    [MaxLength(255)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Budget? Budget { get; set; }
}