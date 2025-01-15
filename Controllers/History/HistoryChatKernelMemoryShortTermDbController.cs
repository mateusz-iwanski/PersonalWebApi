using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using PersonalWebApi.Models.Azure;
using PersonalWebApi.Models.Models.Memory;
using PersonalWebApi.Services.NoSQLDB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalWebApi.Controllers.History
{
    /// <summary>
    /// Controller for managing short-term chat history in Cosmos DB.
    /// This data are additional chat information which are generated when Kernel Memory short term memory is used
    /// </summary>
    [ApiController]
    [Route("api/chat/history/kernel-memory/short-term")]
    public class HistoryChatKernelMemoryShortTermDbController : ControllerBase
    {
        private readonly INoSqlDbService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryChatKernelMemoryShortTermDbController"/> class.
        /// </summary>
        /// <param name="service">Service for interacting with Cosmos DB.</param>
        public HistoryChatKernelMemoryShortTermDbController(INoSqlDbService service)
        {
            _service = service;
        }

        #region ChatHistory

        /// <summary>
        /// Creates a new chat history item in Cosmos DB.
        /// </summary>
        /// <param name="chatHistoryDto">The DTO containing the chat history data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult with the created chat history item.</returns>
        /// <remarks>
        /// This data are additional chat information which are generated when Kernel Memory short term memory is used.<br/>
        /// This endpoint allows creating a new chat history item in the short-term chat history container.
        /// </remarks>
        [HttpPost("create")]
        public async Task<IActionResult> CreateChatHistory([FromBody] ChatHistoryShortTermMessageDto chatHistoryDto)
        {
            var response = await _service.CreateItemAsync(chatHistoryDto);
            return Ok(response.Resource);
        }

        /// <summary>
        /// Retrieves a chat history item from Cosmos DB based on the provided item ID and conversation UUID.
        /// </summary>
        /// <param name="id">The unique identifier of the chat history item.</param>
        /// <param name="conversationUuid">The unique identifier of the conversation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult with the retrieved chat history item.</returns>
        /// <remarks>
        /// This endpoint retrieves a specific chat history item from the short-term chat history container using the item ID and conversation UUID.
        /// </remarks>
        [HttpGet("get/{id}/{conversationUuid}")]
        public async Task<IActionResult> GetChatHistory(string id, string conversationUuid)
        {
            var response = await _service.GetItemAsync<ChatHistoryShortTermMessageDto>(id, ChatHistoryShortTermMessageDto.ContainerNameStatic(), conversationUuid);
            return Ok(response.Resource);
        }

        /// <summary>
        /// Retrieves chat history items from Cosmos DB based on a query.
        /// </summary>
        /// <param name="query">The query string to filter the items.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult with the retrieved chat history items.</returns>
        /// <remarks>
        /// This endpoint retrieves chat history items from the short-term chat history container based on a specified query.
        /// </remarks>
        [HttpGet("get/query")]
        public async Task<IActionResult> QueryChatHistory([FromQuery] string query)
        {
            var queryDefinition = new QueryDefinition(query);
            var result = await _service.GetByQueryAsync<ChatHistoryShortTermMessageDto>(queryDefinition, ChatHistoryShortTermMessageDto.ContainerNameStatic());
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing chat history item in Cosmos DB.
        /// </summary>
        /// <param name="chatHistoryDto">The DTO containing the updated chat history data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult with the updated chat history item.</returns>
        /// <remarks>
        /// This endpoint updates an existing chat history item in the short-term chat history container.
        /// </remarks>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateChatHistory([FromBody] ChatHistoryShortTermMessageDto chatHistoryDto)
        {
            var response = await _service.UpdateItemAsync(chatHistoryDto);
            return Ok(response.Resource);
        }

        /// <summary>
        /// Deletes a chat history item from Cosmos DB based on the provided item ID and conversation UUID.
        /// </summary>
        /// <param name="id">The unique identifier of the chat history item to be deleted.</param>
        /// <param name="conversationUuid">The unique identifier of the conversation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult indicating the result of the delete operation.</returns>
        /// <remarks>
        /// This endpoint deletes a specific chat history item from the short-term chat history container using the item ID and conversation UUID.
        /// </remarks>
        [HttpDelete("delete/{id}/{conversationUuid}")]
        public async Task<IActionResult> DeleteChatHistory(string id, string conversationUuid)
        {
            var response = await _service.DeleteItemAsync<ChatHistoryShortTermMessageDto>(id, ChatHistoryShortTermMessageDto.ContainerNameStatic(), conversationUuid);
            return Ok(response.Resource);
        }

        /// <summary>
        /// Retrieves the schema of the chat history DTO.
        /// </summary>
        /// <returns>An IActionResult containing the JSON structure of the chat history DTO.</returns>
        /// <remarks>
        /// This endpoint returns the JSON schema of the chat history DTO for reference.
        /// </remarks>
        [HttpGet("get/schema")]
        public IActionResult GetChatHistorySchema()
        {
            var chatHistoryDto = new ChatHistoryShortTermMessageDto(
                conversationUuid: Guid.Empty,
                sessionUuid: Guid.Empty
            )
            {
                Message = string.Empty,
                Role = string.Empty,
                Action = string.Empty,
                ActionMessage = string.Empty,
                MessageType = string.Empty
            };

            string jsonStructure = JsonConvert.SerializeObject(chatHistoryDto, Formatting.Indented);
            return Ok(jsonStructure);
        }

        #endregion
    }
}
