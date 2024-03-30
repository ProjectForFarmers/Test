﻿using AutoMapper;
using FarmersMarketplace.Application.Exceptions;
using FarmersMarketplace.Application.Helpers;
using FarmersMarketplace.Application.Interfaces;
using FarmersMarketplace.Domain;
using FarmersMarketplace.Domain.Accounts;
using FarmersMarketplace.Domain.Payment;
using FarmersMarketplace.Elasticsearch.Documents;
using Microsoft.EntityFrameworkCore;
using Nest;
using ApplicationException = FarmersMarketplace.Application.Exceptions.ApplicationException;

namespace FarmersMarketplace.Elasticsearch.Factories
{
    public class ProductIndexFactory : IIndexFactory
    {
        public void CreateIndex(IElasticClient client)
        {
            string indexName = Indecies.Products;

            if (!client.Indices.Exists(indexName).Exists)
            {
                var descriptor = new CreateIndexDescriptor(indexName);
                descriptor = ConfigureIndex(descriptor);
                client.Indices.Create(indexName, c => descriptor);
            }
        }

        public CreateIndexDescriptor ConfigureIndex(CreateIndexDescriptor descriptor)
        {
            return descriptor
                .Map<ProductDocument>(mappingDescriptor => mappingDescriptor.Dynamic(false)
                    .Properties(props => props
                        .Keyword(k => k
                            .Name(product => product.Id))
                        .Keyword(k => k
                            .Name(product => product.ProducerId))
                        .Keyword(k => k
                            .Name(product => product.SubcategoryId))
                        .Text(t => t
                            .Name(product => product.Name))
                        .Text(t => t
                            .Name(product => product.ArticleNumber))
                        .Text(t => t
                            .Name(product => product.CategoryName))
                        .Text(t => t
                            .Name(product => product.SubcategoryName))
                        .Number(t => t
                            .Name(product => product.Count))
                        .Text(t => t
                            .Name(product => product.UnitOfMeasurement))
                        .Date(t => t
                            .Name(product => product.CreationDate))
                        .Number(t => t
                            .Name(product => product.PricePerOne))
                        .Number(t => t
                            .Name(product => product.Status))
                        .Date(t => t
                            .Name(product => product.ExpirationDate))
                        .Keyword(t => t
                            .Name(product => product.ImageName))
                        .Text(t => t
                            .Name(product => product.ProducerName))
                        .Keyword(t => t
                            .Name(product => product.ProducerImageName))
                        .Number(t => t
                            .Name(product => product.Rating))
                        .Number(t => t
                            .Name(product => product.FeedbacksCount))));
        }

        public async Task LoadData(IElasticClient client, IApplicationDbContext dbContext, IMapper mapper)
        {
            var deleteResponse = client.DeleteByQuery<ProductDocument>(d => d
                .Index(Indecies.Products)
                .Query(q => q.MatchAll()));

            if (!deleteResponse.IsValid)
            {
                string message = $"Products documents was not deleted successfully from Elasticsearch.";
                string userFacingMessage = CultureHelper.Exception("ElasticsearchProductsNotDeleted");

                throw new ApplicationException(message, userFacingMessage);
            }

            var products = await dbContext.Products.Include(p => p.Subcategory)
                .Include(p => p.Category)
                .Include(p => p.Feedbacks)
                .ToArrayAsync();

            var documents = new ProductDocument[products.Length];

            for (int i = 0; i < products.Length; i++)
            {
                documents[i] = mapper.Map<ProductDocument>(products[i]);

                if (products[i].Producer == Producer.Farm)
                {
                    var farm = await dbContext.Farms.FirstOrDefaultAsync(f => f.Id == products[i].ProducerId);

                    if (farm == null)
                    {
                        string message = $"Farm with Id {products[i].ProducerId} was not found.";
                        string userFacingMessage = CultureHelper.Exception("FarmNotFound");

                        throw new NotFoundException(message, userFacingMessage);
                    }

                    documents[i].ProducerName = farm.Name;
                    documents[i].ProducerImageName =
                        (farm.ImagesNames != null && farm.ImagesNames.Count > 0)
                        ? farm.ImagesNames[0]
                        : "";

                    documents[i].HasOnlinePayment = farm.PaymentTypes != null && farm.PaymentTypes.Contains(PaymentType.Online);
                    documents[i].ReceivingMethods = farm.ReceivingMethods;
                }
                else if (products[i].Producer == Producer.Seller)
                {
                    var seller = await dbContext.Sellers.FirstOrDefaultAsync(f => f.Id == products[i].ProducerId);

                    if (seller == null)
                    {
                        string message = $"Account with Id {products[i].ProducerId} was not found.";
                        string userFacingMessage = CultureHelper.Exception("AccountNotFound");

                        throw new NotFoundException(message, userFacingMessage);
                    }

                    documents[i].ProducerName = seller.Surname + " " + seller.Name;
                    documents[i].ProducerImageName =
                        (seller.ImagesNames != null && seller.ImagesNames.Count > 0)
                        ? seller.ImagesNames[0]
                        : "";

                    documents[i].HasOnlinePayment = seller.PaymentTypes != null && seller.PaymentTypes.Contains(PaymentType.Online);
                    documents[i].ReceivingMethods = seller.ReceivingMethods;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            var bulkIndexResponse = client.IndexMany(documents);

            if (!bulkIndexResponse.IsValid)
            {
                string message = $"Products documents was not uploaded successfully to Elasticsearch.";
                string userFacingMessage = CultureHelper.Exception("ElasticsearchProductsNotUpoaded");

                throw new ApplicationException(message, userFacingMessage);
            }
        }
    }

}
