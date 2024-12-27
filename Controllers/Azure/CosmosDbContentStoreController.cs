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
    [Route("api/cosmosdb/www-content-store")]
    public class CosmosDbContentStoreDtoController : ControllerBase
    {
        private readonly ICosmosDbContentStoreService _service;

        public CosmosDbContentStoreDtoController(ICosmosDbContentStoreService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateItem([FromBody] SiteContentStoreCosmosDbDto item)
        {
            if (item.Data.Length > 1990000) // Check if data exceeds 1.99 MB
            {
                return BadRequest("Data exceeds the maximum allowed size of 1.99 MB.");
            }

            var response = await _service.CreateItemAsync(item);
            return Created();
        }

        [HttpGet("get/{id}/{uri}")]
        public async Task<IActionResult> GetItemAsync(string id, string uri)
        {
            var response = await _service.GetItemAsync(id, uri);
            return Ok(response.Resource);
        }

        [HttpGet("get-by-uuid/{uuid}")]
        public async Task<IActionResult> GetByUuidAsync(string uuid)
        {
            var response = await _service.GetByUuidAsync(uuid);
            return Ok(response);
        }

        [HttpPut("put/{id}/{uri}")]
        public async Task<IActionResult> UpdateItem(string id, [FromBody] SiteContentStoreCosmosDbDto item)
        {
            var response = await _service.UpdateItemAsync(id, item);
            return Ok(response.Resource);
        }

        [HttpDelete("delete/{uuid}/{domain}")]
        public async Task<IActionResult> DeleteItemAsync(string id, string uri)
        {
            await _service.DeleteItemAsync(id, uri);
            return NoContent();
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetItemsAsync([FromQuery] string query)
        {
            var items = await _service.GetItemsAsync(query);
            return Ok(items);
        }
    }
}