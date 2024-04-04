namespace UserServiceTemplate.Dtos;

public class UserReadDTO {
    public string? Uid { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
