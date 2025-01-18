namespace PersonalWebApi.Processes.FileStorage.Events
{
    public static class FileEvents
    {
        // file upload step
        public const string StartProcess = nameof(StartProcess);
        public const string Downloaded = nameof(Downloaded);
        public const string Uploaded = nameof(Uploaded);

        // log file action step
        public const string ActionLogSaved = nameof(ActionLogSaved);

        // file metadata composer step
        public const string MetadataCollected = nameof(MetadataCollected);
    }
}
