namespace YourProjectName.Domain.Entities
{
    public class AttackTroop
    {
        public Guid AttackId { get; set; }
        public string TroopType { get; set; }
        
        public int Sent { get; set; }
        public int? Lost { get; set; }
        public int? Returned { get; set; }
        
        // روابط
        public Attack Attack { get; set; }
    }
}