namespace hotelier_core_app.Domain.Helpers
{
    /// <summary>
    /// Specifies the supported database providers for the application.
    /// </summary>
    public enum DBProvider
    {
        /// <summary>
        /// Dapper-based SQL provider.
        /// </summary>
        SQL_Dapper = 1,
        /// <summary>
        /// Entity Framework Core-based SQL provider.
        /// </summary>
        SQL_EFCore
    }
}
