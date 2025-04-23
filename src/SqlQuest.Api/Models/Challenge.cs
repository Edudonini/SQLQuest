namespace SqlQuest.Api.Models;

public class Challenge
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SeedSql { get; set; } = string.Empty;
    public string SolutionSql { get; set; } = string.Empty;
}
