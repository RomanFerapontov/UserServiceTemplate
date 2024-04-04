using System.ComponentModel.DataAnnotations;

namespace UserServiceTemplate.Models;

public class User {
    [Required]
    [Key]
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string? Uid { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; } = "USER";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
