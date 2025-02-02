using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Persistance.Core;
using IntegrationTests.Common.Fixtures.Persistence;
using Application.Exceptions;
using Domain.Entities.Map;

namespace IntegrationTests.Infrastructure.Persistence
{
    public class DocumentRepositoryTests : IClassFixture<DocumentRepositoryFixture>
    {
        private readonly DocumentRepositoryFixture fixture;
        private readonly IDocumentRepository<object> repository;


        public DocumentRepositoryTests(DocumentRepositoryFixture fixture)
        {
            this.fixture = fixture;

            var scope = fixture.Application.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            this.repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository<object>>();
        }

        [Fact]
        public async Task Add_Should_BeCorrect()
        {
            var objToAdd = new { test1 = "test1", test2 = 15.525 };

            dynamic added = await repository.Add(objToAdd, DocumentRepositoryFixture.TEST_COLLECTION_1);

            await AssertMockObject(objToAdd, added);
        }

        [Fact]
        public async Task AddMany_Should_BeCorrect()
        {
            var objsToAdd = new List<object>() {
                new { test1 = "test1", test2 = 15.525 },
                new { test1 = "test2", test2 = 17.525 },
                new { test1 = "test3", test2 = -17.525 }
            }; 

            var addedList = await repository.AddMany(objsToAdd, DocumentRepositoryFixture.TEST_COLLECTION_1);

            Assert.Equal(3, addedList.Count);
            for (int i = 0; i < objsToAdd.Count; i++)
            {
                dynamic objToAdd = objsToAdd[i];
                dynamic added = addedList[i];
                await AssertMockObject(objToAdd, added);
            }
        }

        private async Task AssertMockObject(dynamic expected, dynamic actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.test1, actual.test1.ToString());
            Assert.Equal(expected.test2, double.Parse(actual.test2.ToString()));
            dynamic fromDb = await repository.Get(actual._key.ToString(), TransactionsFixture.TEST_COLLECTION_1);
            Assert.NotNull(fromDb);
            Assert.Equal(expected.test1, fromDb.test1.ToString());
            Assert.Equal(expected.test2, double.Parse(fromDb.test2.ToString()));
        }

        [Fact]
        public async Task Get_Should_BeCorrect()
        {
            var keyToFind = fixture.DocumentKeysToFind.First();

            dynamic result = await repository.Get(keyToFind, DocumentRepositoryFixture.TEST_COLLECTION_2);

            Assert.NotNull(result);
            Assert.Equal(keyToFind, result._key.ToString());
        }

        [Fact]
        public async Task Update_Should_BeCorrect()
        {
            var keyToUpdate = fixture.DocumentKeyToUpdate;
            var fieldsToUpdate = new { test2 = 15.525 };

            dynamic updated = await repository.Update(keyToUpdate, DocumentRepositoryFixture.TEST_COLLECTION_1, fieldsToUpdate);

            Assert.NotNull(updated);
            Assert.Equal(fieldsToUpdate.test2, double.Parse(updated.test2.ToString()));
            dynamic fromDb = await repository.Get(keyToUpdate, TransactionsFixture.TEST_COLLECTION_1);
            Assert.NotNull(fromDb);
            Assert.Equal(fieldsToUpdate.test2, double.Parse(fromDb.test2.ToString()));
        }

        [Fact]
        public async Task Update_Should_ThrowResourceNotFound()
        {
            var keyToUpdate = "-1";
            var fieldsToUpdate = new { test2 = 15.525 };

            Func<Task> act = async () => await repository.Update(keyToUpdate, DocumentRepositoryFixture.TEST_COLLECTION_1, fieldsToUpdate);

            await Assert.ThrowsAsync<ResourceNotFoundException>(act);
        }

        [Fact]
        public async Task Delete_Should_BeCorrect()
        {
            var keyToDelete = fixture.DocumentKeyToDelete;

            await repository.Delete(keyToDelete, DocumentRepositoryFixture.TEST_COLLECTION_1);

            Func<Task> assert = async () =>
            {
                await repository.Get(keyToDelete, TransactionsFixture.TEST_COLLECTION_1);
            };
            await Assert.ThrowsAsync<ResourceNotFoundException>(assert);
        }

        [Fact]
        public async Task Delete_Should_ThrowResourceNotFound()
        {
            var keyToDelete = "-1";

            Func<Task> act = async () => await repository.Delete(keyToDelete, DocumentRepositoryFixture.TEST_COLLECTION_1);

            await Assert.ThrowsAsync<ResourceNotFoundException>(act);
        }

        [Fact]
        public async Task Execute_Should_BeCorrect()
        {
            string query = $"FOR d in {TransactionsFixture.TEST_COLLECTION_2} " +
                            "FILTER d.test_id == @id1 || d.test_id == @id2 " +
                            "RETURN d";
            var bindingVars = new Dictionary<string, object>()
            {
                { "id1", "0" },
                { "id2", "1" }
            };

            var result = await repository.Execute(query, bindingVars);

            Assert.Equal(2, result.Count());
            for (int i = 0; i < result.Count(); i++)
            {
                dynamic fetched = result.ElementAt(i);
                string expectedId = i.ToString();

                Assert.NotNull(fetched);
                Assert.Equal(expectedId, fetched.test_id.ToString());
            }
        }
    }
}