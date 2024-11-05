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
    public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
    {
        private readonly IMapper _mapper;

        public AuctionCreatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            Console.WriteLine("---> Consuming message 'Create Auction' from Auction Service" + context.Message.Id);

            // Create item from the message
            var item = _mapper.Map<Item>(context.Message);

            if (item.Model == "Foo")
            {
                throw new ArgumentException("Cannot sell cars with name of Foo");
            }

            // Saves the Item object to a mongo database asynchronously.
            // Do not check if the item was saved successfully for now.
            await item.SaveAsync();
        }
    }
}