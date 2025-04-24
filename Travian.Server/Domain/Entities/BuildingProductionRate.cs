using System.ComponentModel.DataAnnotations;

namespace YourProjectName.Domain.Entities
{
    public class BuildingProductionRate
    {
        [Key]
        public BuildingType BuildingType { get; set; }
        
        public int BaseProduction { get; set; }
        public float ProductionFactor { get; set; }
        public int BaseStorage { get; set; }
        public float StorageFactor { get; set; }
    }
}