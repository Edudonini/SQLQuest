using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using SqlQuest.Api.Data;
using SqlQuest.Api.DTOs;
using SqlQuest.Api.Models;
using SqlQuest.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<SqlQuestDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlQuestDb")));
builder.Services.AddScoped<SqlEvaluatorService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication().AddJwtBearer();

var app = builder.Build();

app.UseCors(); // ⬅️ isso aqui é essencial

app.UseSwagger();
app.UseSwaggerUI();


// Seeding
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SqlQuestDbContext>();
    db.Database.Migrate();

    if (!db.Challenges.Any())
    {
        db.Challenges.AddRange(
            new Challenge
            {
                Title = "Selecione todos os clientes",
                Description = "Retorne todas as linhas da tabela Customers.",
                SeedSql = """
                          CREATE TABLE Customers (Id INT, Name TEXT);
                          INSERT INTO Customers VALUES (1,'Ana'),(2,'Bruno');
                          """,
                SolutionSql = "SELECT * FROM Customers;"
            },
            new Challenge
            {
                Title = "Contagem de linhas",
                Description = "Conte quantos clientes existem na tabela Customers.",
                SeedSql = """
                          CREATE TABLE Customers (Id INT, Name TEXT);
                          INSERT INTO Customers VALUES (1,'Ana'),(2,'Bruno'),(3,'Clara');
                          """,
                SolutionSql = "SELECT COUNT(*) AS Total FROM Customers;"
            });
        await db.SaveChangesAsync();
    }
}

// Endpoints
app.MapGet("/api/challenges", async (SqlQuestDbContext db) =>
    await db.Challenges.AsNoTracking().ToListAsync());

app.MapPost("/api/attempt", async (
    AttemptRequest req,
    SqlQuestDbContext db,
    SqlEvaluatorService evaluator) =>
{
    var challenge = await db.Challenges.FindAsync(req.ChallengeId);
    if (challenge is null) return Results.NotFound("Challenge não encontrado.");

    var result = await evaluator.EvaluateAsync(challenge, req.UserSql);
    return Results.Ok(result);
});

app.MapPost("/api/auth/register", async (UserDto dto, AuthService svc, SqlQuestDbContext db) =>
{
    if(db.Users.Any(u => u.UserName == dto.UserName)) return Results.BadRequest("Usuário existente");
    svc.CreatePasswordHash(dto.Password, out var hash, out var salt);
    var user = new User { UserName = dto.UserName, PasswordHash = hash, PasswordSalt = salt};
    db.Users.Add(user); await db.SaveChangesAsync();
    return Results.Ok(new { token = svc.Token(user)});
});

app.MapPost("/api/auth/login", async (UserDto dto, AuthService auth, SqlQuestDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
    if (user is null || !auth.Verify(dto.Password, user.PasswordHash, user.PasswordSalt))
        return Results.Unauthorized();

    var token = auth.Token(user);
    return Results.Ok(new { token });
});

app.MapPost("/api/progress", async (ProgressDto dto, SqlQuestDbContext db) =>
{
    db.UserProgresses.Add(new UserProgress
    {
        UserId = dto.UserId,
        ChallengeId = dto.ChallengeId,
        Passed = dto.Passed,
        AttemptedAt = DateTime.UtcNow
    });
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();
