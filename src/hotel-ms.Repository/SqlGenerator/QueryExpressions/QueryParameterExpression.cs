namespace hotelier_core_app.Domain.SqlGenerator.QueryExpressions
{
    internal class QueryParameterExpression : QueryExpression
    {
        public string PropertyName { get; set; }

        public object PropertyValue { get; set; }

        public string QueryOperator { get; set; }

        public bool NestedProperty { get; set; }

        public QueryParameterExpression()
        {
            base.NodeType = QueryExpressionType.Parameter;
        }

        internal QueryParameterExpression(string linkingOperator, string propertyName, object propertyValue, string queryOperator, bool nestedProperty)
            : this()
        {
            base.LinkingOperator = linkingOperator;
            PropertyName = propertyName;
            PropertyValue = propertyValue;
            QueryOperator = queryOperator;
            NestedProperty = nestedProperty;
        }

        public override string ToString()
        {
            return $"[{base.ToString()}, PropertyName:{PropertyName}, PropertyValue:{PropertyValue}, QueryOperator:{QueryOperator}, NestedProperty:{NestedProperty}]";
        }
    }
}
