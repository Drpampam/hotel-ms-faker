namespace hotelier_core_app.Domain.Attributes
{
    public abstract class JoinAttributeBase : Attribute
    {
        private string JoinAttribute { get; }

        public string TableName { get; set; }

        public string TableSchema { get; set; }

        public string Key { get; set; }

        public string ExternalKey { get; set; }

        public string TableAlias { get; set; }

        public JoinAttributeBase()
        {
        }

        protected JoinAttributeBase(string tableName, string key, string externalKey, string tableSchema, string tableAlias, string attrString = "JOIN")
        {
            TableName = "public.\"" + tableName + "\""; ;
            Key = key;
            ExternalKey = externalKey;
            TableSchema = tableSchema;
            TableAlias = tableAlias;
            JoinAttribute = attrString;
        }

        public override string ToString()
        {
            return JoinAttribute;
        }
    }
}
