using Application.Exceptions;
using Application.Services.Readiness.Contracts;
using Domain.Entities.Readiness;
using Domain.Entities.User;
using Infrastructure.Persistance.Core;
using Infrastructure.Persistance.Repositories.Readiness.Serialization;
using Newtonsoft.Json;

namespace Infrastructure.Persistance.Repositories.Readiness
{
    internal class ReadinessRepository : IReadinessRepository
    {
        private readonly IDocumentRepository<UserReadiness> readinessRepository;
        private readonly IDocumentRepository<object> reporterRepository;


        public ReadinessRepository(
            IDocumentRepository<UserReadiness> readinessRepository, 
            IDocumentRepository<object> reporterRepository)
        {
            this.readinessRepository = readinessRepository;
            this.reporterRepository = reporterRepository;
        }

        public async Task<UserReadiness> Get(string identityId)
        {
            var vars = new Dictionary<string, object>()
            {
                { "identity_id", identityId }
            };

            var query =
                $"FOR u in {GlobalCollections.USERS} " +
                $"FILTER u.identity_id == @identity_id " +
                $"FOR r in {GlobalCollections.USER_READINESS} " +
                $"FILTER u._key == r.{UserReadinessConverter.USER_REF} " +
                $"RETURN r";

            var result = await readinessRepository.Execute(query, vars);
            if (!result.Any())
            {
                throw new ResourceNotFoundException($"User with identity_id: {identityId} or his readiness does not exist"); 
            }

            return result.Single();
        }

        public async Task<UserReadiness> Update(string identityId, UserReadiness userReadiness)
        {
            string keyRef = await GetUserProp(
                "doc.identity_id == @value", "_key", identityId);
            userReadiness.UserId = keyRef;

            var vars = new Dictionary<string, object>()
            {
                { "user_ref", keyRef }
            };
            var userReadinessJson = JsonConvert.SerializeObject(userReadiness);

            var query =
                $"UPSERT {{ {UserReadinessConverter.USER_REF}: @user_ref }} " +
                    $"INSERT {userReadinessJson} " +
                    $"UPDATE {userReadinessJson} " +
                    $"IN {GlobalCollections.USER_READINESS}";

            await readinessRepository.Execute(query, vars);
            return userReadiness;
        }

        private async Task<string> GetUserProp(string filter, string prop, string value)
        {
            var query =
                $"FOR doc IN {GlobalCollections.USERS} " +
                $"FILTER {filter} " +
                $"RETURN doc.{prop}";
            var vars = new Dictionary<string, object>()
            {
                { "value", value }
            };

            var result = await reporterRepository.Execute(query, vars);
            if (!result.Any())
            {
                throw new ResourceNotFoundException($"User with identity_id: {value} does not exist");
            }

            return (string)result.Single();
        }
    }
}
