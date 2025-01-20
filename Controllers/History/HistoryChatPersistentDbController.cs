using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;
using PersonalWebApi.Models.Models.SemanticKernel;
using PersonalWebApi.Services.Services.History;
using System.Text;

namespace PersonalWebApi.Controllers.History
{
    /// <summary>
    /// Controller for managing persistent chat history between users and the assistant AI.
    /// </summary>
    [ApiController]
    [Route("api/chat/history/persistent")]
    public class HistoryChatPersistentDbController : ControllerBase
    {
        private readonly PersistentChatHistoryService _chatHistoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryChatPersistentDbController"/> class.
        /// </summary>
        /// <param name="chatHistoryService">Service for managing chat history.</param>
        /// <param name="httpContextAccessor">Accessor for HTTP context.</param>
        public HistoryChatPersistentDbController(PersistentChatHistoryService chatHistoryService)
        {
            _chatHistoryService = chatHistoryService;
        }

        /// <summary>
        /// Adds a new message to the persistent chat history.
        /// </summary>
        /// <param name="messageDto">The message content to be added.</param>
        /// <param name="role">The role of the author. Possible values: User, Assistant, System.</param>
        /// <returns>Returns a status indicating the result of the operation.</returns>
        /// <remarks>
        /// This endpoint allows adding a new message to the chat history. The message content is provided in the request body,
        /// and the role of the author is specified as a query parameter.
        /// </remarks>
        [HttpPost("add")]
        public async Task<IActionResult> AddMessage([FromBody] ChatMessagePersistenceDto messageDto, [FromQuery] AuthorRole role)
        {
            if (messageDto == null)
            {
                return BadRequest("Message content is required.");
            }

            // Add the message to the chat history
            _chatHistoryService.AddMessage(role, messageDto.Message, Encoding.UTF8, messageDto.Metadata);

            // Save the message to Cosmos DB
            await _chatHistoryService.SaveChatAsync();

            return Ok("Message added and saved successfully.");
        }

        /// <summary>
        /// Retrieves the latest message from the persistent chat history.
        /// </summary>
        /// <returns>Returns the latest message if found; otherwise, returns a not found status.</returns>
        /// <remarks>
        /// This endpoint retrieves the most recent message from the chat history.
        /// </remarks>
        [HttpGet("get/latest")]
        public IActionResult GetLatestMessage()
        {
            var latestMessage = _chatHistoryService.GetLatestMessage();
            if (latestMessage == null)
            {
                return NotFound("No messages found.");
            }

            return Ok(latestMessage);
        }
    }
}
