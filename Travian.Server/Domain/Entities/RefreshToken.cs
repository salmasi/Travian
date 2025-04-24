using System.ComponentModel.DataAnnotations;

namespace YourProjectName.Domain.Entities
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Token { get; set; }

        [Required]
        public DateTime Expires { get; set; }

        [Required]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        public string? CreatedByIp { get; set; }

        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsRevoked => Revoked != null;
        public bool IsActive => !IsRevoked && !IsExpired;

        // Foreign Key
        public Guid PlayerId { get; set; }

        // Navigation Property
        public virtual Player Player { get; set; }
    }
}