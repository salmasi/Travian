using System.ComponentModel.DataAnnotations;

namespace YourProjectName.Domain.Entities
{
    public class Building
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public BuildingType Type { get; set; }

        [Required]
        public int Level { get; set; } = 1;

        public DateTime? UpgradeStartTime { get; set; }
        public DateTime? UpgradeEndTime { get; set; }

        // Foreign Key
        public Guid VillageId { get; set; }

        // Navigation Property
        public virtual Village Village { get; set; }
    }

    public enum BuildingType
    {
        TownHall,
        Woodcutter,
        ClayPit,
        IronMine,
        CropFarm,
        Warehouse,
        Granary,
        Barracks,
        Stable,
        Workshop
    }
}