using System.Text;

namespace hotelier_core_app.Domain.SqlGenerator
{
    public class SqlQuery
    {
        public StringBuilder SqlBuilder { get; }

        public object Param { get; private set; }

        public SqlQuery()
        {
            SqlBuilder = new StringBuilder();
        }

        public SqlQuery(object param)
            : this()
        {
            Param = param;
        }

        public string GetSql()
        {
            return SqlBuilder.ToString().Trim();
        }

        public void SetParam(object param)
        {
            Param = param;
        }
    }
}
