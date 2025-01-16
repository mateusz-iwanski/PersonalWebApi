using Microsoft.SemanticKernel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Services.FileStorage
{
    // https://github.com/microsoft/semantic-kernel/blob/39934f5fa338141c8a64de96895a4e1f440638d7/dotnet/samples/GettingStartedWithProcesses/README.md
    // https://www.linkedin.com/pulse/introducing-semantic-kernel-process-library-new-era-ai-latorre-g8tef
    // https://github.com/microsoft/semantic-kernel/blob/39934f5fa338141c8a64de96895a4e1f440638d7/dotnet/samples/GettingStartedWithProcesses/Step00/Steps/DoSomeWorkStep.cs
    [Experimental("SKEXP0080")]
    public sealed class NextSteps : KernelProcessStep
    {
        [KernelFunction]
        public async ValueTask ExecuteAsync(KernelProcessStepContext context)
        {
            Debug.WriteLine("Step 2 - Doing Some Work...\n");
        }
    }
}
