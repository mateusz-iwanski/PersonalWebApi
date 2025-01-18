﻿namespace PersonalWebApi.Processes.Document.Events
{
    public static class DocumentEvents
    {
        public const string StartProcess = nameof(StartProcess);

        // document reader step
        public const string Readed = nameof(Readed);

        // document summarizer step
        public const string SummaryGenerated = nameof(SummaryGenerated);

        // docuent chunker step
        public const string Chunked = nameof(Chunked);
    }
}