using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _dbContext;
        private readonly IMapper _mapper;
        
        // This is the MassTransit publish endpoint that we use to publish a message onto the message bus
        // when an auction is created. This is used by the SearchService to create an index of all items
        // available for auction and to keep this index up to date.
        private readonly IPublishEndpoint _publishEndpoint;

        /// <summary>
        /// Creates an instance of the AuctionsController.
        /// </summary>
        /// <param name="dbContext">The database context to use.</param>
        /// <param name="mapper">The AutoMapper to use.</param>
        /// <param name="publishEndpoint">The MassTransit publish endpoint to use.</param>
        public AuctionsController(AuctionDbContext dbContext, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;

        }

        /// <summary>
        /// Gets all auctions in the database that have been updated after the specified date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions(string date)
        {
            var query = _dbContext.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

            if (!string.IsNullOrEmpty(date))
            {
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }
            
            return await query.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        /// <summary>
        /// Gets an auction by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDTO>> GetAuctionById(Guid id)
        {
            var auction = await _dbContext.Auctions
                .Include(auction => auction.Item)
                .FirstOrDefaultAsync(auction => auction.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            if (auction.Item == null)
            {
                // An auction should always have an associated item, but we can't guarantee this
                // because the database isn't enforcing it.
                // If this happens, we should investigate why.
                throw new InvalidOperationException($"Auction {id} has no associated item.");
            }

            return _mapper.Map<AuctionDTO>(auction);
        }


        [HttpPost]
        public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDTO auctionDTO)
        {
            var auction = _mapper.Map<Auction>(auctionDTO);

            // Todo: Add current user as seller.

            auction.Seller = "testSeller";
            _dbContext.Auctions.Add(auction);
            var result = await _dbContext.SaveChangesAsync() > 0;
            var newAuctionDto = _mapper.Map<AuctionDTO>(auction);

            // Assume the service bus is running and the SearchService is running also...
            // Publishing the created auction in the service bus.
            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuctionDto));

            if (!result)
            {
                return BadRequest("Failed to create auction");
            }

            // Notice endpoint that the item was created and identify the current item by its id.
            return CreatedAtAction(
                nameof(GetAuctionById),
                new { auction.Id },
                newAuctionDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, [FromBody] UpdateAuctionDTO auctionDTO)
        {
            var auction = await _dbContext.Auctions.Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);
            // Todo: Only that item with seller that is identical to current user.

            if (auction == null)
            {
                return NotFound();
            }

            // Update the data
            auction.Item.Make = auctionDTO.Make ?? auction.Item.Make;
            auction.Item.Model = auctionDTO.Model ?? auction.Item.Model;
            auction.Item.Color = auctionDTO.Color ?? auction.Item.Color;
            auction.Item.Mileage = auctionDTO.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = auctionDTO.Year ?? auction.Item.Year;

            var result = await _dbContext.SaveChangesAsync() > 0;

            if (result)
            {
                return Ok();
            }

            return BadRequest("Failed to update auction.");
        }

    }
}