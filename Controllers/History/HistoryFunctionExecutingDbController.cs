using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using PersonalWebApi.Models.Models.SemanticKernel;
using PersonalWebApi.Services.NoSQLDB;

namespace PersonalWebApi.Controllers.History
{
    /// <summary>
    /// Controller for managing function execution history in Cosmos DB.
    /// This data includes information about functions executed by the Semantic Kernel.
    /// </summary>
    [ApiController]
    [Route("api/function-execution/history")]
    public class HistoryFunctionExecutingDbController : ControllerBase
    {
        private readonly INoSqlDbService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionExecutingHistoryController"/> class.
        /// </summary>
        /// <param name="service">Service for interacting with Cosmos DB.</param>
        public HistoryFunctionExecutingDbController(INoSqlDbService service)
        {
            _service = service;
        }

        #region FunctionExecutionHistory

        /// <summary>
        /// Creates a new function execution history item in Cosmos DB.
        /// </summary>
        /// <param name="functionHistoryDto">The DTO containing the function execution history data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult with the created function execution history item.</returns>
        /// <remarks>
        /// This endpoint allows creating a new function execution history item in the function execution history container.
        /// </remarks>
        [HttpPost("create")]
        public async Task<IActionResult> CreateFunctionHistory([FromBody] FunctionExecutingHistoryDto functionHistoryDto)
        {
            var response = await _service.CreateItemAsync(functionHistoryDto);
            return Ok(response.Resource);
        }

        /// <summary>
        /// Retrieves a function execution history item from Cosmos DB based on the provided item ID and conversation UUID.
        /// </summary>
        /// <param name="id">The unique identifier of the function execution history item.</param>
        /// <param name="conversationUuid">The unique identifier of the conversation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult with the retrieved function execution history item.</returns>
        /// <remarks>
        /// This endpoint retrieves a specific function execution history item from the function execution history container using the item ID and conversation UUID.
        /// </remarks>
        [HttpGet("get/{id}/{conversationUuid}")]
        public async Task<IActionResult> GetFunctionHistory(string id, string conversationUuid)
        {
            var response = await _service.GetItemAsync<FunctionExecutingHistoryDto>(id, FunctionExecutingHistoryDto.ContainerNameStatic(), conversationUuid);
            return Ok(response.Resource);
        }

        /// <summary>
        /// Retrieves function execution history items from Cosmos DB based on a query.
        /// </summary>
        /// <param name="query">The query string to filter the items.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult with the retrieved function execution history items.</returns>
        /// <remarks>
        /// This endpoint retrieves function execution history items from the function execution history container based on a specified query.
        /// </remarks>
        [HttpGet("get/query")]
        public async Task<IActionResult> QueryFunctionHistory([FromQuery] string query)
        {
            var queryDefinition = new QueryDefinition(query);
            var result = await _service.GetByQueryAsync<FunctionExecutingHistoryDto>(queryDefinition, FunctionExecutingHistoryDto.ContainerNameStatic());
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing function execution history item in Cosmos DB.
        /// </summary>
        /// <param name="functionHistoryDto">The DTO containing the updated function execution history data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult with the updated function execution history item.</returns>
        /// <remarks>
        /// This endpoint updates an existing function execution history item in the function execution history container.
        /// </remarks>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateFunctionHistory([FromBody] FunctionExecutingHistoryDto functionHistoryDto)
        {
            var response = await _service.UpdateItemAsync(functionHistoryDto);
            return Ok(response.Resource);
        }

        /// <summary>
        /// Deletes a function execution history item from Cosmos DB based on the provided item ID and conversation UUID.
        /// </summary>
        /// <param name="id">The unique identifier of the function execution history item to be deleted.</param>
        /// <param name="conversationUuid">The unique identifier of the conversation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult indicating the result of the delete operation.</returns>
        /// <remarks>
        /// This endpoint deletes a specific function execution history item from the function execution history container using the item ID and conversation UUID.
        /// </remarks>
        [HttpDelete("delete/{id}/{conversationUuid}")]
        public async Task<IActionResult> DeleteFunctionHistory(string id, string conversationUuid)
        {
            var response = await _service.DeleteItemAsync<FunctionExecutingHistoryDto>(id, FunctionExecutingHistoryDto.ContainerNameStatic(), conversationUuid);
            return Ok(response.Resource);
        }

        /// <summary>
        /// Retrieves the schema of the function execution history DTO.
        /// </summary>
        /// <returns>An IActionResult containing the JSON structure of the function execution history DTO.</returns>
        /// <remarks>
        /// This endpoint returns the JSON schema of the function execution history DTO for reference.
        /// </remarks>
        [HttpGet("get/schema")]
        public IActionResult GetFunctionHistorySchema()
        {
            var functionHistoryDto = new FunctionExecutingHistoryDto(
                conversationUuid: Guid.Empty,
                sessionUuid: Guid.Empty,
                inputArguments: new Dictionary<string, string>(),
                functionName: string.Empty,
                pluginName: string.Empty,
                functionDescription: string.Empty,
                renderedPrompt: string.Empty,
                status: string.Empty,
                executionSettings: new List<Dictionary<string, string?>>()
            );

            string jsonStructure = JsonConvert.SerializeObject(functionHistoryDto, Formatting.Indented);
            return Ok(jsonStructure);
        }

        #endregion
    }
}
