using System.ComponentModel.DataAnnotations;

namespace YourProjectName.Domain.Entities
{
    public class Village
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = "روستای اصلی";

        [Required]
        public int X { get; set; }

        [Required]
        public int Y { get; set; }

        public bool IsCapital { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public Guid PlayerId { get; set; }

        // Navigation Properties
        public virtual Player Player { get; set; }
        public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();
        public virtual ICollection<Troop> Troops { get; set; } = new List<Troop>();
        public virtual Resources Resources { get; set; }
    }
}