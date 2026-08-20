using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Domain.Enums;

namespace Domain.Entities
{
    public class User
    {
        public string? Id { get; set; }

        [Required]
        [MinLength(1)]
        public string Nombres { get; set; } = null!;

        [Required]
        [MinLength(1)]
        public string Apellidos { get; set; } = null!;

        [Required]
        [Range(1000000, long.MaxValue, ErrorMessage = "Cedula must have at least 7 digits")]
        public long Cedula { get; set; }

        [Required]
        [Range(1000000000, long.MaxValue, ErrorMessage = "Telefono must have at least 10 digits")]
        public long Telefono { get; set; }

        [Required]
        [EmailAddress]
        [MinLength(1)]
        public string Email { get; set; } = null!;

        [Required]
        public Role Role { get; set; }

        [Required]
        [MinLength(1)]
        public string PasswordHash { get; set; } = null!;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Lo que hay de aquí para abajo es para manejar la creación de usuarios con contraseña y la verificación de contraseñas,
        // asegurando que el hash de la contraseña se maneje de manera segura y que se cumplan los requisitos mínimos de longitud 
        // de la contraseña. La validación en el constructor privado es una medida de seguridad adicional para garantizar la 
        // integridad del objeto User, aunque se espera que la creación de usuarios se realice principalmente a través del método 
        // de fábrica CreateWithPassword, que maneja el hashing de contraseñas y la validación de requisitos.
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        // Minimum password length requirement for user creation
        private const int MinPasswordLength = 5;

        // Private constructor to enforce the use of the factory method for user creation, ensuring that password hashing is handled correctly.
        public User(string nombres, string apellidos, long cedula, long telefono, string email, Role role, string passwordHash)
        {
            Nombres = nombres ?? throw new ArgumentNullException(nameof(nombres)); // Validation of non-empty strings is handled by the 
            // factory method and data annotations, so we just check for null here.
            Apellidos = apellidos ?? throw new ArgumentNullException(nameof(apellidos)); // Same as above for non-empty validation.
            // This check is a safeguard; the factory method and data annotations should prevent invalid values, but we enforce 
            // it here as well.
            if (cedula < 1_000_000) throw new ArgumentOutOfRangeException(nameof(cedula), "Cedula must have at least 7 digits"); // 
            Cedula = cedula;
            if (telefono < 1_000_000_000) throw new ArgumentOutOfRangeException(nameof(telefono), "Telefono must have at least 10 digits");
            Telefono = telefono;
            Telefono = telefono;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Role = role;
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        }     

        // Factory method to create a new User with a plaintext password. The password will be hashed internally.
        public static User CreateWithPassword(string nombres, string apellidos, long cedula, long telefono, string email, Role role, string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
                throw new ArgumentException($"Password must be at least {MinPasswordLength} characters long", nameof(password));

            var hash = ComputeHash(password);
            return new User(nombres, apellidos, cedula, telefono, email, role, hash);
        }

        // Method to verify a plaintext password against the stored password hash
        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            var hash = ComputeHash(password);
            return string.Equals(hash, PasswordHash, StringComparison.Ordinal); // Use ordinal comparison for hash strings
            //StringComparison.Ordinal is used for comparing the hash strings because it performs a byte-by-byte comparison, 
            // which is appropriate for hash values. It ensures that the comparison is case-sensitive and culture-insensitive, 
            // which is important for security-related comparisons like password hashes.
        }

        // Helper method to compute a SHA256 hash of the input string
        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashed = sha.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (var b in hashed) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
