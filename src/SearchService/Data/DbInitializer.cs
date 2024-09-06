using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Models
{
    public class DbInitializer
    {
        /// <summary>
        /// Initializes the SearchDb database and fetches all items from the Auction Service
        /// </summary>
        /// <param name="app">The web application</param>
        /// <returns>A task that completes when the initialization has finished</returns>
        public static async Task InitializeDb(WebApplication app)
        {
            // Initialize SearchDb database
            // This is done by calling the InitAsync method on the DB class, which takes the name of the database and the connection string
            await DB.InitAsync("SearchDb", MongoClientSettings
                .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            // Create index of the mongo
            // This is done by calling the Index method on the DB class, which takes the type of the entity to index and the type of the key
            // The key is a lambda expression that specifies the field to index
            await DB.Index<Item>()
                .Key(x => x.Make, KeyType.Text)
                .Key(x => x.Model, KeyType.Text)
                .Key(x => x.Color, KeyType.Text)
                .CreateAsync();

            // Fetch all items from the Auction Service
            // This is done by using the AuctionSearchServiceHttpClient, which is a class that provides the synchronous communication with the auction service directly
            // We have to wait for the response - not very efficient!!!
            using var scope = app.Services.CreateScope();
            var httpClient = scope.ServiceProvider.GetService<AuctionServiceHttpClient>();
            var items = await httpClient.GetItemsForSearchDb();


            if (items.Count > 0)
            {
                // Save the items in the SearchDb database
                await DB.SaveAsync(items);
                Console.WriteLine($"Initialized database for SearchService from the Auction Service via synchronous http client => {items.Count} items obtained from the Auction.");
            }
        }

    }
}