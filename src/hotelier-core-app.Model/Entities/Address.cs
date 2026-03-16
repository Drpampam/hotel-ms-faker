using hotelier_core_app.Model.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing an address record.
    /// </summary>
    [Table("Address")]
    [TableName("Address")]
    [Serializable]
    public class Address
    {
        /// <summary>
        /// Gets or sets the unique identifier for the address.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the street name of the address.
        /// </summary>
        [StringLength(100)]
        public string? Street { get; set; }

        /// <summary>
        /// Gets or sets the city of the address.
        /// </summary>
        [StringLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// Gets or sets the state of the address.
        /// </summary>
        [StringLength(100)]
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the zip code of the address.
        /// </summary>
        [StringLength(50)]
        public string? ZipCode { get; set; }

        /// <summary>
        /// Gets or sets the country of the address.
        /// </summary>
        [StringLength(150)]
        public string? Country { get; set; }

        /// <summary>
        /// Gets or sets the latitude coordinate of the address.
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude coordinate of the address.
        /// </summary>
        public decimal Longitude { get; set; }
    }
}
