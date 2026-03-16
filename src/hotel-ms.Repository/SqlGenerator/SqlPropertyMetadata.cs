using hotelier_core_app.Domain.Attributes;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace hotelier_core_app.Domain.SqlGenerator
{
    public class SqlPropertyMetadata
    {
        public PropertyInfo PropertyInfo { get; }

        public string Alias { get; set; }

        public string ColumnName { get; set; }

        public string CleanColumnName { get; set; }

        public bool IgnoreUpdate { get; set; }

        public bool IsNullable { get; set; }

        public bool IsUnique { get; set; }

        public bool IsClustered { get; set; }

        public virtual string PropertyName => PropertyInfo.Name;

        public SqlPropertyMetadata(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            ColumnAttribute customAttribute = PropertyInfo.GetCustomAttribute<ColumnAttribute>();
            if (!string.IsNullOrEmpty(customAttribute?.Name))
            {
                Alias = customAttribute.Name;
                ColumnName = Alias;
            }
            else
            {
                ColumnName = PropertyInfo.Name;
            }

            CleanColumnName = ColumnName;
            if (PropertyInfo.GetCustomAttribute<IgnoreDuringUpdateAttribute>() != null)
            {
                IgnoreUpdate = true;
            }

            IsNullable = propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}