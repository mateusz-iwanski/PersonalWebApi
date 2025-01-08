﻿using PersonalWebApi.Exceptions;
using PersonalWebApi.Models.Azure;
using PersonalWebApi.Models.Models.Azure;
using PersonalWebApi.Seeder.Agent.History;
using PersonalWebApi.Services.Azure;

namespace PersonalWebApi.Seeder.Seeder.PageContent
{
    public class PageContentDbSeeder
    {
        private readonly IConfiguration _configuration;
        private readonly ICosmosDbService _cosmosDbService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryCosmosDbSeeder"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="service">The Cosmos DB service.</param>
        /// <exception cref="ArgumentNullException">Thrown when configuration or service is null.</exception>
        public PageContentDbSeeder(IConfiguration configuration, ICosmosDbService service)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _cosmosDbService = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Seeds the Cosmos DB with the required database and container if they do not exist.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="SettingsException">Thrown when the database name is not found in the configuration.</exception>
        public async Task SeedIfDbAndContainerNotExists()
        {
            var cosmosDbDatabaseName = _configuration.GetSection("Azure:CosmosDb:CosmosDbDatabaseName").Value ??
                throw new SettingsException("Azure:CosmosDb:CosmosDbDatabaseName in settings not exists.");

            await _cosmosDbService.CreateDatabaseAndContainerIfNotExistsAsync(
                cosmosDbDatabaseName,
                PageContentCosmosDbDto.ContainerNameStatic(),
                PageContentCosmosDbDto.PartitionKeyNameStatic()
            );
        }
    }
}
