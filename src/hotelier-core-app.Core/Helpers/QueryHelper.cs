namespace hotelier_core_app.Core.Helpers
{
    /// <summary>
    /// Helper class for generating query string parameters from objects.
    /// </summary>
    public class QueryHelper
    {
        /// <summary>
        /// Generates a query string from the properties of the given object.
        /// </summary>
        /// <param name="queryObject">The object containing query parameters.</param>
        /// <returns>A query string representing the object's properties.</returns>
        public static string GenerateQueryParams(object queryObject)
        {
            string query = string.Empty;
            if (queryObject != null)
            {
                foreach (var prop in queryObject.GetType().GetProperties())
                {
                    var propValue = prop.GetValue(queryObject);
                    //For more object type coverage, please add additional validation
                    if (propValue != null
                        && ((prop.PropertyType == typeof(string) && !string.IsNullOrWhiteSpace((string)propValue))
                        || (prop.PropertyType == typeof(long) && (long)propValue != 0))
                        || (prop.PropertyType == typeof(bool) && propValue != null && (bool)propValue != false))
                    {
                        if (query == string.Empty)
                            query += "?" + prop.Name + "=" + propValue;
                        else query += "&" + prop.Name + "=" + propValue;
                    }
                }
            }
            return query;
        }
    }
}
