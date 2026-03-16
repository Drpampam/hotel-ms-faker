namespace hotelier_core_app.Domain.SqlGenerator.QueryExpressions
{
    internal abstract class QueryExpression
    {
        public QueryExpressionType NodeType { get; set; }

        public string LinkingOperator { get; set; }

        public override string ToString()
        {
            return $"[NodeType:{NodeType}, LinkingOperator:{LinkingOperator}]";
        }
    }
}
