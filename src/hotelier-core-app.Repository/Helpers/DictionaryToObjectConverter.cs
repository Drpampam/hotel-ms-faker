using System.Dynamic;
using System.Reflection;

namespace hotelier_core_app.Domain.Helpers
{
    public static class DictionaryToObjectConverter
    {
        /// <summary>
        /// Creates an instance of the specified class type and populates its properties from the dictionary.
        /// </summary>
        /// <typeparam name="T">The type of the class to create.</typeparam>
        /// <param name="dict">The dictionary containing property names and values.</param>
        /// <param name="type">The type of the class to instantiate.</param>
        /// <returns>An instance of type T with properties set from the dictionary.</returns>
        public static T? GetClassInstance<T>(this Dictionary<string, object> dict, Type type) where T : class
        {
            object? obj = Activator.CreateInstance(type);
            if (obj == null)
                return default;
            foreach (KeyValuePair<string, object> item in dict)
            {
                PropertyInfo? property = type.GetProperty(item.Key);
                if (property != null)
                {
                    property.SetValue(obj, item.Value, null);
                }
            }

            return obj as T;
        }

        /// <summary>
        /// Converts the dictionary to an anonymous object using ExpandoObject.
        /// </summary>
        /// <param name="dict">The dictionary to convert.</param>
        /// <returns>An ExpandoObject representing the dictionary as an anonymous object.</returns>
        public static object ConvertToAnonymousObject(this Dictionary<string, object> dict)
        {
            ExpandoObject expandoObject = new ExpandoObject();
            // The cast to IDictionary<string, object> is safe for ExpandoObject
            var collection = (IDictionary<string, object>)expandoObject;
            foreach (KeyValuePair<string, object> item in dict)
            {
                collection.Add(item.Key, item.Value);
            }

            return expandoObject;
        }
    }
}
