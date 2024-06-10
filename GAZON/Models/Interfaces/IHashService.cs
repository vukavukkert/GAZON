namespace GAZON.Models.Interfaces;

public interface IHashService
{
    string HashPassword(string password);
    bool VerifyHashedPassword(string hashedPassword, string password);
}
