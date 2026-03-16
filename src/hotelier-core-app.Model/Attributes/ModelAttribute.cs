namespace hotelier_core_app.Model.Attributes
{
    /// <summary>
    /// Attribute applied to properties that should be ignored during insert or update operations.
    /// Use this to prevent certain properties from being persisted to the database when creating or updating records.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IgnoreDuringInsertOrUpdateAttribute : Attribute
    {
        // No members required; marker attribute only.
    }

    /// <summary>
    /// Attribute applied to classes that are mapped to database tables.
    /// Use this to specify the table name for an entity class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableNameAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the table mapped to the class.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableNameAttribute"/> class with the specified table name.
        /// </summary>
        /// <param name="name">The name of the table to map the class to.</param>
        public TableNameAttribute(string name)
        {
            Name = name;
        }
    }
}
