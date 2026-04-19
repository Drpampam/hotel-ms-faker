using hotelier_core_app.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    [Table("ActivationCode")]
    public class ActivationCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(64)]
        public string CodeHash { get; set; } = string.Empty;

        [Required]
        public PlanType PlanType { get; set; }

        [Required]
        [StringLength(200)]
        public string BoundToEmail { get; set; } = string.Empty;

        public bool IsUsed { get; set; } = false;

        public long? UsedByTenantId { get; set; }

        public DateTime? UsedAt { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(200)]
        public string GeneratedBy { get; set; } = string.Empty;
    }
}
