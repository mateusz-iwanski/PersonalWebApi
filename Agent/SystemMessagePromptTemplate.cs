using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Prompts;
using System.Diagnostics.CodeAnalysis;

namespace PersonalWebApi.Agent
{
    [Experimental("KMEXP00")]
    public class SystemMessagePromptTemplate
    {
        private const string VerificationPrompt = """
                                              Facts:
                                              {{$facts}}
                                              ======
                                              Given only the facts above, verify the fact below.
                                              You don't know where the knowledge comes from, just answer.
                                              If you have sufficient information to verify, reply only with 'TRUE', nothing else.
                                              If you have sufficient information to deny, reply only with 'FALSE', nothing else.
                                              If you don't have sufficient information, reply with 'NEED MORE INFO'.
                                              User: {{$input}}
                                              Verification: 
                                              """;

        private readonly EmbeddedPromptProvider _fallbackProvider = new();

        public string ReadPrompt(string promptName)
        {
            switch (promptName)
            {
                case Constants.PromptNamesAnswerWithFacts:
                    return VerificationPrompt;

                default:
                    // Fall back to the default
                    return this._fallbackProvider.ReadPrompt(promptName);
            }
        }
    }
}
