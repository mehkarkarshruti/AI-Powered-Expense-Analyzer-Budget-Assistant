namespace ExpenseAnalyzer.Core.Entities;

public class Alert
{
    public int AlertId { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Warning";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;

    // Navigation properties
    public User? User { get; set; }
}
