namespace PersonalWebApi.Processes.Qdrant.Events
{
    public static class QdrantEvents
    {
        public const string StartProcess = nameof(StartProcess);
        public const string EmbeddingAdded = nameof(EmbeddingAdded);

        // it is only available in Qdrant pipeline, not use it anywhere else
        public const string ChunksTagified = nameof(ChunksTagified);
    }
}
