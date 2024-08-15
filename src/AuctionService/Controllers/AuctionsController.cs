using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
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

        public AuctionsController(AuctionDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions()
        {
            var auctions = await _dbContext.Auctions
                .Include(x => x.Item)
                .OrderBy(x => x.Item.Make)
                .ToListAsync();

            return _mapper.Map<List<AuctionDTO>>(auctions);
        }

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

            if (!result)
            {
                return BadRequest("Failed to create auction");
            }

            // Notice endpoint that the item was created and identify the current item by its id.
            return CreatedAtAction(
                nameof(GetAuctionById),
                new { auction.Id },
                _mapper.Map<AuctionDTO>(auction));
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