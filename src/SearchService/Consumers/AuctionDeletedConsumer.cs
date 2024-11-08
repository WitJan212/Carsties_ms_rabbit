using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
    public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
    {
        public async Task Consume(ConsumeContext<AuctionDeleted> context)
        {
            Console.WriteLine("---> [Search service] - Consuming message 'Delete Auction' from Auction Service" + context.Message.Id);

            var result = await DB.DeleteAsync<Item>(context.Message.Id);

            if (!result.IsAcknowledged)
            {
                throw new MessageException(typeof(AuctionDeleted), $"--> {context.Message.Id} - Item not found in SearchService database. Cannot delete from mongo DB");
            }
        }
    }
}