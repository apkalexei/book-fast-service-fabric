using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BookFast.Web.Contracts;
using BookFast.Web.Contracts.Exceptions;
using BookFast.Web.Contracts.Models;
using BookFast.Facility.Client;
using BookFast.ServiceFabric.Communication;

namespace BookFast.Web.Proxy
{
    internal class AccommodationProxy : IAccommodationService
    {
        private readonly IAccommodationMapper mapper;
        private readonly IPartitionClientFactory<CommunicationClient<IBookFastFacilityAPI>> partitionClientFactory;

        public AccommodationProxy(IAccommodationMapper mapper,
            IPartitionClientFactory<CommunicationClient<IBookFastFacilityAPI>> partitionClientFactory)
        {
            this.mapper = mapper;
            this.partitionClientFactory = partitionClientFactory;
        }

        public async Task<List<Accommodation>> ListAsync(Guid facilityId)
        {
            var result = await partitionClientFactory.CreatePartitionClient().InvokeWithRetryAsync(async client =>
            {
                var api = await client.CreateApiClient();
                return await api.ListAccommodationsWithHttpMessagesAsync(facilityId);
            });

            if (result.Response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new FacilityNotFoundException(facilityId);
            }

            return mapper.MapFrom(result.Body);
        }

        public async Task<Accommodation> FindAsync(Guid accommodationId)
        {
            var result = await partitionClientFactory.CreatePartitionClient().InvokeWithRetryAsync(async client =>
            {
                var api = await client.CreateApiClient();
                return await api.FindAccommodationWithHttpMessagesAsync(accommodationId);
            });

            if (result.Response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new AccommodationNotFoundException(accommodationId);
            }

            return mapper.MapFrom(result.Body);
        }

        public async Task CreateAsync(Guid facilityId, AccommodationDetails details)
        {
            var result = await partitionClientFactory.CreatePartitionClient().InvokeWithRetryAsync(async client =>
            {
                var api = await client.CreateApiClient();
                return await api.CreateAccommodationWithHttpMessagesAsync(facilityId, mapper.MapFrom(details));
            });

            if (result.Response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new FacilityNotFoundException(facilityId);
            }
        }

        public async Task UpdateAsync(Guid accommodationId, AccommodationDetails details)
        {
            var result = await partitionClientFactory.CreatePartitionClient().InvokeWithRetryAsync(async client =>
            {
                var api = await client.CreateApiClient();
                return await api.UpdateAccommodationWithHttpMessagesAsync(accommodationId, mapper.MapFrom(details));
            });

            if (result.Response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new AccommodationNotFoundException(accommodationId);
            }
        }

        public async Task DeleteAsync(Guid accommodationId)
        {
            var result = await partitionClientFactory.CreatePartitionClient().InvokeWithRetryAsync(async client =>
            {
                var api = await client.CreateApiClient();
                return await api.DeleteAccommodationWithHttpMessagesAsync(accommodationId);
            });

            if (result.Response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new AccommodationNotFoundException(accommodationId);
            }
        }
    }
}