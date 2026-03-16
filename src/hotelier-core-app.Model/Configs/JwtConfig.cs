namespace hotelier_core_app.Model.Configs
{
    /// <summary>
    /// Configuration settings for JWT authentication.
    /// </summary>
    public class JwtConfig
    {
        /// <summary>
        /// The secret key used to sign JWT tokens.
        /// </summary>
        public required string TokenKey { get; set; }

        /// <summary>
        /// The issuer of the JWT token.
        /// </summary>
        public required string TokenIssuer { get; set; }

        /// <summary>
        /// The expiry period for the JWT token.
        /// </summary>
        public required string TokenExpiryPeriod { get; set; }
    }
}
