using System.ComponentModel.DataAnnotations;

namespace YourProjectName.Domain.Entities
{
    public class Resources
    {
        [Key]
        public Guid VillageId { get; set; }

        public int Wood { get; set; } = 500;
        public int Clay { get; set; } = 500;
        public int Iron { get; set; } = 500;
        public int Crop { get; set; } = 500;

        public int WarehouseCapacity { get; set; } = 1000;
        public int GranaryCapacity { get; set; } = 1000;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual Village Village { get; set; }
    }
}