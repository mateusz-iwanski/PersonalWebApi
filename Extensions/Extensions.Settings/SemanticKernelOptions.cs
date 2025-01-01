namespace PersonalWebApi.Extensions.ExtensionsSettings
{
    public class SemanticKernelOptions
    {
        public SemanticServicesAccess Access { get; set; }
        public class OpenAiAccess
        {
            public string ApiKey { get; set; }
            public string OrganizationId { get; set; }
            public string ServiceId { get; set; }
            public string DefaultModelId { get; set; }
        }

        public class SemanticServicesAccess
        {
            public OpenAiAccess OpenAi { get; set; }
        }
    }
}
