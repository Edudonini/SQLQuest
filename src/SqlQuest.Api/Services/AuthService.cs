using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using SqlQuest.Api.Models;
using SqlQuest.Api.Data;

namespace SqlQuest.Api.Services;

public class AuthService
{
    private readonly IConfiguration _cfg;
    private readonly SqlQuestDbContext _db;
    public AuthService(IConfiguration c, SqlQuestDbContext d) { _cfg = c; _db = d; }

    public void CreatePasswordHash(string pwd, out byte[] hash, out byte[] salt)
    {
        using var h = new HMACSHA512();
        salt = h.Key; hash = h.ComputeHash(Encoding.UTF8.GetBytes(pwd));
    }

    public bool Verify(string pwd, byte[] hash, byte[] salt)
    {
        using var h = new HMACSHA512(salt);
        return h.ComputeHash(Encoding.UTF8.GetBytes(pwd)).SequenceEqual(hash);
    }

    public string Token(User u)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var jwt = new JwtSecurityToken(signingCredentials: creds, expires: DateTime.UtcNow.AddHours(4));
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
