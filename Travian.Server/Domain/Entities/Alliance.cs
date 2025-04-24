using System.ComponentModel.DataAnnotations;

namespace YourProjectName.Domain.Entities
{
    public class Alliance
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        
        [Required]
        [MaxLength(5)]
        public string Tag { get; set; }
        
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // روابط
        public Guid FounderId { get; set; }
        public Player Founder { get; set; }
        
        public virtual ICollection<AllianceMember> Members { get; set; } = new List<AllianceMember>();
        public virtual ICollection<AllianceInvite> Invites { get; set; } = new List<AllianceInvite>();
    }
}