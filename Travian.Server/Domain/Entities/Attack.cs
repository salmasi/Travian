namespace YourProjectName.Domain.Entities
{
    public class Attack
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid AttackerVillageId { get; set; }
        public Guid DefenderVillageId { get; set; }
        
        public AttackType Type { get; set; }
        public AttackStatus Status { get; set; } = AttackStatus.Pending;
        
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public DateTime? ReturnTime { get; set; }
        
        public int? LootWood { get; set; }
        public int? LootClay { get; set; }
        public int? LootIron { get; set; }
        public int? LootCrop { get; set; }
        
        // روابط
        public Village AttackerVillage { get; set; }
        public Village DefenderVillage { get; set; }
        public virtual ICollection<AttackTroop> Troops { get; set; } = new List<AttackTroop>();
        public virtual ICollection<BattleReport> Reports { get; set; } = new List<BattleReport>();
    }

    public enum AttackType
    {
        Raid,
        Attack,
        Reinforcement,
        Scout
    }

    public enum AttackStatus
    {
        Pending,
        Ongoing,
        Completed,
        Returned,
        Canceled
    }
}