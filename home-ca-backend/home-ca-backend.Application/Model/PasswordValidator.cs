using System.Text.RegularExpressions;

namespace home_ca_backend.Application.Model;

internal class PasswordValidator
{
    public static bool BeSufficientPassword(string password) => 
        Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$");
}
