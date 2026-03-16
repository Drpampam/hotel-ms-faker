using hotelier_core_app.Model.Attributes;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing a user token for an application user.
    /// </summary>
    [Table("UserToken")]
    [TableName("UserToken")]
    [Serializable]
    public class ApplicationUserToken : IdentityUserToken<long>
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user token.
        /// </summary>
        public long Id { get; set; }
    }
}
