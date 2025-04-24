namespace SqlQuest.Api.DTOs;

public record ProgressDto(int UserId, int ChallengeId, bool Passed);