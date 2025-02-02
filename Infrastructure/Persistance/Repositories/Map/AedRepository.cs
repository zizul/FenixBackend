using Application.Exceptions;
using Application.Services.Map.PointsOfInterest.Contracts;
using Domain.Entities.Map;
using Infrastructure.Persistance.Core;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Persistance.Repositories.Map
{
    internal class AedRepository : IAedRepository
    {
        private readonly IDocumentRepository<Aed> aedsRepository;
        private readonly IObjectStorage objectStorage;
        private readonly ITransaction transaction;


        public AedRepository(
            IDocumentRepository<Aed> aedsRepository, 
            IObjectStorage objectStorage,
            ITransaction transaction)
        {
            this.aedsRepository = aedsRepository;
            this.objectStorage = objectStorage;
            this.transaction = transaction;
        }

        public async Task<Aed> Add(Aed aed)
        {
            return await aedsRepository.Add(aed, GlobalCollections.AEDS);
        }

        public async Task<Aed> Update(string id, Aed aed)
        {
            return await aedsRepository.Update(id, GlobalCollections.AEDS, aed);
        }

        public async Task Delete(string id)
        {
            await aedsRepository.Delete(id, GlobalCollections.AEDS);
        }

        public async Task<string> UpdatePhoto(string id, IFormFile photo)
        {
            var write = new string[]
            {
                GlobalCollections.AEDS,
            };

            var url = await transaction.Transact(async () =>
            {
                var aed = await aedsRepository.Get(id, GlobalCollections.AEDS);

                await DeleteOldPhotoIfExists(aed);
                var url = await UploadAndUpdatePhoto(aed, photo);

                return url;
            }, exclusiveCollections: write);

            return url;
        }

        private async Task DeleteOldPhotoIfExists(Aed aed)
        {
            if (string.IsNullOrEmpty(aed.PhotoUrl))
            {
                return;
            }

            await objectStorage.Delete(aed.PhotoUrl);
        }

        private async Task<string> UploadAndUpdatePhoto(Aed aed, IFormFile photo)
        {
            var url = await objectStorage.Put(photo, ObjectBuckets.AEDS_PHOTOS);

            aed.PhotoUrl = url;
            await aedsRepository.Update(aed.Id.ToString(), GlobalCollections.AEDS, aed);

            return url;
        }

        public async Task DeletePhoto(string id)
        {
            var write = new string[]
            {
                GlobalCollections.AEDS,
            };

            await transaction.Transact(async () =>
            {
                var aed = await aedsRepository.Get(id, GlobalCollections.AEDS);

                await DeletePhotoOrThrowExceptionIfNotExists(aed);

                aed.PhotoUrl = null;
                await aedsRepository.Update(id, GlobalCollections.AEDS, aed);
            }, exclusiveCollections: write);
        }

        private async Task DeletePhotoOrThrowExceptionIfNotExists(Aed aed)
        {
            if (string.IsNullOrEmpty(aed.PhotoUrl))
            {
                throw ResourceNotFoundException.WithId<Aed>($"{aed.Id}:photoUrl");
            }

            await objectStorage.Delete(aed.PhotoUrl);
        }
    }
}
