using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{
    /// <summary>
    /// Provides the synchronous communication with the auction service directly. Not using the RabbitMQ.
    /// </summary>
    public class AuctionServiceHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        
        public AuctionServiceHttpClient(HttpClient httpClient, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gets all items from the auction service that have been updated
        /// since the last updated item in the search database.
        /// </summary>
        /// <returns>A list of items</returns>
        public async Task<List<Item>> GetItemsForSearchDb()
        {
            // Get the date of the last updated auction in the search mongo database.
            var lastUpdatedAuction = await DB.Find<Item, string>()
                .Sort(item => item.Descending(x => x.UpdatedAt))
                .Project(item => item.UpdatedAt.ToString()) // Project the items in Mongo into a string, which is the UpdatedAt date as a string
                .ExecuteFirstAsync(); // Execute the query in Mongo and get the first result as a string

            // Requests all items from the auction service that have been updated
            // since the last updated item in the search database.
            return await _httpClient.GetFromJsonAsync<List<Item>>(_configuration["AuctionServiceUrl"] 
            + "/api/auctions?date=" + lastUpdatedAuction);
			}
    }
} 