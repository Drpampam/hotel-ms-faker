namespace hotelier_core_app.Migrations
{
    /// <summary>
    /// Provides tenant schema management using AsyncLocal for multi-tenancy.
    /// </summary>
    public class TenantProvider : ITenantProvider
    {
        private static readonly AsyncLocal<string> _schema = new AsyncLocal<string>();

        /// <summary>
        /// Gets the current tenant schema.
        /// </summary>
        /// <returns>The schema name.</returns>
        public string GetSchema() => _schema.Value ?? "public";

        /// <summary>
        /// Sets the tenant schema.
        /// </summary>
        /// <param name="schema">The schema name to set.</param>
        public void SetSchema(string schema) => _schema.Value = schema;
    }
}