﻿using LLama.Common;
using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using PersonalWebApi.Entities.System;
using PersonalWebApi.Models.Models.Azure;
using System.Security.Claims;

namespace PersonalWebApi.Models.Models.Memory
{
    /// <summary>
    /// Represents a Data Transfer Object (DTO) for storing chat history in a Cosmos DB.
    /// This class extends the <see cref="CosmosDbDtoBase"/> to include specific properties
    /// related to chat messages, such as the message content, role of the sender, and any
    /// associated actions or metadata. It provides methods to retrieve container and partition
    /// key names, ensuring the data is correctly partitioned and stored in the Cosmos DB.
    /// Normaly, this class represents short term messages.
    /// </summary>
    public class ChatHistoryShortTermFileMessageDto : ChatHistoryShortTermMessageDto
    {
        [JsonProperty(PropertyName = "fileId")]
        public Guid FileId { get; set; }

        [JsonProperty(PropertyName = "filePath")]
        public string SourceFilePath { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "fileName")]
        public string SourceFileName { get; set; } = string.Empty;

        public ChatHistoryShortTermFileMessageDto(Guid conversationUuid, Guid sessionUuid, Guid fileId)
            : base(conversationUuid, sessionUuid)
        {
            FileId = fileId;
        }

    }
}
