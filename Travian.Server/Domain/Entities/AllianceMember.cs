namespace YourProjectName.Domain.Entities
{
    public class AllianceMember
    {
        public Guid AllianceId { get; set; }
        public Guid PlayerId { get; set; }
        
        public AllianceRole Role { get; set; } = AllianceRole.Member;
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        
        // روابط
        public Alliance Alliance { get; set; }
        public Player Player { get; set; }
    }

    public enum AllianceRole
    {
        Leader,
        DeputyLeader,
        Officer,
        Member,
        Recruit
    }
}