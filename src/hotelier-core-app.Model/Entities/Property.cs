using hotelier_core_app.Model.Attributes;
using hotelier_core_app.Model.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    [Table("Property")]
    [TableName("Property")]
    [Serializable]
    /// <summary>
    /// Represents a property entity in the system.
    /// </summary>
    public class Property : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        /// <summary>
        /// Gets or sets the unique identifier for the property.
        /// </summary>
        public long Id { get; set; }

        [StringLength(255)]
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string? Name { get; set; }

        [StringLength(500)]
        /// <summary>
        /// Gets or sets the description of the property.
        /// </summary>
        public string? Description { get; set; }

        [StringLength(255)]
        /// <summary>
        /// Gets or sets the image URL of the property.
        /// </summary>
        public string? Image { get; set; }

        [StringLength(200)]
        /// <summary>
        /// Gets or sets the creator of the property record.
        /// </summary>
        public string? CreatedBy { get; set; }

        [StringLength(200)]
        /// <summary>
        /// Gets or sets the last modifier of the property record.
        /// </summary>
        public string? ModifiedBy { get; set; }
        /// <summary>
        /// Gets or sets the creation date of the property record.
        /// </summary>
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// Gets or sets the last modification date of the property record.
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the property is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        [ForeignKey("Tenant")]
        /// <summary>
        /// Gets or sets the tenant ID associated with the property.
        /// </summary>
        public long TenantId { get; set; }
        /// <summary>
        /// Gets or sets the tenant entity associated with the property.
        /// </summary>
        public Tenant? Tenant { get; set; }

        [ForeignKey("Address")]
        /// <summary>
        /// Gets or sets the address ID associated with the property.
        /// </summary>
        public long AddressId { get; set; }
        /// <summary>
        /// Gets or sets the address entity associated with the property.
        /// </summary>
        public Address? Address { get; set; }

        /// <summary>
        /// Gets or sets the collection of rooms associated with the property.
        /// </summary>
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
