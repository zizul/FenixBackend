namespace Infrastructure.Persistance.Core
{
    internal interface ITransaction
    {
        public Task Transact(
            Func<Task> action,
            string[]? readCollections = null,
            string[]? writeCollections = null,
            string[]? exclusiveCollections = null);
        public Task<T> Transact<T>(
            Func<Task<T>> update,
            string[]? readCollections = null,
            string[]? writeCollections = null,
            string[]? exclusiveCollections = null);
        public string? GetTransactionId();
    }
}
