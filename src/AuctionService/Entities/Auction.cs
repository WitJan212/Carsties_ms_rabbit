using System.Collections.Immutable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Entities
{
    // [Table("Auctions")]
    public class Auction
    {
        public Guid Id { get; set; }

        public int ReservedPrice { get; set; } = 0;

        public string Seller { get; set; }

        public string Winner { get; set; }

        public int? SoldAmount { get; set; }

        public int CurrentHighBid { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime AuctionEnd { get; set; }

        public Status Status { get; set; }

        public Item Item { get; set; }
    }
}