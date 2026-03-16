using hotelier_core_app.Domain.Attributes;
using hotelier_core_app.Domain.Extensions;
using hotelier_core_app.Domain.Helpers;
using hotelier_core_app.Domain.SqlGenerator.QueryExpressions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace hotelier_core_app.Domain.SqlGenerator
{
    public class SqlGenerator<TEntity> : ISqlGenerator<TEntity>, IAutoDependencyRepository where TEntity : class
    {
        public PropertyInfo[] NavigationProperties { get; protected set; }

        public PropertyInfo[] InsertProperties { get; protected set; }

        public SqlPropertyMetadata[] SearchableProperties { get; protected set; }

        public string TableName { get; protected set; }

        public Type IdType { get; protected set; }

        public SqlPropertyMetadata[] KeyProperties { get; protected set; }

        public PropertyInfo[] LastUpdatedTimeTrackingProperties { get; protected set; }

        public SqlGenerator()
        {
            Initialize();
        }

        public SqlQuery GetInsertQuery(TEntity entity)
        {
            SqlQuery sqlQuery = new SqlQuery(entity);
            sqlQuery.SqlBuilder.AppendFormat(
                "INSERT INTO {0} ({1}) VALUES ({2})",
                TableName,
                string.Join(", ", InsertProperties.Select(p => $"\"{p.Name}\"")),
                string.Join(", ", InsertProperties.Select(p => $"@{p.Name}"))
            );

            if (IsNumericType(IdType))
            {
                sqlQuery.SqlBuilder.Append(" RETURNING \"Id\"");
            }

            return sqlQuery;
        }

        public object GetInsertQueryParams(TEntity entity)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (PropertyInfo propertyInfo in InsertProperties)
            {
                dictionary.Add(propertyInfo.Name, propertyInfo.GetValue(entity));
            }

            return dictionary.ConvertToAnonymousObject();
        }

        public SqlQuery GetSelectQuery(Expression<Func<TEntity, bool>> predicate, int limit = 0, params Expression<Func<TEntity, object>>[] includes)
        {
            SqlQuery selectAllQuery = GetSelectAllQuery(limit, includes);
            AppendWherePredicateQuery(selectAllQuery, predicate);
            return selectAllQuery;
        }

        public SqlQuery GetSelectAllQuery(int limit = 0, params Expression<Func<TEntity, object>>[] includes)
        {
            SqlQuery sqlQuery = new SqlQuery();
            if (limit <= 0)
            {
                sqlQuery.SqlBuilder.Append("SELECT *");
            }
            else
            {
                sqlQuery.SqlBuilder.Append($"SELECT * LIMIT {limit}");
            }

            if (includes.Length != 0)
            {
                string value = AppendJoinToSelect(sqlQuery, includes);
                sqlQuery.SqlBuilder.Append(" FROM ").Append(TableName).Append(" ");
                sqlQuery.SqlBuilder.Append(value);
            }
            else
            {
                sqlQuery.SqlBuilder.Append(" FROM ").Append(TableName).Append(" ");
            }

            return sqlQuery;
        }

        public SqlQuery GetSelectById(object id)
        {
            if (KeyProperties.Length != 1)
            {
                throw new NotSupportedException("GetSelectById supports only 1 key");
            }

            SqlPropertyMetadata sqlPropertyMetadata = KeyProperties[0];
            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.SqlBuilder.Append("SELECT * ");
            sqlQuery.SqlBuilder.Append(" FROM ").Append(TableName).Append(" ");
            IDictionary<string, object> param = new Dictionary<string, object> { { sqlPropertyMetadata.PropertyName, id } };
            sqlQuery.SqlBuilder.Append("WHERE ").Append(TableName).Append(".")
                .Append(sqlPropertyMetadata.ColumnName)
                .Append(" = @")
                .Append(sqlPropertyMetadata.PropertyName)
                .Append(" ");
            sqlQuery.SetParam(param);
            return sqlQuery;
        }

        public SqlQuery GetUpdateQuery(TEntity entity)
        {
            SqlPropertyMetadata[] array = SearchableProperties.Where((SqlPropertyMetadata p) => !KeyProperties
                .Any((SqlPropertyMetadata k) => k.PropertyName.Equals(p.PropertyName, StringComparison.OrdinalIgnoreCase)) &&
                !p.IgnoreUpdate).ToArray();

            if (!array.Any())
            {
                throw new ArgumentException("Can't update without [Key]");
            }

            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.SqlBuilder.Append("UPDATE ").Append(TableName).Append(" ");
            sqlQuery.SqlBuilder.Append("SET ");
            sqlQuery.SqlBuilder.Append(GetFieldsUpdate(TableName, array));
            sqlQuery.SqlBuilder.Append(" WHERE ");
            sqlQuery.SqlBuilder.Append(string.Join(" AND ", from p in KeyProperties
                                                            where !p.IgnoreUpdate
                                                            select $"{TableName}.[{p.ColumnName}] = @{p.PropertyName}"));
            Dictionary<string, object> dictionary;
            if (sqlQuery.Param != null)
            {
                dictionary = sqlQuery.Param as Dictionary<string, object>;
                if (dictionary != null)
                {
                    goto IL_0105;
                }
            }

            dictionary = new Dictionary<string, object>();
            goto IL_0105;
        IL_0105:
            foreach (SqlPropertyMetadata item in array.Concat(KeyProperties))
            {
                string propertyName = item.PropertyName;
                object value = entity.GetType().GetProperty(item.PropertyName).GetValue(entity, null);
                if (LastUpdatedTimeTrackingProperties.Contains(item.PropertyInfo) && item.PropertyInfo.PropertyType == typeof(DateTime))
                {
                    value = DateTime.UtcNow;
                }

                dictionary.Add(propertyName, value);
            }

            sqlQuery.SetParam(dictionary);
            return sqlQuery;
        }

        public SqlQuery GetCount(Expression<Func<TEntity, bool>> predicate)
        {
            SqlQuery sqlQuery = new SqlQuery();
            sqlQuery.SqlBuilder.Append("SELECT COUNT(*)");
            sqlQuery.SqlBuilder.Append(" FROM ").Append(TableName).Append(" ");
            AppendWherePredicateQuery(sqlQuery, predicate);
            return sqlQuery;
        }

        private string GetFieldsUpdate(string tableName, IEnumerable<SqlPropertyMetadata> properties)
        {
            string tableName2 = tableName;
            return string.Join(", ", properties.Select((SqlPropertyMetadata p) => $"{tableName2}.[{p.CleanColumnName}] = @{p.PropertyName}"));
        }

        public SqlQuery GetUniqueSelectQuery(TEntity entity)
        {
            Type typeFromHandle = typeof(TEntity);
            typeFromHandle.GetTypeInfo();
            SqlPropertyMetadata[] array = (from p in typeFromHandle.FindClassProperties()
                                           where p.GetCustomAttributes<UniqueColumnAttribute>().Any()
                                           select new SqlPropertyMetadata(p)).ToArray();
            List<string> list = new List<string>();
            SqlPropertyMetadata[] array2 = array;
            foreach (SqlPropertyMetadata sqlPropertyMetadata in array2)
            {
                list.Add(sqlPropertyMetadata.ColumnName + " = @" + sqlPropertyMetadata.ColumnName);
            }

            SqlQuery sqlQuery = new SqlQuery(entity);
            sqlQuery.SqlBuilder.AppendFormat("SELECT * FROM {0} WHERE {1} LIMIT 1", TableName, string.Join(" OR ", list.ToArray()));
            return sqlQuery;
        }

        public SqlQuery GetBulkInsertQuery(IEnumerable<TEntity> entities)
        {
            TEntity[] array = (entities as TEntity[]) ?? entities.ToArray();
            if (!array.Any())
            {
                throw new ArgumentException("collection is empty");
            }

            Type type = array[0].GetType();
            SqlQuery sqlQuery = new SqlQuery();
            List<string> list = new List<string>();
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            int i;
            for (i = 0; i < array.Length; i++)
            {
                TEntity obj = array[i];
                PropertyInfo[] insertProperties = InsertProperties;
                foreach (PropertyInfo propertyInfo in insertProperties)
                {
                    object value = type.GetProperty(propertyInfo.Name).GetValue(obj, null);
                    dictionary.Add(propertyInfo.Name + i, value);
                }

                list.Add(string.Format("({0})", string.Join(", ", InsertProperties.Select((PropertyInfo p) => "@" + p.Name + i))));
            }

            sqlQuery.SqlBuilder.AppendFormat("INSERT INTO {0} ({1}) VALUES {2}", TableName, string.Join(", ", InsertProperties.Select((PropertyInfo p) => $"{p.Name}")), string.Join(", ", list));
            sqlQuery.SetParam(dictionary);
            return sqlQuery;
        }

        public SqlQuery GetBulkUpdateQuery(IEnumerable<TEntity> entities)
        {
            TEntity[] array = (entities as TEntity[]) ?? entities.ToArray();
            if (!array.Any())
            {
                throw new ArgumentException("collection is empty");
            }

            Type type = array[0].GetType();
            SqlPropertyMetadata[] array2 = SearchableProperties.Where((SqlPropertyMetadata p) => !KeyProperties.Any((SqlPropertyMetadata k) => k.PropertyName.Equals(p.PropertyName, StringComparison.OrdinalIgnoreCase)) && !p.IgnoreUpdate).ToArray();
            SqlQuery sqlQuery = new SqlQuery();
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            DateTime utcNow = DateTime.UtcNow;
            int i;
            for (i = 0; i < array.Length; i++)
            {
                TEntity obj = array[i];
                if (i > 0)
                {
                    sqlQuery.SqlBuilder.Append("; ");
                }

                string value = string.Join(", ", array2.Select((SqlPropertyMetadata p) => $"{p.ColumnName} = @{p.PropertyName}{i}"));
                string value2 = string.Join(" AND ", from p in KeyProperties
                                                     where !p.IgnoreUpdate
                                                     select $"{p.ColumnName} = @{p.PropertyName}{i}");
                StringBuilder sqlBuilder = sqlQuery.SqlBuilder;
                StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(19, 3, sqlBuilder);
                handler.AppendLiteral("UPDATE ");
                handler.AppendFormatted(TableName);
                handler.AppendLiteral(" SET ");
                handler.AppendFormatted(value);
                handler.AppendLiteral(" WHERE ");
                handler.AppendFormatted(value2);
                sqlBuilder.Append(ref handler);
                SqlPropertyMetadata[] array3 = array2;
                foreach (SqlPropertyMetadata sqlPropertyMetadata in array3)
                {
                    string key = sqlPropertyMetadata.PropertyName + i;
                    object value3 = type.GetProperty(sqlPropertyMetadata.PropertyName).GetValue(obj, null);
                    if (LastUpdatedTimeTrackingProperties.Contains(sqlPropertyMetadata.PropertyInfo) && sqlPropertyMetadata.GetType() == typeof(DateTime))
                    {
                        value3 = utcNow;
                    }

                    dictionary.Add(key, value3);
                }

                foreach (SqlPropertyMetadata item in KeyProperties.Where((SqlPropertyMetadata p) => !p.IgnoreUpdate))
                {
                    dictionary.Add(item.PropertyName + i, type.GetProperty(item.PropertyName).GetValue(obj, null));
                }
            }

            sqlQuery.SetParam(dictionary);
            return sqlQuery;
        }

        private void AppendWherePredicateQuery(SqlQuery sqlQuery, Expression<Func<TEntity, bool>> predicate)
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>();
            List<QueryExpression> queryProperties = GetQueryProperties(predicate.Body);
            sqlQuery.SqlBuilder.Append(" WHERE ");
            int qLevel = 0;
            StringBuilder sqlBuilder = new StringBuilder();
            List<KeyValuePair<string, object>> conditions = new List<KeyValuePair<string, object>>();
            BuildQuerySql(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);
            dictionary.AddRange(conditions);
            sqlQuery.SqlBuilder.AppendFormat("{0} ", sqlBuilder);
            sqlQuery.SetParam(dictionary);
        }

        private string AppendJoinToSelect(SqlQuery originalBuilder, params Expression<Func<TEntity, object>>[] includes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Expression<Func<TEntity, object>> include in includes)
            {
                PropertyInfo propertyInfo = NavigationProperties.First((PropertyInfo q) => q.Name == ExpressionHelper.GetPropertyName(include));
                JoinAttributeBase customAttribute = propertyInfo.GetCustomAttribute<JoinAttributeBase>();
                if (customAttribute != null)
                {
                    SqlPropertyMetadata[] navigationPropertyMetaDataProperties = (propertyInfo.PropertyType.IsGenericType ? propertyInfo.PropertyType.GenericTypeArguments[0] : propertyInfo.PropertyType).GetNavigationPropertyMetaDataProperties();
                    customAttribute.TableName = GetTableNameWithSchemaPrefix(customAttribute.TableName, customAttribute.TableSchema);
                    StringBuilder sqlBuilder = originalBuilder.SqlBuilder;
                    StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 1, sqlBuilder);
                    handler.AppendLiteral(", ");
                    handler.AppendFormatted(GetFieldsSelect(string.IsNullOrEmpty(customAttribute.TableAlias) ? customAttribute.TableName : customAttribute.TableAlias, navigationPropertyMetaDataProperties));
                    handler.AppendLiteral(" ");
                    sqlBuilder.Append(ref handler);
                    AppendJoinQuery(customAttribute, stringBuilder, TableName);
                }
            }

            return stringBuilder.ToString();
        }

        private void AppendJoinQuery(JoinAttributeBase attrJoin, StringBuilder joinBuilder, string tableName)
        {
            string text = attrJoin.ToString();
            if (attrJoin is CrossJoinAttribute)
            {
                joinBuilder.Append((attrJoin.TableAlias == string.Empty) ? (text + " " + attrJoin.TableName + " ") : $"{text} {attrJoin.TableName} AS {attrJoin.TableAlias} ");
                return;
            }

            joinBuilder.Append((attrJoin.TableAlias == string.Empty) ? $"{text} {attrJoin.TableName} ON {tableName}.{attrJoin.Key} = {attrJoin.TableName}.{attrJoin.ExternalKey}" : $"{text} {attrJoin.TableName} AS {attrJoin.TableAlias} ON {tableName}.{attrJoin.Key} = {attrJoin.TableAlias}.{attrJoin.ExternalKey}");
        }

        private static string GetTableNameWithSchemaPrefix(string tableName, string tableSchema, string startQuotationMark = "", string endQuotationMark = "")
        {
            if (string.IsNullOrEmpty(tableSchema))
            {
                return startQuotationMark + tableName + endQuotationMark;
            }

            return startQuotationMark + tableSchema + endQuotationMark + "." + startQuotationMark + tableName + endQuotationMark;
        }

        private static string GetFieldsSelect(string tableName, IEnumerable<SqlPropertyMetadata> properties)
        {
            string tableName2 = tableName;
            return string.Join(", ", properties.Select(ProjectionFunction));
            string ProjectionFunction(SqlPropertyMetadata p)
            {
                if (!string.IsNullOrEmpty(p.Alias))
                {
                    return $"{tableName2}.[{p.CleanColumnName}] AS {p.PropertyName}";
                }

                return tableName2 + ".[" + p.CleanColumnName + "]";
            }
        }

        private List<QueryExpression> GetQueryProperties(Expression expr)
        {
            QueryExpression queryProperties = GetQueryProperties(expr, ExpressionType.Default);
            if (!(queryProperties is QueryParameterExpression))
            {
                if (queryProperties is QueryBinaryExpression queryBinaryExpression)
                {
                    return queryBinaryExpression.Nodes;
                }

                throw new NotSupportedException(queryProperties.ToString());
            }

            return new List<QueryExpression> { queryProperties };
        }

        private QueryExpression GetQueryProperties(Expression expr, ExpressionType linkingType)
        {
            bool isNotUnary = false;
            if (expr is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Not && unaryExpression.Operand is MethodCallExpression)
            {
                expr = unaryExpression.Operand;
                isNotUnary = true;
            }

            if (expr is MethodCallExpression methodCallExpression)
            {
                string text = methodCallExpression.Method.Name;
                Expression @object = methodCallExpression.Object;
                while (true)
                {
                    switch (text)
                    {
                        case "Contains":
                            {
                                if (@object != null && @object.NodeType == ExpressionType.MemberAccess && @object.Type == typeof(string))
                                {
                                    goto IL_00de;
                                }

                                bool nested2;
                                string propertyNamePath2 = ExpressionHelper.GetPropertyNamePath(methodCallExpression, out nested2);
                                if (!SearchableProperties.Select((SqlPropertyMetadata x) => x.PropertyName).Contains(propertyNamePath2))
                                {
                                    throw new NotSupportedException("predicate can't parse");
                                }

                                object valuesFromCollection = ExpressionHelper.GetValuesFromCollection(methodCallExpression);
                                string methodCallSqlOperator2 = ExpressionHelper.GetMethodCallSqlOperator(text, isNotUnary);
                                return new QueryParameterExpression(ExpressionHelper.GetSqlOperator(linkingType), propertyNamePath2, valuesFromCollection, methodCallSqlOperator2, nested2);
                            }
                        case "StringContains":
                        case "CompareString":
                        case "Equals":
                        case "StartsWith":
                        case "EndsWith":
                            if (@object != null && @object.NodeType == ExpressionType.MemberAccess)
                            {
                                bool nested;
                                string propertyNamePath = ExpressionHelper.GetPropertyNamePath(@object, out nested);
                                if (!SearchableProperties.Select((SqlPropertyMetadata x) => x.PropertyName).Contains(propertyNamePath))
                                {
                                    throw new NotSupportedException("predicate can't parse");
                                }

                                object valuesFromStringMethod = ExpressionHelper.GetValuesFromStringMethod(methodCallExpression);
                                string sqlLikeValue = ExpressionHelper.GetSqlLikeValue(text, valuesFromStringMethod);
                                string methodCallSqlOperator = ExpressionHelper.GetMethodCallSqlOperator(text, isNotUnary);
                                return new QueryParameterExpression(ExpressionHelper.GetSqlOperator(linkingType), propertyNamePath, sqlLikeValue, methodCallSqlOperator, nested);
                            }

                            break;
                    }

                    break;
                IL_00de:
                    text = "StringContains";
                }

                throw new NotSupportedException("'" + text + "' method is not supported");
            }

            string propertyName;
            bool nested3;
            int num;
            if (expr is BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType != ExpressionType.AndAlso && binaryExpression.NodeType != ExpressionType.OrElse)
                {
                    propertyName = ExpressionHelper.GetPropertyNamePath(binaryExpression, out nested3);
                    if (nested3)
                    {
                        num = (propertyName.EndsWith("HasValue") ? 1 : 0);
                        if (num != 0)
                        {
                            SqlPropertyMetadata sqlPropertyMetadata = SearchableProperties.FirstOrDefault((SqlPropertyMetadata x) => x.IsNullable && x.PropertyName + "HasValue" == propertyName);
                            if (sqlPropertyMetadata != null)
                            {
                                nested3 = false;
                            }

                            propertyName = sqlPropertyMetadata.PropertyName;
                            goto IL_02c4;
                        }
                    }
                    else
                    {
                        num = 0;
                    }

                    if (!SearchableProperties.Select((SqlPropertyMetadata x) => x.PropertyName).Contains(propertyName))
                    {
                        throw new NotSupportedException("predicate can't parse");
                    }

                    goto IL_02c4;
                }

                QueryExpression queryExpression = GetQueryProperties(binaryExpression.Left, ExpressionType.Default);
                QueryExpression queryExpression2 = GetQueryProperties(binaryExpression.Right, binaryExpression.NodeType);
                if (!(queryExpression is QueryParameterExpression queryParameterExpression))
                {
                    if (queryExpression is QueryBinaryExpression queryBinaryExpression)
                    {
                        if (!(queryExpression2 is QueryParameterExpression queryParameterExpression2))
                        {
                            if (queryExpression2 is QueryBinaryExpression queryBinaryExpression2 && queryBinaryExpression.Nodes.Last().LinkingOperator == queryBinaryExpression2.LinkingOperator)
                            {
                                if (queryBinaryExpression2.LinkingOperator == queryBinaryExpression2.Nodes.Last().LinkingOperator)
                                {
                                    queryBinaryExpression2.Nodes[0].LinkingOperator = queryBinaryExpression2.LinkingOperator;
                                    queryBinaryExpression.Nodes.AddRange(queryBinaryExpression2.Nodes);
                                }
                                else
                                {
                                    queryBinaryExpression.Nodes.Add(queryBinaryExpression2);
                                }

                                queryExpression2 = null;
                            }
                        }
                        else if (queryParameterExpression2.LinkingOperator == queryBinaryExpression.Nodes.Last().LinkingOperator)
                        {
                            queryBinaryExpression.Nodes.Add(queryParameterExpression2);
                            queryExpression2 = null;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(queryParameterExpression.LinkingOperator) && !string.IsNullOrEmpty(queryExpression2.LinkingOperator) && queryExpression2 is QueryBinaryExpression queryBinaryExpression3 && queryParameterExpression.LinkingOperator == queryBinaryExpression3.Nodes.Last().LinkingOperator)
                {
                    QueryBinaryExpression obj = new QueryBinaryExpression
                    {
                        LinkingOperator = queryExpression.LinkingOperator,
                        Nodes = new List<QueryExpression> { queryExpression }
                    };
                    queryBinaryExpression3.Nodes[0].LinkingOperator = queryBinaryExpression3.LinkingOperator;
                    obj.Nodes.AddRange(queryBinaryExpression3.Nodes);
                    queryExpression = obj;
                    queryExpression2 = null;
                }

                string sqlOperator = ExpressionHelper.GetSqlOperator(linkingType);
                if (queryExpression2 == null)
                {
                    queryExpression.LinkingOperator = sqlOperator;
                    return queryExpression;
                }

                return new QueryBinaryExpression
                {
                    NodeType = QueryExpressionType.Binary,
                    LinkingOperator = sqlOperator,
                    Nodes = new List<QueryExpression> { queryExpression, queryExpression2 }
                };
            }

            return GetQueryProperties(GetBinaryExpression(expr), linkingType);
        IL_02c4:
            object obj2 = ExpressionHelper.GetValue(binaryExpression.Right);
            ExpressionType type = ((num == 0) ? binaryExpression.NodeType : ((!(bool)obj2) ? ExpressionType.Equal : ExpressionType.NotEqual));
            if (num != 0)
            {
                obj2 = null;
            }

            string sqlOperator2 = ExpressionHelper.GetSqlOperator(type);
            return new QueryParameterExpression(ExpressionHelper.GetSqlOperator(linkingType), propertyName, obj2, sqlOperator2, nested3);
        }

        private void BuildQuerySql(IList<QueryExpression> queryProperties, ref StringBuilder sqlBuilder, ref List<KeyValuePair<string, object>> conditions, ref int qLevel)
        {
            foreach (QueryExpression queryProperty in queryProperties)
            {
                if (!string.IsNullOrEmpty(queryProperty.LinkingOperator))
                {
                    if (sqlBuilder.Length > 0)
                    {
                        sqlBuilder.Append(" ");
                    }

                    sqlBuilder.Append(queryProperty.LinkingOperator).Append(" ");
                }

                QueryParameterExpression queryParameterExpression = queryProperty as QueryParameterExpression;
                if (queryParameterExpression == null)
                {
                    if (queryProperty is QueryBinaryExpression queryBinaryExpression)
                    {
                        StringBuilder sqlBuilder2 = new StringBuilder();
                        List<KeyValuePair<string, object>> conditions2 = new List<KeyValuePair<string, object>>();
                        BuildQuerySql(queryBinaryExpression.Nodes, ref sqlBuilder2, ref conditions2, ref qLevel);
                        if (queryBinaryExpression.Nodes.Count == 1)
                        {
                            sqlBuilder.Append(sqlBuilder2);
                        }
                        else
                        {
                            sqlBuilder.AppendFormat("({0})", sqlBuilder2);
                        }

                        conditions.AddRange(conditions2);
                    }

                    continue;
                }

                string tableName = TableName;
                string columnName = SearchableProperties.First((SqlPropertyMetadata x) => x.PropertyName == queryParameterExpression.PropertyName).ColumnName;
                if (queryParameterExpression.PropertyValue == null)
                {
                    sqlBuilder.AppendFormat("{0}.{1} {2} NULL", tableName, columnName, (queryParameterExpression.QueryOperator == "=") ? "IS" : "IS NOT");
                }
                else
                {
                    string text = $"{queryParameterExpression.PropertyName}_p{qLevel}";
                    sqlBuilder.AppendFormat("{0}.{1} {2} @{3}", tableName, columnName, queryParameterExpression.QueryOperator, text);
                    conditions.Add(new KeyValuePair<string, object>(text, queryParameterExpression.PropertyValue));
                }

                qLevel++;
            }
        }

        public static BinaryExpression GetBinaryExpression(Expression expression)
        {
            return (expression as BinaryExpression) ?? Expression.MakeBinary(ExpressionType.Equal, expression, (expression.NodeType == ExpressionType.Not) ? Expression.Constant(false) : Expression.Constant(true));
        }

        private static bool IsNumericType(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);
            if ((uint)(typeCode - 5) <= 10u)
            {
                return true;
            }

            return false;
        }

        private void Initialize()
        {
            Type typeFromHandle = typeof(TEntity);
            TypeInfo typeInfo = typeFromHandle.GetTypeInfo();
            TableAttribute customAttribute = typeInfo.GetCustomAttribute<TableAttribute>();
            TableName = ((!string.IsNullOrEmpty(customAttribute?.Schema)) ? customAttribute.Schema + "." : "") + ((!string.IsNullOrEmpty(customAttribute?.Name)) ? $"\"{customAttribute.Name}\"" : $"\"{typeInfo.Name}\"");
            PropertyInfo[] source = typeFromHandle.FindClassProperties();
            NavigationProperties = source.Where((PropertyInfo p) => p.CanWrite && p.GetCustomAttributes<JoinAttributeBase>().Any()).ToArray();
            InsertProperties = source.Where((PropertyInfo p) => p.CanWrite && !p.GetCustomAttributes<IgnoreDuringInsertAttribute>().Any() && !p.GetCustomAttributes<KeyAttribute>().Any()).ToArray();
            SearchableProperties = (from p in source
                                    where !p.GetCustomAttributes<NotMappedAttribute>().Any()
                                    select new SqlPropertyMetadata(p)).ToArray();
            KeyProperties = (from p in source
                             where p.GetCustomAttributes<KeyAttribute>().Any() || p.Name.ToLower() == "id"
                             select new SqlPropertyMetadata(p)).ToArray();
            IdType = source.First((PropertyInfo p) => p.Name.ToLower() == "id").PropertyType;
            LastUpdatedTimeTrackingProperties = source.Where((PropertyInfo p) => p.CanWrite && p.GetCustomAttribute<TrackLastUpdatedTimeAttribute>() != null).ToArray();
        }
    }
}
