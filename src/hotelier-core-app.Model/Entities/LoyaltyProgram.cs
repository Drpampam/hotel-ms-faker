using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing a loyalty program entry for a user.
    /// </summary>
    [Table("LoyaltyProgram")]
    [TableName("LoyaltyProgram")]
    [Serializable]
    public class LoyaltyProgram : IBaseEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the loyalty program entry.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the number of points earned by the user.
        /// </summary>
        public int PointsEarned { get; set; }

        /// <summary>
        /// Gets or sets the number of points redeemed by the user.
        /// </summary>
        public int PointsRedeemed { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who created the loyalty program entry.
        /// </summary>
        [StringLength(200)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who last modified the loyalty program entry.
        /// </summary>
        [StringLength(200)]
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the loyalty program entry.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last modified date of the loyalty program entry.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the loyalty program entry is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the user ID associated with the loyalty program entry.
        /// </summary>
        [ForeignKey("User")]
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the user entity associated with the loyalty program entry.
        /// </summary>
        public ApplicationUser? User { get; set; }
    }
}
