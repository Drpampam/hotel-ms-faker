namespace hotelier_core_app.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IgnoreDuringInsertAttribute : Attribute
    {
    }
}
