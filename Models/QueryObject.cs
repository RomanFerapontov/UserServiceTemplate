namespace UserServiceTemplate.Models;

public class QueryObject {
    public string? SortBy { get; set; } = null;
    public string? OrderBy { get; set; } = null;
    public int Limit { get; set; } = 20;
    public int Offset { get; set; } = 0;
}
