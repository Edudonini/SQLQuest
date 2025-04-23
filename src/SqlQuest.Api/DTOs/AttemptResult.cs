namespace SqlQuest.Api.DTOs;

public record AttemptResult(
    bool Passed,
    string Message,
    IEnumerable<IDictionary<string, object>>? ExpectedRows = null,
    IEnumerable<IDictionary<string, object>>? UserRows = null);
