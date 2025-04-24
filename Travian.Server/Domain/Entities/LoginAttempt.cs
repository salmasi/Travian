using System.ComponentModel.DataAnnotations;

namespace YourProjectName.Domain.Entities
{
    public class LoginAttempt
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public string IpAddress { get; set; }
        
        [Required]
        public string Username { get; set; }
        
        public bool IsSuccessful { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}