using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using PersonalWebApi.Models;
using PersonalWebApi.Models.Azure;
using PersonalWebApi.Models.Models.Azure;
using PersonalWebApi.Services.Azure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalWebApi.Controllers.Azure
{
    [ApiController]
    [Route("api/cosmosdb/item/")]
    public class AzureCosmosPageContentDbController : ControllerBase
    {
        private readonly ICosmosDbService _service;

        public AzureCosmosPageContentDbController(ICosmosDbService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves the content of a page from Cosmos DB based on the provided item ID, container name, and partition key.
        /// </summary>
        /// <param name="itemId">The unique identifier of the item.</param>
        /// <param name="containerName">The name of the Cosmos DB container.</param>
        /// <param name="partitionKeyUriData">The partition key data for the item.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the page content DTO.</returns>
        [HttpGet("get/page-content/{itemId}/{containerName}/{partitionKeyUriData}")]
        public async Task<PageContentCosmosDbDto> GetPageContentAsync(Guid itemId, string containerName, string partitionKeyUriData)
        {
            return await _service.GetItemAsync<PageContentCosmosDbDto>(itemId.ToString(), containerName, partitionKeyUriData);
        }

        /// <summary>
        /// Creates a new page content item in Cosmos DB.
        /// </summary>
        /// <param name="siteContentStoreCosmosDbDto">The DTO containing the page content data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created page content DTO.</returns>
        [HttpPost("create/page-content")]
        public async Task<PageContentCosmosDbDto> AddPageContentAsync(PageContentCosmosDbDto siteContentStoreCosmosDbDto)
        {
            siteContentStoreCosmosDbDto.SetUser(User);
            return await _service.CreateItemAsync<PageContentCosmosDbDto>(siteContentStoreCosmosDbDto);
        }

        /// <summary>
        /// Retrieves a page content item from Cosmos DB based on a query.
        /// </summary>
        /// <param name="query">The query string to filter the items.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the page content DTO.</returns>
        /// <remarks>
        /// You can't modify id and uri fields.
        /// </remarks>
        [HttpGet("get/page-content/query")]
        public async Task<List<PageContentCosmosDbDto>> GetPageByQueryAsync([FromQuery] string query)
        {
            QueryDefinition queryDefinition = new QueryDefinition(query);
            return await _service.GetByQueryAsync<PageContentCosmosDbDto>(queryDefinition, PageContentCosmosDbDto.ContainerNameStatic());
        }

        /// <summary>
        /// Updates an existing page content item in Cosmos DB.
        /// </summary>
        /// <param name="item">The DTO containing the updated page content data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult with the updated item.</returns>
        [HttpPut("update/page-content")]
        public async Task<IActionResult> UpdatePageItem([FromBody] PageContentCosmosDbDto item)
        {
            var response = await _service.UpdateItemAsync(item);
            return Ok(response.Resource);
        }

        /// <summary>
        /// Deletes a page content item from Cosmos DB based on the provided item ID and URI.
        /// </summary>
        /// <param name="itemId">The unique identifier of the item to be deleted.</param>
        /// <param name="uriData">The URI of the item to be deleted.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult indicating the result of the delete operation.</returns>
        [HttpDelete("delete/page-content/{itemId}/{uriData}")]
        public async Task<IActionResult> DeleteItemAsync(string itemId, string uriData)
        {
            await _service.DeleteItemAsync<PageContentCosmosDbDto>(itemId, PageContentCosmosDbDto.ContainerNameStatic(), uriData);
            return NoContent();
        }

        /// <summary>
        /// Retrieves the schema of the page content DTO.
        /// </summary>
        /// <returns>An IActionResult containing the JSON structure of the page content DTO.</returns>
        [HttpGet("get/page-content/schema")]
        public IActionResult GetPageContentSchema()
        {
            var siteContentDto = new PageContentCosmosDbDto(
                uuid: string.Empty,
                domain: string.Empty,
                uri: string.Empty,
                content: string.Empty,
                tags: new List<string>(),
                conversationUuid: Guid.Empty,
                sessionUuid: Guid.Empty
            );

            string jsonStructure = JsonConvert.SerializeObject(siteContentDto, Formatting.Indented);
            return Ok(jsonStructure);
        }

       
    }
}
