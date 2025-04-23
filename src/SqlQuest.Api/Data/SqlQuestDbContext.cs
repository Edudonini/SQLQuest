using Microsoft.EntityFrameworkCore;
using SqlQuest.Api.Models;

namespace SqlQuest.Api.Data;

public class SqlQuestDbContext : DbContext
{
    public SqlQuestDbContext(DbContextOptions<SqlQuestDbContext> opts) : base(opts) { }

    public DbSet<Challenge> Challenges => Set<Challenge>();
}
