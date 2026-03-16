using hotelier_core_app.Model.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing an audit log entry.
    /// </summary>
    [Table("AuditLog")]
    [TableName("AuditLog")]
    [Serializable]
    public class AuditLog
    {
        /// <summary>
        /// Gets or sets the unique identifier for the audit log entry.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the device type used for the action.
        /// </summary>
        [StringLength(200)]
        public string? Devicetype { get; set; }

        /// <summary>
        /// Gets or sets the IP address from which the action was performed.
        /// </summary>
        [StringLength(200)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the MAC address from which the action was performed.
        /// </summary>
        [StringLength(200)]
        public string? MacAddress { get; set; }

        /// <summary>
        /// Gets or sets the latitude coordinate of the action.
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude coordinate of the action.
        /// </summary>
        public decimal Longitude { get; set; }

        /// <summary>
        /// Gets or sets the location description of the action.
        /// </summary>
        [StringLength(500)]
        public string? Location { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who performed the action.
        /// </summary>
        [StringLength(200)]
        public string? PerformedBy { get; set; }

        /// <summary>
        /// Gets or sets the email of the user who performed the action.
        /// </summary>
        [StringLength(200)]
        public string? PerformerEmail { get; set; }

        /// <summary>
        /// Gets or sets the entity against which the action was performed.
        /// </summary>
        [StringLength(255)]
        public string? PerformedAgainst { get; set; }

        /// <summary>
        /// Gets or sets the action performed.
        /// </summary>
        [StringLength(200)]
        public string? Action { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the action was performed.
        /// </summary>
        public DateTime DatePerformed { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLog"/> class.
        /// </summary>
        public AuditLog() { }
    }
}
