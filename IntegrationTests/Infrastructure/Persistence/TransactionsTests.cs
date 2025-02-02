using Application.Exceptions;
using Infrastructure.Persistance.Core;
using IntegrationTests.Common.Fixtures.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Infrastructure.Persistence
{
    public class TransactionsTests : IClassFixture<TransactionsFixture>
    {
        private readonly TransactionsFixture fixture;
        private readonly ITransaction transaction;
        private readonly IDocumentRepository<object> repository;


        public TransactionsTests(TransactionsFixture fixture)
        {
            this.fixture = fixture;

            var scope = fixture.Application.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            this.transaction = fixture.GetTransaction(scope);
            this.repository = fixture.GetRepository(scope);
        }

        [Fact]
        public async Task Transact_Should_CommitChanges()
        {
            var objToAdd = new { _key = "test1", test = "test" };

            var added = await transaction.Transact(async () =>
            {
                await TransactionAction(repository, objToAdd);
                return objToAdd;
            }, exclusiveCollections: TransactionsFixture.TEST_COLLECTIONS);

            dynamic fromDb1 = await repository.Get(objToAdd._key, TransactionsFixture.TEST_COLLECTION_1);
            dynamic fromDb2 = await repository.Get(objToAdd._key, TransactionsFixture.TEST_COLLECTION_2);
            Assert.NotNull(fromDb1);
            Assert.NotNull(fromDb2);
            Assert.Equal(objToAdd._key, added._key);
            Assert.Equal(objToAdd.test, added.test);
            Assert.Equal(objToAdd._key, fromDb1._key.ToString());
            Assert.Equal(objToAdd.test, fromDb1.test.ToString());
            Assert.Equal(objToAdd._key, fromDb2._key.ToString());
            Assert.Equal(objToAdd.test, fromDb2.test.ToString());
        }

        [Fact]
        public async Task Transact_Should_AbortChanges()
        {
            var objToAdd = new { _key = "test2", test = "test" };

            Func<Task> act = async () => await transaction.Transact(async () =>
            {
                await TransactionAction(repository, objToAdd);
                throw new Exception("some error");
                return objToAdd;
            }, exclusiveCollections: TransactionsFixture.TEST_COLLECTIONS);

            await Assert.ThrowsAsync<Exception>(act);
            Func<Task> assertDb1 = async () =>
            {
                await repository.Get(objToAdd._key, TransactionsFixture.TEST_COLLECTION_1);
            };
            Func<Task> assertDb2 = async () =>
            {
                await repository.Get(objToAdd._key, TransactionsFixture.TEST_COLLECTION_2);
            };
            await Assert.ThrowsAsync<ResourceNotFoundException>(assertDb1);
            await Assert.ThrowsAsync<ResourceNotFoundException>(assertDb2);
        }

        private async Task TransactionAction(IDocumentRepository<object> repository, object document)
        {
            await repository.Add(document, TransactionsFixture.TEST_COLLECTION_1);
            await repository.Add(document, TransactionsFixture.TEST_COLLECTION_2);
        }
    }
}