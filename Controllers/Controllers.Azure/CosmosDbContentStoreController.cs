using Microsoft.AspNetCore.Mvc;
using PersonalWebApi.Models;
using PersonalWebApi.Models.Azure;
using PersonalWebApi.Services.Azure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalWebApi.Controllers.Azure
{
    [ApiController]
    [Route("api/cosmosdb/item")]
    public class CosmosDbContentStoreDtoController : ControllerBase
    {
        private readonly ICosmosDbContentStoreService _service;

        public CosmosDbContentStoreDtoController(ICosmosDbContentStoreService service)
        {
            _service = service;
        }

        /// <summary>
        /// Creates a new item in the Cosmos DB content store for site.
        /// </summary>
        /// <param name="item">The item to be created.</param>
        /// <returns>IActionResult indicating the result of the create operation.</returns>
        /// <remarks>Data should not exceed 1.99 MB.</remarks>
        [HttpPost("www-content/create")]
        public async Task<IActionResult> CreateItem([FromBody] SiteContentStoreCosmosDbDto item)
        {
            if (item.Data.Length > 1990000) // Check if data exceeds 1.99 MB
            {
                return BadRequest("Data exceeds the maximum allowed size of 1.99 MB.");
            }

            var response = await _service.CreateItemAsync(item);
            return Created();
        }

        /// <summary>
        /// Retrieves an item from the Cosmos DB content store by ID and URI.
        /// </summary>
        /// <param name="id">The ID of the item.</param>
        /// <param name="uri">The URI of the item.</param>
        /// <returns>IActionResult with the retrieved item.</returns>
        [HttpGet("get/{id}/{uri}")]
        public async Task<IActionResult> GetItemAsync(string id, string uri)
        {
            var response = await _service.GetItemAsync(id, uri);
            return Ok(response.Resource);
        }

        /// <summary>
        /// Retrieves an item from the Cosmos DB content store by UUID.
        /// </summary>
        /// <param name="uuid">The UUID of the item.</param>
        /// <returns>IActionResult with the retrieved item.</returns>
        [HttpGet("get-by-uuid/{uuid}")]
        public async Task<IActionResult> GetByUuidAsync(string uuid)
        {
            var response = await _service.GetByUuidAsync(uuid);
            return Ok(response);
        }

        /// <summary>
        /// Updates an existing item in the Cosmos DB content store.
        /// </summary>
        /// <param name="id">The ID of the item to be updated.</param>
        /// <param name="item">The updated item data.</param>
        /// <returns>IActionResult with the updated item.</returns>
        [HttpPut("www-content/put/{id}/{uri}")]
        public async Task<IActionResult> UpdateItem(string id, [FromBody] SiteContentStoreCosmosDbDto item)
        {
            var response = await _service.UpdateItemAsync(id, item);
            return Ok(response.Resource);
        }

        /// <summary>
        /// Deletes an item from the Cosmos DB content store by ID and URI.
        /// </summary>
        /// <param name="id">The ID of the item to be deleted.</param>
        /// <param name="uri">The URI of the item to be deleted.</param>
        /// <returns>IActionResult indicating the result of the delete operation.</returns>
        [HttpDelete("delete/{uuid}/{domain}")]
        public async Task<IActionResult> DeleteItemAsync(string id, string uri)
        {
            await _service.DeleteItemAsync(id, uri);
            return NoContent();
        }

        /// <summary>
        /// Retrieves a list of items from the Cosmos DB content store based on a query.
        /// </summary>
        /// <param name="query">The query to filter items.</param>
        /// <returns>IActionResult with the list of retrieved items.</returns>
        [HttpGet("list")]
        public async Task<IActionResult> GetItemsAsync([FromQuery] string query)
        {
            var items = await _service.GetItemsAsync(query);
            return Ok(items);
        }
    }
}
