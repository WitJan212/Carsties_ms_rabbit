using MongoDB.Entities;

namespace SearchService.Models
{
    /// <summary>
    /// This class definition defines a data model for an Item entity, which inherits from the Entity class provided by the MongoDB.Entities library.
    /// </summary>
    public class Item : Entity
    {
        public int ReservedPrice { get; set; }

        public string Seller { get; set; }
        
        public string Winner { get; set; }

        public int SoldAmount { get; set; }
        
        public int CurrentHighBid { get; set; }
        
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime AuctionEnd { get; set; }

        public string Status { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }

        public int Year { get; set; }

        public string Color { get; set; }

        public int Mileage { get; set; }

        public string ImageUrl { get; set; }
    }
}