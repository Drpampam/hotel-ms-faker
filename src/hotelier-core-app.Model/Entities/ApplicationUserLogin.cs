using hotelier_core_app.Model.Attributes;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing a user login for an application user.
    /// </summary>
    [Table("UserLogin")]
    [TableName("UserLogin")]
    [Serializable]
    public class ApplicationUserLogin : IdentityUserLogin<long>
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user login.
        /// </summary>
        public long Id { get; set; }
    }
}
