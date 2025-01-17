using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Data.SqlClient;
using Microsoft.KernelMemory;
using PersonalWebApi.Models.Models.Azure;
using PersonalWebApi.Models.Models.Memory;
using PersonalWebApi.Services.NoSQLDB;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace PersonalWebApi.Services.Services.History
{
    /// <summary>
    /// Manages the history of interactions with the assistant, including loading and saving chat history.
    /// Loading and saving chat history is done with Microsoft Kernel Memory.
    /// This class interacts with Cosmos DB to store and retrieve chat history records from/to Microsoft Kernel Memory, 
    /// ensuring that only authorized users can access their respective histories.
    /// </summary>
    public class AssistantHistoryManager : IAssistantHistoryManager
    {
        private readonly INoSqlDbService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssistantHistoryManager"/> class.
        /// </summary>
        /// <param name="service">The Cosmos DB service for data operations.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor to retrieve user information.</param>
        public AssistantHistoryManager(INoSqlDbService service, IHttpContextAccessor httpContextAccessor)
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
            if (EqualityComparer<T>.Default.Equals(result, default)) return true;

            // If the history exists and the user matches, return true
            if (result.CreatedBy == _user) return true;

            throw new UnauthorizedAccessException("You do not have access to this chat history.");
        }

        /// <summary>
        /// Checks if the current user has access to the chat history for the specified conversation UUID.
        /// </summary>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        /// <returns>True if the user has access; otherwise, false.</returns>
        private async Task<bool> checkHistoryAccessForUserAsync<T>(Guid conversationUuid) where T : CosmosDbDtoBase
        {
            var query = $"SELECT * FROM c WHERE c.conversationUuid = '{conversationUuid}'";
            var queryDefinition = new QueryDefinition(query);

            // Use reflection to call the static method ContainerNameStatic on type T
            var containerName = (string)typeof(T).GetMethod("ContainerNameStatic", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);

            var result = await _service.GetByQueryAsync<T>(queryDefinition, containerName);

            if (result.Count > 0)
                return isUserPermittedForHistory(result.FirstOrDefault());

            return isUserPermittedForHistory(default(T));
        }

        /// <summary>
        /// Loads the chat history by date add for the specified conversation UUID into memory.
        /// </summary>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        public async Task<List<T>> LoadAsync<T>(Guid conversationUuid) where T : CosmosDbDtoBase
        {
            await checkHistoryAccessForUserAsync<T>(conversationUuid);

            var query = $"SELECT * FROM c WHERE c.conversationUuid = '{conversationUuid}' ORDER BY c.createdAt ASC";
            var queryDefinition = new QueryDefinition(query);

            // Use reflection to call the static method ContainerNameStatic on type T
            var containerName = (string)typeof(T).GetMethod("ContainerNameStatic", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);

            return await _service.GetByQueryAsync<T>(queryDefinition, containerName);
        }

        /// <summary>
        /// Saves a new chat message to the chat history.
        /// </summary>
        /// <param name="sessionUuid">The unique identifier for the session.</param>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        /// <param name="message">The chat message to save.</param>
        /// <returns>The saved chat history record.</returns>
        public async Task<T> SaveAsync<T>(T historyDto) where T : CosmosDbDtoBase
        {
            historyDto.CreatedBy = _user;
            return await _service.CreateItemAsync(historyDto);
        }

        /// <summary>
        /// Retrieves a list of items from the Cosmos DB container based on the specified conversation UUID.
        /// This method uses a generic type parameter to support various DTO types that inherit from <see cref="CosmosDbDtoBase"/>.
        /// It constructs a query to select items where the conversation UUID matches the provided value.
        /// The container name is dynamically determined using reflection to call the static method <c>ContainerNameStatic</c> on the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the items to retrieve, which must inherit from <see cref="CosmosDbDtoBase"/>.</typeparam>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of items of the specified type.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the current user does not have access to the specified chat history.</exception>
        /// <example>
        /// <code>
        /// // load data by CosmosDbDtoBase objects
        /// var ShortTermDeleteDocument = await assistantHistoryManager.LoadItemsAsync<ChatHistoryShortTermDeleteDocumentDto>(conversationUuid);
        /// var StorageEvents = await assistantHistoryManager.LoadItemsAsync<StorageEventsDto>(conversationUuid);
        /// </code>
        /// </example>
        /// <example>      
        public async Task<List<T>> LoadItemsAsync<T>(Guid conversationUuid) where T : CosmosDbDtoBase
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.conversationUuid = @conversationUuid")
                .WithParameter("@conversationUuid", conversationUuid.ToString());

            // Use reflection to call the static method ContainerNameStatic on type T
            var containerName = (string)typeof(T).GetMethod("ContainerNameStatic", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);

            return await _service.GetByQueryAsync<T>(query, containerName);
        }

        /// <summary>
        /// Retrieves a list of items from the Cosmos DB container based on the specified field criteria.
        /// This method uses a generic type parameter to support various DTO types that inherit from <see cref="CosmosDbDtoBase"/>.
        /// It constructs a query to select items where the specified fields match the provided values.
        /// The container name is dynamically determined using reflection to call the static method <c>ContainerNameStatic</c> on the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the items to retrieve, which must inherit from <see cref="CosmosDbDtoBase"/>.</typeparam>
        /// <param name="fieldCriteria">A dictionary of field names and values to filter the items.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of items of the specified type.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the current user does not have access to the specified chat history.</exception>
        /// <example>
        /// <code>
        /// // load data by CosmosDbDtoBase objects
        /// var criteria = new Dictionary<string, object> { { "conversationUuid", conversationUuid }, { "createdBy", "user@example.com" } };
        /// var items = await assistantHistoryManager.LoadItemsAsync<ChatHistoryShortTermDeleteDocumentDto>(criteria);
        /// </code>
        /// </example>
        public async Task<List<T>> LoadItemsAsync<T>(Dictionary<string, object> fieldCriteria) where T : CosmosDbDtoBase
        {
            var queryBuilder = new StringBuilder("SELECT * FROM c WHERE ");
            var queryParameters = new List<(string Name, object Value)>();

            foreach (var field in fieldCriteria)
            {
                queryBuilder.Append($"c.{field.Key} = @{field.Key} AND ");
                queryParameters.Add((field.Key, field.Value));
            }

            // Remove the trailing " AND "
            queryBuilder.Length -= 5;

            var queryDefinition = new QueryDefinition(queryBuilder.ToString());
            foreach (var param in queryParameters)
            {
                queryDefinition.WithParameter(param.Name, param.Value);
            }

            // Use reflection to call the static method ContainerNameStatic on type T
            var containerName = (string)typeof(T).GetMethod("ContainerNameStatic", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);

            return await _service.GetByQueryAsync<T>(queryDefinition, containerName);
        }


    }
}
