using System.ComponentModel.DataAnnotations;

namespace ExpenseAnalyzer.API.DTOs
{
    public class UpdateUserDto  //allowing the user to update Name and Email address
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;
    }
}
