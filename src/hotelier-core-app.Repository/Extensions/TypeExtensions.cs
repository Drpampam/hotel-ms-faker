using hotelier_core_app.Domain.Attributes;
using hotelier_core_app.Domain.SqlGenerator;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace hotelier_core_app.Domain.Extensions
{
    internal static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _reflectionPropertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();

        private static readonly ConcurrentDictionary<Type, SqlPropertyMetadata[]> _metaDataPropertyCache = new ConcurrentDictionary<Type, SqlPropertyMetadata[]>();

        /// <summary>
        /// Finds and caches all properties of the specified class type.
        /// </summary>
        /// <param name="objectType">The type of the class to inspect.</param>
        /// <returns>An array of PropertyInfo representing the class properties.</returns>
        public static PropertyInfo[] FindClassProperties(this Type objectType)
        {
            if (_reflectionPropertyCache.TryGetValue(objectType, out PropertyInfo[]? value))
            {
                return value ?? Array.Empty<PropertyInfo>();
            }

            PropertyInfo[] properties = objectType.GetProperties();
            _reflectionPropertyCache.TryAdd(objectType, properties);
            return properties;
        }

        /// <summary>
        /// Finds and caches metadata properties of the specified class type, ordered by identity, key, and column order.
        /// </summary>
        /// <param name="objectType">The type of the class to inspect.</param>
        /// <returns>An array of SqlPropertyMetadata representing the metadata properties.</returns>
        public static SqlPropertyMetadata[] FindClassMetaDataProperties(this Type objectType)
        {
            if (_metaDataPropertyCache.TryGetValue(objectType, out SqlPropertyMetadata[]? value))
            {
                return value ?? Array.Empty<SqlPropertyMetadata>();
            }

            SqlPropertyMetadata[] array = (from p in (from x in objectType.GetProperties()
                                                      orderby x.GetCustomAttribute<IdentityAttribute>() != null descending, x.GetCustomAttribute<KeyAttribute>() != null descending
                                                      select x).ThenBy((PropertyInfo p) => (from a in p.GetCustomAttributes<ColumnAttribute>()
                                                                                            select a.Order).DefaultIfEmpty(int.MaxValue).FirstOrDefault()).Where(ExpressionHelper.GetPrimitivePropertiesPredicate())
                                           where !p.GetCustomAttributes<NotMappedAttribute>().Any()
                                           select new SqlPropertyMetadata(p)).ToArray();
            _metaDataPropertyCache.TryAdd(objectType, array);
            return array;
        }

        /// <summary>
        /// Finds and caches navigation property metadata for the specified class type, excluding keys and 'id' properties.
        /// </summary>
        /// <param name="objectType">The type of the class to inspect.</param>
        /// <returns>An array of SqlPropertyMetadata representing navigation properties.</returns>
        public static SqlPropertyMetadata[] GetNavigationPropertyMetaDataProperties(this Type objectType)
        {
            if (_metaDataPropertyCache.TryGetValue(objectType, out SqlPropertyMetadata[]? value))
            {
                return value ?? Array.Empty<SqlPropertyMetadata>();
            }

            SqlPropertyMetadata[] array = (from p in objectType.GetProperties()
                                           where !p.GetCustomAttributes<NotMappedAttribute>().Any() && !p.GetCustomAttributes<KeyAttribute>().Any() && p.Name.ToLower() != "id"
                                           select new SqlPropertyMetadata(p)).ToArray();
            _metaDataPropertyCache.TryAdd(objectType, array);
            return array;
        }

        /// <summary>
        /// Returns the underlying type if the specified type is nullable; otherwise, returns the type itself.
        /// </summary>
        /// <param name="type">The type to unwrap.</param>
        /// <returns>The underlying type if nullable, or the original type.</returns>
        public static Type UnwrapNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }
    }
}
