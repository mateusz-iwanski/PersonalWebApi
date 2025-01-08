using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.KernelMemory;
using PersonalWebApi.Models.Models.Azure;
using PersonalWebApi.Services.Azure;
using System.Security.Claims;

namespace PersonalWebApi.Agent
{
    /// <summary>
    /// Manages the history of interactions with the assistant, including loading and saving chat history.
    /// Loading and saving chat history is done with Microsoft Kernel Memory.
    /// This class interacts with Cosmos DB to store and retrieve chat history records from/to Microsoft Kernel Memory, 
    /// ensuring that only authorized users can access their respective histories.
    /// </summary>
    public class AssistantHistoryManager : IAssistantHistoryManager
    {
        private readonly ICosmosDbService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssistantHistoryManager"/> class.
        /// </summary>
        /// <param name="service">The Cosmos DB service for data operations.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor to retrieve user information.</param>
        public AssistantHistoryManager(ICosmosDbService service, IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
            _user = getCurrentUser().FindFirstValue(ClaimTypes.Name) ?? ClaimTypes.Anonymous;
        }

        /// <summary>
        /// Retrieves the current user from the HTTP context.
        /// </summary>
        /// <returns>The current user's claims principal.</returns>
        private ClaimsPrincipal getCurrentUser()
        {
            return _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        }

        /// <summary>
        /// Checks if the current user is permitted to access the specified chat history.
        /// </summary>
        /// <typeparam name="T">The type of the chat history record.</typeparam>
        /// <param name="result">The chat history record to check.</param>
        /// <returns>True if the user is permitted; otherwise, throws an <see cref="UnauthorizedAccessException"/>.</returns>
        private bool isUserPermittedForHistory<T>(T? result) where T : CosmosDbDtoBase
        {
            // If no history exists, return true (indicating a new conversation)
            if (EqualityComparer<T>.Default.Equals(result, default(T))) return true;

            // If the history exists and the user matches, return true
            if (result.CreatedBy == _user) return true;

            throw new UnauthorizedAccessException("You do not have access to this chat history.");
        }

        /// <summary>
        /// Checks if the current user has access to the chat history for the specified conversation UUID.
        /// </summary>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        /// <returns>True if the user has access; otherwise, false.</returns>
        private async Task<bool> checkHistoryAccessForUserAsync(Guid conversationUuid)
        {
            var query = $"SELECT * FROM c WHERE c.conversationUuid = '{conversationUuid}'";
            var queryDefinition = new QueryDefinition(query);
            var result = await _service.GetByQueryAsync<ChatHistoryStoreDbDto>(queryDefinition, ChatHistoryStoreDbDto.ContainerNameStatic());

            if (result.Count > 0) 
                return isUserPermittedForHistory(result.FirstOrDefault());

            return isUserPermittedForHistory(default(CosmosDbDtoBase));
        }

        /// <summary>
        /// Loads the chat history for the specified conversation UUID into memory.
        /// </summary>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        /// <param name="memory">The memory interface to load the chat history into.</param>
        public async Task LoadAsync(Guid conversationUuid, IKernelMemory memory)
        {
            await checkHistoryAccessForUserAsync(conversationUuid);

            var query = $"SELECT * FROM c WHERE c.conversationUuid = '{conversationUuid}' ORDER BY c.createdAt ASC";
            var queryDefinition = new QueryDefinition(query);
            var result = await _service.GetByQueryAsync<ChatHistoryStoreDbDto>(queryDefinition, ChatHistoryStoreDbDto.ContainerNameStatic());

            foreach (var item in result)
                await memory.ImportTextAsync(item.Message);
        }

        /// <summary>
        /// Saves a new chat message to the chat history.
        /// </summary>
        /// <param name="sessionUuid">The unique identifier for the session.</param>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        /// <param name="message">The chat message to save.</param>
        /// <returns>The saved chat history record.</returns>
        public async Task<ChatHistoryStoreDbDto> SaveAsync(Guid sessionUuid, Guid conversationUuid, string message)
        {
            var chatHistory = new ChatHistoryStoreDbDto(conversationUuid, sessionUuid)
            {
                Message = message,
                CreatedBy = _user
            };
            return await _service.CreateItemAsync(chatHistory);
        }
    }
}
