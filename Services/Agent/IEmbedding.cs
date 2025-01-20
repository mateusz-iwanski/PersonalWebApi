﻿namespace PersonalWebApi.Services.Services.Agent
{
    public interface IEmbedding
    {
        Task<ReadOnlyMemory<float>> EmbeddingAsync(string input, ulong dimensions);
    }
}
