using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        string password = "admin123";
        string hashedPassword = HashPassword(password);
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {hashedPassword}");
        
        Console.WriteLine();
        Console.WriteLine("Hash for other test users:");
        Console.WriteLine($"mario123: {HashPassword("mario123")}");
        Console.WriteLine($"lucia123: {HashPassword("lucia123")}");
    }
    
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
