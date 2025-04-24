namespace YourProjectName.Domain.Entities
{
    public class BattleReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AttackId { get; set; }
        public Guid RecipientId { get; set; }
        
        public ReportType Type { get; set; }
        public string Content { get; set; } // JSON formatted data
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // روابط
        public Attack Attack { get; set; }
        public Player Recipient { get; set; }
    }

    public enum ReportType
    {
        Attack,
        Defense,
        Scout,
        Reinforcement
    }
}