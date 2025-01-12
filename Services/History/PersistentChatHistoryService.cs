using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
using Microsoft.SemanticKernel;
using System.Collections.Generic;
using System.Collections;
using PersonalWebApi.Models.Models.SemanticKernel;
using PersonalWebApi.Services.Services.System;
using OllamaSharp.Models.Chat;
using OpenAI.Assistants;

namespace PersonalWebApi.Services.Services.History
{
    public class PersistentChatHistoryService : IEnumerable<ChatMessageContent>, IPersistentChatHistoryService
    {
        private readonly ChatHistory _chatHistory;
        private readonly IAssistantHistoryManager _assistantHistoryManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PersistentChatHistoryService(IAssistantHistoryManager assistantHistoryManager,  IHttpContextAccessor httpContextAccessor)
        {
            _chatHistory = new ChatHistory();
            _assistantHistoryManager = assistantHistoryManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public int Count => _chatHistory.Count;

        public void AddMessage(AuthorRole authorRole, string content, Encoding? encoding = null, IReadOnlyDictionary<string, object?>? metadata = null) =>
            _chatHistory.AddMessage(authorRole, content, encoding, metadata);

        public void AddMessage(AuthorRole authorRole, ChatMessageContentItemCollection contentItems, Encoding? encoding = null, IReadOnlyDictionary<string, object?>? metadata = null) =>
            _chatHistory.AddMessage(authorRole, contentItems, encoding, metadata);

        public void AddUserMessage(string content) =>
            _chatHistory.AddUserMessage(content);

        public void AddUserMessage(ChatMessageContentItemCollection contentItems) =>
            _chatHistory.AddUserMessage(contentItems);

        public void AddAssistantMessage(string content) =>
            _chatHistory.AddAssistantMessage(content);

        public void AddSystemMessage(string content) =>
            _chatHistory.AddSystemMessage(content);

        public void Add(ChatMessageContent item) =>
            _chatHistory.Add(item);

        public void AddRange(IEnumerable<ChatMessageContent> items) =>
            _chatHistory.AddRange(items);

        public void Insert(int index, ChatMessageContent item) =>
            _chatHistory.Insert(index, item);

        public void CopyTo(ChatMessageContent[] array, int arrayIndex) =>
            _chatHistory.CopyTo(array, arrayIndex);

        public void Clear() =>
            _chatHistory.Clear();

        public ChatMessageContent this[int index]
        {
            get => _chatHistory[index];
            set => _chatHistory[index] = value;
        }

        public bool Contains(ChatMessageContent item) =>
            _chatHistory.Contains(item);

        public int IndexOf(ChatMessageContent item) =>
            _chatHistory.IndexOf(item);

        public void RemoveAt(int index) =>
            _chatHistory.RemoveAt(index);

        public bool Remove(ChatMessageContent item) =>
            _chatHistory.Remove(item);

        public void RemoveRange(int index, int count) =>
            _chatHistory.RemoveRange(index, count);

        public bool IsReadOnly => ((ICollection<ChatMessageContent>)_chatHistory).IsReadOnly;

        public IEnumerator<ChatMessageContent> GetEnumerator() =>
             ((IEnumerable<ChatMessageContent>)_chatHistory).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)_chatHistory).GetEnumerator();

        // Additional functionality
        public void AddMessageWithTimestamp(AuthorRole authorRole, string content, DateTime timestamp, Encoding? encoding = null, IReadOnlyDictionary<string, object?>? metadata = null)
        {
            var message = new ChatMessageContent
            {
                Role = authorRole,
                Content = content,
                Metadata = metadata,
                Encoding = encoding
            };
            _chatHistory.Add(message);
        }

        public List<ChatMessageContent> GetMessagesByAuthor(AuthorRole authorRole)
        {
            var messages = new List<ChatMessageContent>();
            foreach (var message in _chatHistory)
            {
                if (message.Role == authorRole)
                {
                    messages.Add(message);
                }
            }
            return messages;
        }

        public ChatMessageContent? GetLatestMessage()
        {
            if (_chatHistory.Count == 0)
            {
                return null;
            }
            return _chatHistory[_chatHistory.Count - 1];
        }

        // Cosmos DB functionality
        public async Task SaveChatAsync()
        {
            (Guid conversationUuid, Guid sessionUuid) = ContextAccessorReader.RetrieveCrucialUuid(_httpContextAccessor);

            foreach (var message in _chatHistory)
            {
                var messageDto = new ChatMessagePersistenceDto(conversationUuid, sessionUuid)
                {
                    Message = message.Content,
                    Role = message.Role.ToString(),
                    Encoding = message.Encoding?.WebName,
                    Metadata = message.Metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                };

                await _assistantHistoryManager.SaveAsync(messageDto);
            }
        }


        // load conversation 
        public async Task<ChatHistory> LoadConversationAsync()
        {
            (Guid conversationUuid, Guid sessionUuid) = ContextAccessorReader.RetrieveCrucialUuid(_httpContextAccessor);
            
            var messages = await _assistantHistoryManager.LoadAsync<ChatMessagePersistenceDto>(conversationUuid);
            
            foreach (var message in messages)
            {
                var chatMessageContent = new ChatMessageContent
                {
                    Role = FromString(message.Role),
                    Content = message.Message,
                    Metadata = message.Metadata,
                    Encoding = Encoding.GetEncoding(message.Encoding)
                };
                _chatHistory.Add(chatMessageContent);
            }

            return _chatHistory;
        }

        public static AuthorRole FromString(string role)
        {
            return role.ToLower() switch
            {
                "system" => AuthorRole.System,
                "assistant" => AuthorRole.Assistant,
                "user" => AuthorRole.User,
                "tool" => AuthorRole.Tool,
                _ => throw new ArgumentException($"Invalid role: {role}")
            };
        }
    }
}
