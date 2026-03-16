namespace hotelier_core_app.Domain.Attributes
{
    public sealed class CrossJoinAttribute : JoinAttributeBase
    {
        public CrossJoinAttribute()
        {
        }

        public CrossJoinAttribute(string tableName)
            : base(tableName, string.Empty, string.Empty, string.Empty, string.Empty, "CROSS JOIN")
        {
        }
    }
}
