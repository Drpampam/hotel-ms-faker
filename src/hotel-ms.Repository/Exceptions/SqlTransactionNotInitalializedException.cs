namespace hotelier_core_app.Domain.Exceptions
{
    public class SqlTransactionNotInitializedException : Exception
    {
        public SqlTransactionNotInitializedException()
        {
        }

        public SqlTransactionNotInitializedException(string message)
            : base(message)
        {
        }
    }
}
