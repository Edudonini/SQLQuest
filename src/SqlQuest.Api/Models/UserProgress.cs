namespace SqlQuest.Api.Models;

public class UserProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ChallengeId { get; set; }
    public bool Passed { get; set; }
    public DateTime AttemptedAt { get; set; }
}