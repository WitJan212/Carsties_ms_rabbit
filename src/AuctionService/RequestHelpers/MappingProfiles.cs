using AuctionService.DTOs;
using AuctionService.Entities;

namespace AuctionService.RequestHelpers
{
    public class MappingProfiles : AutoMapper.Profile
    {
        public MappingProfiles()
        {
            CreateMap<Auction, AuctionDTO>()
                .IncludeMembers(x => x.Item);  // Auction.Item are included

            CreateMap<Item, AuctionDTO>();

            CreateMap<CreateAuctionDTO, Auction>()
                .ForMember(d => d.Item, o => o.MapFrom(s => s));

            CreateMap<CreateAuctionDTO, Item>();

            // Maps the CreateAuctionDTO to the Auction and Item entities for MassTransit exchange.
            // AuctionCreated class is known for all the services.
            CreateMap<AuctionDTO, Contracts.AuctionCreated>();
        }
    }
}