namespace hotelier_core_app.Domain.SqlGenerator.QueryExpressions
{
    internal class QueryBinaryExpression : QueryExpression
    {
        public List<QueryExpression> Nodes { get; set; }

        public QueryBinaryExpression()
        {
            base.NodeType = QueryExpressionType.Binary;
        }

        public override string ToString()
        {
            return $"[{base.ToString()} ({string.Join(",", Nodes)})]";
        }
    }
}
