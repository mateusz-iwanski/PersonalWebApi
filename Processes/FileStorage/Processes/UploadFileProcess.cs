using Microsoft.SemanticKernel;
using PersonalWebApi.Processes.FileStorage.Events;
using PersonalWebApi.Processes.FileStorage.Steps;
using PersonalWebApi.Services.Qdrant.Processes.Steps;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Processes.FileStorage.Processes
{
    public static class UploadFileProcess
    {
        [Experimental("SKEXP0080")]
        public static ProcessBuilder CreateProcess(string processName = "FileStorageUploadFile")
        {
            var process = new ProcessBuilder(processName);

            var fileStorageStep = process.AddStepFromType<FileStorageStep>();
            var logFileActionStep = process.AddStepFromType<LogFileActionStep>();

            process.OnInputEvent(FileEvents.StartProcess).SendEventTo(new ProcessFunctionTargetBuilder(fileStorageStep));
            fileStorageStep.OnEvent(FileEvents.Uploaded).SendEventTo(new ProcessFunctionTargetBuilder(logFileActionStep, parameterName: "stepDataModel"));

            // logFileActionStep -> return FileStorageStepDto

            return process;
        }
    }
}
