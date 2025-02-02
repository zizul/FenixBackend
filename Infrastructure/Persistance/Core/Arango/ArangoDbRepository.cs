using Application.Exceptions;
using ArangoDBNetStandard;
using ArangoDBNetStandard.CollectionApi.Models;
using ArangoDBNetStandard.DocumentApi.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace Infrastructure.Persistance.Core.Arango
{
    internal class ArangoDbRepository<T> : IDocumentRepository<T>
        where T : class
    {
        private readonly IArangoDBClient client;

        private ITransaction transaction;


        public ArangoDbRepository(IArangoDbClientContext context, ITransaction transaction)
        {
            this.client = context.Client;
            this.transaction = transaction;
        }

        public async Task<T> Add(T entity, string collectionName)
        {
            await CreateCollectionIfNotExists(collectionName);

            try
            {
                var response = await client.Document.PostDocumentAsync(
                    collectionName,
                    entity,
                    new PostDocumentsQuery() { ReturnNew = true },
                    headers: GetDocumentHeader());
                return response.New;
            }
            catch (ApiErrorException e)
            {
                throw GetDbException(e);
            }
        }

        public async Task<List<T>> AddMany(List<T> entities, string collectionName)
        {
            await CreateCollectionIfNotExists(collectionName);

            var response = await client.Document.PostDocumentsAsync(
                collectionName,
                entities,
                new PostDocumentsQuery() { ReturnNew = true },
                headers: GetDocumentHeader());

            return response.Select(x => x.New).ToList();
        }

        public async Task Delete(string key, string collectionName)
        {
            try
            {
                await client.Document.DeleteDocumentAsync(
                    collectionName,
                    key,
                    headers: GetDocumentHeader());
            }
            catch (ApiErrorException e)
            {
                throw GetDbException(e, key);
            }
        }

        public async Task<T> Get(string key, string collectionName)
        {
            try
            {
                return await client.Document.GetDocumentAsync<T>(
                    collectionName,
                    key,
                    headers: GetDocumentHeader());
            }
            catch (ApiErrorException e)
            {
                throw GetDbException(e, key);
            }
        }

        public async Task<IEnumerable<T>> Execute(string aqlQuery, Dictionary<string, object>? bindVars = null)
        {
            try
            {

                return (await client.Cursor.PostCursorAsync<T>(
                    aqlQuery,
                    bindVars,
                    transactionId: transaction.GetTransactionId())).Result;
            }
            catch (ApiErrorException e)
            {
                throw GetDbException(e);
            }
        }

        public async Task<T> Update(string key, string collectionName, T entity)
        {
            try
            {
                var response = await client.Document.PatchDocumentAsync<T, T>(
                    collectionName,
                    key,
                    entity,
                    new PatchDocumentQuery() { ReturnNew = true },
                    headers: GetDocumentHeader());
                return response.New;
            }
            catch (ApiErrorException e)
            {
                throw GetDbException(e, key);
            }
        }

        private DocumentHeaderProperties? GetDocumentHeader()
        {
            var transactionId = transaction.GetTransactionId();

            if (string.IsNullOrEmpty(transactionId))
            {
                return null;
            }

            return new DocumentHeaderProperties()
            {
                TransactionId = transactionId,
            };
        }

        private async Task CreateCollectionIfNotExists(string collectionName)
        {
            if (!await IsExists(collectionName))
            {
                await client.Collection.PostCollectionAsync(
                    new PostCollectionBody
                    {
                        Name = collectionName
                    });
            }
        }

        private async Task<bool> IsExists(string collectionName)
        {
            try
            {
                var collection = await client.Collection.GetCollectionAsync(collectionName);
                return true;
            }
            catch (ApiErrorException e)
            {
                return false;
            }
        }

        private Exception GetDbException(ApiErrorException e, string? key = null)
        {
            Exception resultException = e;

            if (e.ApiError.Code == System.Net.HttpStatusCode.NotFound)
            {
                resultException = ResourceNotFoundException.WithId<T>(key);
            }
            else if (e.ApiError.Code == System.Net.HttpStatusCode.Conflict)
            {
                resultException = new ResourceConflictException(e.Message);
            }

            return resultException;
        }
    }
}
