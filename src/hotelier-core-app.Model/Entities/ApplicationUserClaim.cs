using hotelier_core_app.Model.Attributes;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotelier_core_app.Model.Entities
{
    /// <summary>
    /// Entity representing a user claim for an application user.
    /// </summary>
    [Table("UserClaim")]
    [TableName("UserClaim")]
    [Serializable]
    public class ApplicationUserClaim : IdentityUserClaim<long>
    {
        // No additional members required; inherits from IdentityUserClaim.
    }
}
