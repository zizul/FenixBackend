using ArangoDBNetStandard;
using ArangoDBNetStandard.TransactionApi.Models;

namespace Infrastructure.Persistance.Core.Arango
{
    internal class ArangoDbTransaction : ITransaction
    {
        private readonly IArangoDBClient client;

        private string? transactionId;


        public ArangoDbTransaction(IArangoDbClientContext clientContext)
        {
            client = clientContext.Client;
        }

        public async Task Transact(
            Func<Task> action,
            string[]? readCollections = null,
            string[]? writeCollections = null,
            string[]? exclusiveCollections = null)
        {
            await Transact(async () =>
            {
                await action();
                return transactionId;
            }, readCollections, writeCollections, exclusiveCollections);
        }

        public async Task<T> Transact<T>(
            Func<Task<T>> update,
            string[]? readCollections = null,
            string[]? writeCollections = null,
            string[]? exclusiveCollections = null)
        {
            try
            {
                await BeginTransaction(readCollections, writeCollections, exclusiveCollections);

                var updated = await update();

                await CommitTransaction();

                return updated;
            }
            catch
            {
                await AbortTransaction();
                throw;
            }
        }

        public string? GetTransactionId()
        {
            return transactionId;
        }

        private async Task<string> BeginTransaction(
            string[]? readCollections, string[]? writeCollections, string[]? exclusiveCollections)
        {
            readCollections ??= new string[0];
            writeCollections ??= new string[0];
            exclusiveCollections ??= new string[0];

            var response = await client.Transaction.BeginTransaction(new StreamTransactionBody()
            {
                // Allow reading from undeclared collections
                AllowImplicit = true,

                Collections = new PostTransactionRequestCollections()
                {
                    Read = readCollections,
                    Write = writeCollections,
                    Exclusive = exclusiveCollections,
                }
            });

            transactionId = response.Result.Id;
            return response.Result.Id;
        }

        private async Task AbortTransaction()
        {
            var response = await client.Transaction.AbortTransaction(transactionId);
            transactionId = null;
        }

        private async Task CommitTransaction()
        {
            var response = await client.Transaction.CommitTransaction(transactionId);
            transactionId = null;
        }
    }
}
