namespace hotelier_core_app.Migrations
{
    /// <summary>
    /// Interface for providing and setting tenant schema information.
    /// </summary>
    public interface ITenantProvider
    {
        /// <summary>
        /// Returns the schema name for the current tenant.
        /// </summary>
        /// <returns>The schema name.</returns>
        string GetSchema();

        /// <summary>
        /// Sets the schema for the current tenant.
        /// </summary>
        /// <param name="schema">The schema name to set.</param>
        void SetSchema(string schema);
    }
}