using Application.Exceptions;
using Application.Services.User.Contracts;
using Domain.Entities.Readiness;
using Domain.Entities.User;
using Infrastructure.Persistance.Core;
using Newtonsoft.Json;

namespace Infrastructure.Persistance.Repositories.User
{
    internal class UserRepository : IUserRepository
    {
        private readonly IDocumentRepository<BasicUser> userRepository;
        private readonly IDocumentRepository<Device> devicesRepository;
        private readonly IDocumentRepository<UserReadiness> readinessRepository;
        private readonly ITransaction transaction;


        public UserRepository(
            IDocumentRepository<BasicUser> userRepository,
            IDocumentRepository<Device> devicesRepository,
            IDocumentRepository<UserReadiness> readinessRepository,
            ITransaction transaction)
        {
            this.userRepository = userRepository;
            this.devicesRepository = devicesRepository;
            this.readinessRepository = readinessRepository;
            this.transaction = transaction;
        }

        public async Task<BasicUser> Add(BasicUser user)
        {
            await ValidateUserUniqueness(null, user);

            return await userRepository.Add(user, GlobalCollections.USERS);
        }

        public async Task Delete(string id)
        {
            var write = new string[] 
            { 
                GlobalCollections.USERS, 
                GlobalCollections.USER_DEVICES,
                GlobalCollections.USER_READINESS,
            };

            await transaction.Transact(async () =>
            {
                var removedUser = await DeleteUser(id);
                await DeleteRelatedDevices(removedUser.Id);
                await DeleteRelatedReadiness(removedUser.Id);
            }, exclusiveCollections: write);
        }

        private async Task<BasicUser> DeleteUser(string id)
        {
            var bindingParams = new Dictionary<string, object>() {
                { "identity_id", id }
            };

            var removedUsers = await userRepository.Execute(
                $"FOR u IN {GlobalCollections.USERS} " +
                $"FILTER u.identity_id == @identity_id " +
                $"REMOVE u IN {GlobalCollections.USERS} " +
                $"RETURN OLD",
                bindingParams);

            if (!removedUsers.Any())
            {
                throw ResourceNotFoundException.WithId<BasicUser>(id);
            }

            return removedUsers.Single();
        }

        private async Task DeleteRelatedDevices(string userRef)
        {
            var bindingParams = new Dictionary<string, object>() 
            {
                { "user_ref", userRef }
            };

            await devicesRepository.Execute(
                $"FOR d IN {GlobalCollections.USER_DEVICES} " +
                $"FILTER d.user_ref == @user_ref " +
                $"REMOVE d IN {GlobalCollections.USER_DEVICES} ",
                bindingParams);
        }

        private async Task DeleteRelatedReadiness(string userRef)
        {
            var bindingParams = new Dictionary<string, object>() 
            {
                { "user_ref", userRef }
            };

            await readinessRepository.Execute(
                $"FOR ur IN {GlobalCollections.USER_READINESS} " +
                $"FILTER ur.user_ref == @user_ref " +
                $"REMOVE ur IN {GlobalCollections.USER_READINESS} ",
                bindingParams);
        }

        public async Task<BasicUser> Update(string id, BasicUser user)
        {
            var write = new string[] { GlobalCollections.USERS };

            var updated = await transaction.Transact(async () =>
            {
                return await UpdateInDb(id, user);
            }, exclusiveCollections: write);

            return updated;
        }

        private async Task<BasicUser> UpdateInDb(string id, BasicUser user)
        {
            await ValidateUserUniqueness(id, user);

            var bindingParams = new Dictionary<string, object>() {
                { "identity_id", id }
            };

            var json = JsonConvert.SerializeObject(user);
            var updated = await userRepository.Execute(
                $"FOR u IN {GlobalCollections.USERS} " +
                $"FILTER u.identity_id == @identity_id " +
                $"UPDATE u WITH {json} IN {GlobalCollections.USERS} " +
                $"RETURN OLD",
                bindingParams);

            if (!updated.Any())
            {
                throw ResourceNotFoundException.WithId<BasicUser>(id);
            }

            return user;
        }

        private async Task ValidateUserUniqueness(string? identityId, BasicUser newData)
        {
            await ThrowExceptionIfNotUnique(identityId, "identity_id", newData.IdentityId);
            await ThrowExceptionIfNotUnique(identityId, "username", newData.Username);
            await ThrowExceptionIfNotUnique(identityId, "email", newData.Email);
            await ThrowExceptionIfNotUnique(identityId, "mobile_number", newData.MobileNumber);
        }

        private async Task ThrowExceptionIfNotUnique(string? identityId, string propertyName, string value)
        {
            var bindingParams = new Dictionary<string, object>()
            {
                { "value", value }
            };

            string excludeUserFilter = string.Empty;
            if (identityId != null)
            {
                bindingParams.Add("identity_id", identityId);
                excludeUserFilter = "FILTER u.identity_id != @identity_id ";
            }

            var found = await userRepository.Execute(
                $"FOR u IN {GlobalCollections.USERS} " +
                $"{excludeUserFilter}" +
                $"FILTER u.{propertyName} == @value " +
                $"RETURN u",
                bindingParams);
            if (found.Any())
            {
                throw new ResourceConflictException($"User with {propertyName}: {value} is already exists");
            }
        }
    }
}
