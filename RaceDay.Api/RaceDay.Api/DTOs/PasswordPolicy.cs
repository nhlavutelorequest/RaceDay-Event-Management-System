namespace RaceDay.Api.DTOs
{
    public static class PasswordPolicy
    {
        private static readonly HashSet<string> Common = new(StringComparer.OrdinalIgnoreCase)
        {
            "123456", "12345678", "123456789", "1234567890", "password", "password1",
            "password1!", "password123", "qwerty", "qwerty123", "abc123", "letmein",
            "welcome", "welcome1", "admin", "admin123", "iloveyou", "monkey",
            "football", "111111", "000000", "raceday", "raceday123", "p@ssw0rd",
            "passw0rd", "Password1", "Password123", "Password@123", "Admin@123"
        };

        // Returns null when the password is acceptable, otherwise a message for the user.
        public static string? Validate(string? password, string? email = null)
        {
            if (string.IsNullOrEmpty(password))
                return "Password is required.";
            if (password.Length < 8)
                return "Password must be at least 8 characters long.";
            if (!password.Any(char.IsUpper))
                return "Password must contain at least one uppercase letter.";
            if (!password.Any(char.IsLower))
                return "Password must contain at least one lowercase letter.";
            if (!password.Any(char.IsDigit))
                return "Password must contain at least one number.";
            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                return "Password must contain at least one special character (for example ! @ # $ %).";
            if (Common.Contains(password))
                return "That password is too common. Please choose a stronger one.";

            if (!string.IsNullOrWhiteSpace(email))
            {
                var name = email.Split('@')[0];
                if (name.Length >= 3 && password.Contains(name, StringComparison.OrdinalIgnoreCase))
                    return "Password must not contain your email name.";
            }

            return null;
        }
    }
}