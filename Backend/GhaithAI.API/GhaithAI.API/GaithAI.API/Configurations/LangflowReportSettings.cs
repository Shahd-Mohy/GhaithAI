namespace GhaithAI.API.GaithAI.API.Configurations
{
    /// <summary>
    /// Strongly-typed settings for the Langflow Report generation flow.
    /// Bound from the "LangflowReport" section in appsettings.json.
    /// Separate from <see cref="LangflowSettings"/> which handles the Chat flow.
    /// </summary>
    public class LangflowReportSettings
    {
        /// <summary>Base URL of the Langflow server (e.g. http://localhost:7860).</summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>Flow ID of the 3-stage clinical report pipeline on Langflow.</summary>
        public string FlowId { get; set; } = string.Empty;

        /// <summary>API key for authenticating with the Langflow server.</summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>Node ID for the Chat Input node — receives the session transcript.</summary>
        public string ChatInputNodeId { get; set; } = string.Empty;

        /// <summary>Node ID for the 1_Clinical_Analyst LLM node (temperature 0.07).</summary>
        public string ClinicalAnalystNodeId { get; set; } = string.Empty;

        /// <summary>Node ID for the 2_Medical_Compliance LLM node (temperature 0.01).</summary>
        public string MedicalComplianceNodeId { get; set; } = string.Empty;

        /// <summary>Node ID for the 3_JSON_Compiler custom component (temperature 0.00).</summary>
        public string JsonCompilerNodeId { get; set; } = string.Empty;
    }
}
