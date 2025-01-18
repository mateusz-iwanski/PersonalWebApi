using Microsoft.SemanticKernel;
using PersonalWebApi.Processes.Document.Events;
using PersonalWebApi.Processes.FileStorage.Events;
using PersonalWebApi.Processes.FileStorage.Processes;
using PersonalWebApi.Processes.FileStorage.Steps;
using PersonalWebApi.Processes.Qdrant.Events;
using PersonalWebApi.Services.Qdrant.Processes.Steps;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.Qdrant.Processes
{
    public static class QdrantAddProcess
    {
        [Experimental("SKEXP0080")]
        public static ProcessBuilder CreateProcess(string processName = "QdrantAdd")
        {
            var process = new ProcessBuilder(processName);

            //var fileStorageStep = process.AddStepFromType<FileStorageStep>();
            //var fileStorageProcess = process.AddStepFromProcess(UploadFileProcess.CreateProcess());

            //var logFileActionStep = process.AddStepFromType<LogFileActionStep>();

            var qdrantAdd = process.AddStepFromType<QdrantStep>();
            var reader = process.AddStepFromType<DocumentReaderStep>();
            var chunker = process.AddStepFromType<TextChunkerStep>();

            process.OnInputEvent(QdrantEvents.StartProcess)
                .SendEventTo(new ProcessFunctionTargetBuilder(reader, functionName: DocumentReaderStepFunctions.ReadUri, parameterName: "uri"));

            reader.OnEvent(DocumentEvents.Readed)
                .SendEventTo(new ProcessFunctionTargetBuilder(chunker, functionName: TextChunkerStepFunctions.ChunkText, parameterName: "content"));

            chunker.OnEvent(DocumentEvents.Chunked)
                .SendEventTo(new ProcessFunctionTargetBuilder(qdrantAdd, functionName: QdrantStepFunctions.AddEmbedding, parameterName: "chunks"));

            //chunker.OnEvent(TextChunkerStepOutputEvents.Chunked)
            //    .SendEventTo(new ProcessFunctionTargetBuilder(qdrant, parameterName: "chunks"));

            return process;
        }
    }
}
