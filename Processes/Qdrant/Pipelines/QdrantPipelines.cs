using Microsoft.SemanticKernel;
using PersonalWebApi.Processes.Document.Events;
using PersonalWebApi.Processes.Document.Models;
using PersonalWebApi.Processes.FileStorage.Events;
using PersonalWebApi.Processes.FileStorage.Processes;
using PersonalWebApi.Processes.FileStorage.Steps;
using PersonalWebApi.Processes.Qdrant.Events;
using PersonalWebApi.Processes.Qdrant.Processes;
using PersonalWebApi.Services.Qdrant.Processes.Steps;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.Qdrant.Pipelines
{
    public class QdrantPipelines
    {
        [Experimental("SKEXP0080")]
        public async Task Add(Kernel kernel, DocumentStepDto documentStepDto)
        {
            // add from kernelrouter

            ProcessBuilder process = new("QdrantPipielineAdd");

            // file storage steps
            var fileStorageStep = process.AddStepFromType<FileStorageStep>();
            var logFileActionStep = process.AddStepFromType<LogFileActionStep>();

            // qdrant steps
            var reader = process.AddStepFromType<DocumentReaderStep>();
            var chunker = process.AddStepFromType<TextChunkerStep>();
            var fileMetadataComposer = process.AddStepFromType<FileMetadataComposer>();
            var qdrantAdd = process.AddStepFromType<QdrantStep>();


            process.OnInputEvent(QdrantEvents.StartProcess)
                .SendEventTo(new ProcessFunctionTargetBuilder(fileStorageStep, functionName: FileStorageFunctions.UploadIFormFile));
            
            fileStorageStep.OnEvent(FileEvents.Uploaded)
                .SendEventTo(new ProcessFunctionTargetBuilder(logFileActionStep, functionName: LogFileActionStepFunctions.SaveActionLog, parameterName: "documentStepDto"))
                .SendEventTo(new ProcessFunctionTargetBuilder(reader, functionName: DocumentReaderStepFunctions.ReadUri, parameterName: "documentStepDto"))
                .SendEventTo(new ProcessFunctionTargetBuilder(fileMetadataComposer, functionName: FileMetadataComposerFunction.Collect, parameterName: "documentStepDto"));

            reader.OnEvent(DocumentEvents.Readed)
                .SendEventTo(new ProcessFunctionTargetBuilder(chunker, functionName: TextChunkerStepFunctions.ChunkText, parameterName: "documentStepDto"));
            //
            chunker.OnEvent(DocumentEvents.Chunked)
                .SendEventTo(new ProcessFunctionTargetBuilder(qdrantAdd, functionName: QdrantStepFunctions.AddEmbedding, parameterName: "documentStepDto"));


            // TODO: cos wymyslec, mozliwe ze sam framework dziala nieprawidlowo
            // nie dziala poniżej, nie wiem czemu nie moge odczytac OnEvent w tym miejscu z UploadFileProcess.CreateProcess
            // w UploadFileProcess.CreateProcess dziala i eventy sie komunikuja
            // w tym miejscu nie reaguje na zaden onevent wykonany w UploadFileProcess.CreateProcess
            // ponizej wszystkie mozliwe wywolania onevent, nic nie dziala
            //var documentreadersterptest = process.AddStepFromType<DocumentReaderStep>();

            //process.OnInputEvent(QdrantEvents.StartProcess).SendEventTo(fileStorage.WhereInputEventIs(FileEvents.StartProcess));

            //fileStorage.OnEvent(FileEvents.Uploaded).SendEventTo(new ProcessFunctionTargetBuilder(documentreadersterptest, nameof(DocumentReaderStepFunctions.PrintIntroMessage)));
            //fileStorage.OnEvent(FileEvents.ActionLogSaved).SendEventTo(new ProcessFunctionTargetBuilder(documentreadersterptest, nameof(DocumentReaderStepFunctions.PrintIntroMessage)));
            //fileStorage.OnEvent(FileEvents.Uploaded).SendEventTo(new ProcessFunctionTargetBuilder(documentreadersterptest, nameof(DocumentReaderStepFunctions.PrintIntroMessage)));

            //process.OnEvent(FileEvents.Uploaded).SendEventTo(new ProcessFunctionTargetBuilder(documentreadersterptest, nameof(DocumentReaderStepFunctions.PrintIntroMessage)));
            //process.OnEvent(FileEvents.ActionLogSaved).SendEventTo(new ProcessFunctionTargetBuilder(documentreadersterptest, nameof(DocumentReaderStepFunctions.PrintIntroMessage)));
            //process.OnEvent(FileEvents.Uploaded).SendEventTo(new ProcessFunctionTargetBuilder(documentreadersterptest, nameof(DocumentReaderStepFunctions.PrintIntroMessage)));

            //process.OnInputEvent(QdrantEvents.StartProcess).SendEventTo(new ProcessFunctionTargetBuilder(documentreadersterptest, nameof(DocumentReaderStepFunctions.PrintIntroMessage)));

            //process.OnEvent(FileEvents.Uploaded).SendEventTo(qdrantAdd.WhereInputEventIs(QdrantEvents.StartProcess));

            var kernelProcess = process.Build();

            using var runningProcess = await kernelProcess.StartAsync(
                kernel,
                new KernelProcessEvent()
                {
                    Id = QdrantEvents.StartProcess,
                    Data = documentStepDto
                });

        }
    }
}
