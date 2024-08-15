using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParameters searchParameters)
        {
            var query = DB.PagedSearch<Item, Item>();

            if (!string.IsNullOrEmpty(searchParameters.SearchTerm))
            {
                query.Match(Search.Full, searchParameters.SearchTerm).SortByTextScore();
            }

            // Ordering
            query = searchParameters.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(a => a.Make)),
                "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
                _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
            };

            // Filtering
            query = searchParameters.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(x => x.AuctionEnd > DateTime.UtcNow.AddHours(1) && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
            };

            // Sellers
            if (!string.IsNullOrEmpty(searchParameters.Seller))
            {
                query.Match(x => x.Seller == searchParameters.Seller);
            }   

            // Winners
            if (!string.IsNullOrEmpty(searchParameters.Winner))
            {
                query.Match(x => x.Winner == searchParameters.Winner);
            }

            // Pagination
            query.PageNumber(searchParameters.PageNumber);
            query.PageSize(searchParameters.PageSize);

            // Execute the query
            var result = await query.ExecuteAsync();

            return Ok(new
            {
                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            });
        }
    }
}