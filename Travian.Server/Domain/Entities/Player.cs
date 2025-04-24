using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourProjectName.Domain.Entities
{
    public class Player
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string? Salt { get; set; }

        public string? VerificationToken { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool IsVerified => VerifiedAt.HasValue;

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        // روابط
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<Village> Villages { get; set; } = new List<Village>();
    }
}