using ArangoDBNetStandard.TransactionApi.Models;
using Infrastructure.Persistance.Core;
using Infrastructure.Persistance.Core.Arango;
using NSubstitute;

namespace UnitTests.Persistance.Core
{
    public class ArangoDbTransactionTests
    {
        private readonly IArangoDbClientContext clientContext;

        private const string TRANSACTION_ID = "123";


        public ArangoDbTransactionTests()
        {
            clientContext = Substitute.For<IArangoDbClientContext>();
            SetupClientContext(TRANSACTION_ID);
        }

        [Fact]
        public async Task Transact_Should_Commit()
        {
            var exclusive = new string[] { GlobalCollections.EVENT_RESPONDERS };
            var transaction = new ArangoDbTransaction(clientContext);

            await transaction.Transact(
                async () => await Task.CompletedTask,
                exclusiveCollections: exclusive);

            await clientContext.Received()
                .Client.Transaction.BeginTransaction(
                    Arg.Is<StreamTransactionBody>(x => exclusive.SequenceEqual(x.Collections.Exclusive)));
            await clientContext.Received()
                .Client.Transaction.CommitTransaction(
                    Arg.Is<string>(id => id == TRANSACTION_ID));
            Assert.True(string.IsNullOrEmpty(transaction.GetTransactionId()));
        }

        [Fact]
        public async Task Transact_Should_Abort()
        {
            var exclusive = new string[] { GlobalCollections.EVENT_RESPONDERS };
            var transaction = new ArangoDbTransaction(clientContext);

            var act = () => 
                transaction.Transact(
                    async () => await Task.Run(() => throw new Exception()),
                    exclusiveCollections: exclusive);

            await Assert.ThrowsAsync<Exception>(act);
            await clientContext.Received()
                .Client.Transaction.BeginTransaction(
                    Arg.Is<StreamTransactionBody>(x => exclusive.SequenceEqual(x.Collections.Exclusive)));
            await clientContext.Received()
                .Client.Transaction.AbortTransaction(
                    Arg.Is<string>(id => id == TRANSACTION_ID));
            Assert.True(string.IsNullOrEmpty(transaction.GetTransactionId()));
        }

        [Fact]
        public async Task Transact_Should_HaveCorrectId()
        {
            var exclusive = new string[] { GlobalCollections.EVENT_RESPONDERS };
            var transaction = new ArangoDbTransaction(clientContext);

            string? idBefore = transaction.GetTransactionId();
            string? idDuring = null;
            await transaction.Transact(
                    async () => {
                        idDuring = transaction.GetTransactionId();
                        await Task.CompletedTask;
                    },
                    exclusiveCollections: exclusive);

            Assert.Null(idBefore);
            Assert.Equal(TRANSACTION_ID, idDuring);
            Assert.Null(transaction.GetTransactionId());
        }

        [Fact]
        public async Task Transact_Should_ReturnUpdated()
        {
            var transaction = new ArangoDbTransaction(clientContext);
            dynamic objToUpdate = new { test = "000" };

            var updated = await transaction.Transact(
                    async () => {
                        objToUpdate = new { test = "111" };
                        await Task.CompletedTask;
                        return objToUpdate;
                    });

            Assert.Equal("111", updated.test);
        }

        private void SetupClientContext(string transactionId)
        {
            var response = new TaskCompletionSource<StreamTransactionResponse>();
            response.SetResult(new StreamTransactionResponse() 
            { 
                Result = new StreamTransactionResult()
                {
                    Id = transactionId
                }
            });

            clientContext.Client.Transaction.BeginTransaction(Arg.Any<StreamTransactionBody>())
                .Returns(response.Task);
        }
    }
}
