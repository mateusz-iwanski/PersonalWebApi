using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using PersonalWebApi.Models.Models.SemanticKernel;
using System.Text;

namespace PersonalWebApi.Services.Services.History
{
    public interface IPersistentChatHistoryService
    {
        ChatMessageContent this[int index] { get; set; }

        int Count { get; }
        bool IsReadOnly { get; }

        void Add(ChatMessageContent item);
        void AddAssistantMessage(string content);
        void AddMessage(AuthorRole authorRole, ChatMessageContentItemCollection contentItems, Encoding? encoding = null, IReadOnlyDictionary<string, object?>? metadata = null);
        void AddMessage(AuthorRole authorRole, string content, Encoding? encoding = null, IReadOnlyDictionary<string, object?>? metadata = null);
        void AddMessageWithTimestamp(AuthorRole authorRole, string content, DateTime timestamp, Encoding? encoding = null, IReadOnlyDictionary<string, object?>? metadata = null);
        void AddRange(IEnumerable<ChatMessageContent> items);
        void AddSystemMessage(string content);
        void AddUserMessage(ChatMessageContentItemCollection contentItems);
        void AddUserMessage(string content);
        void Clear();
        bool Contains(ChatMessageContent item);
        void CopyTo(ChatMessageContent[] array, int arrayIndex);
        IEnumerator<ChatMessageContent> GetEnumerator();
        ChatMessageContent? GetLatestMessage();
        List<ChatMessageContent> GetMessagesByAuthor(AuthorRole authorRole);
        int IndexOf(ChatMessageContent item);
        void Insert(int index, ChatMessageContent item);
        bool Remove(ChatMessageContent item);
        void RemoveAt(int index);
        void RemoveRange(int index, int count);
        Task SaveChatAsync();


        // additional functionality
        Task<ChatHistory> LoadPersistanceConversationAsync();
        Task<ChatHistory> LoadStorageEventsAsync();
        ChatHistory GetChatHistory();
    }
}