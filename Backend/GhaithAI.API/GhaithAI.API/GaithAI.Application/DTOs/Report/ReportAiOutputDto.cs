using System.Text.Json.Serialization;

namespace GhaithAI.API.GaithAI.Application.DTOs.Report
{
    // ─────────────────────────────────────────────────────────────────────────────
    // DTOs used INTERNALLY to deserialize the JSON blob returned by the
    // Langflow 3-stage clinical report pipeline.
    // These are NOT exposed via the API — they are mapped into SessionReport.
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>Root object returned by the Langflow report flow.</summary>
    public sealed class ReportAiOutputDto
    {
        [JsonPropertyName("soap")]
        public ReportSoapDto Soap { get; set; } = new();

        [JsonPropertyName("riskAssessment")]
        public ReportRiskAssessmentDto RiskAssessment { get; set; } = new();

        [JsonPropertyName("meta")]
        public ReportMetaDto? Meta { get; set; }

        [JsonPropertyName("chiefComplaint")]
        public ReportChiefComplaintDto? ChiefComplaint { get; set; }

        [JsonPropertyName("clinicalFormulation")]
        public ReportClinicalFormulationDto? ClinicalFormulation { get; set; }

        [JsonPropertyName("extractionWarnings")]
        public List<string>? ExtractionWarnings { get; set; }
    }

    /// <summary>SOAP note section extracted by the AI.</summary>
    public sealed class ReportSoapDto
    {
        [JsonPropertyName("subjective")]
        public string Subjective { get; set; } = string.Empty;

        [JsonPropertyName("objective")]
        public string Objective { get; set; } = string.Empty;

        [JsonPropertyName("assessment")]
        public string Assessment { get; set; } = string.Empty;

        [JsonPropertyName("plan")]
        public string Plan { get; set; } = string.Empty;

        [JsonPropertyName("pharmacologicalNote")]
        public string? PharmacologicalNote { get; set; }
    }

    /// <summary>Risk assessment section extracted by the AI.</summary>
    public sealed class ReportRiskAssessmentDto
    {
        /// <summary>Values: LOW | MEDIUM | HIGH | CRITICAL</summary>
        [JsonPropertyName("overallRiskTier")]
        public string OverallRiskTier { get; set; } = "LOW";

        [JsonPropertyName("suicidalIdeationPresent")]
        public bool SuicidalIdeationPresent { get; set; }

        [JsonPropertyName("suicidalIdeationDetails")]
        public string? SuicidalIdeationDetails { get; set; }

        [JsonPropertyName("riskNarrative")]
        public string? RiskNarrative { get; set; }
    }

    /// <summary>
    /// Chief complaint section — extracted from the AI pipeline.
    /// Contains the patient's primary concern and episode duration.
    /// </summary>
    public sealed class ReportChiefComplaintDto
    {
        [JsonPropertyName("primaryConcern")]
        public string? PrimaryConcern { get; set; }

        [JsonPropertyName("duration")]
        public string? Duration { get; set; }

        [JsonPropertyName("episodeType")]
        public string? EpisodeType { get; set; }

        [JsonPropertyName("secondaryComplaints")]
        public string? SecondaryComplaints { get; set; }
    }

    /// <summary>
    /// Clinical formulation section — contains differential diagnosis suggestions.
    /// </summary>
    public sealed class ReportClinicalFormulationDto
    {
        [JsonPropertyName("formulationNarrative")]
        public string? FormulationNarrative { get; set; }

        [JsonPropertyName("differentialConsiderations")]
        public List<string>? DifferentialConsiderations { get; set; }
    }

    /// <summary>Optional metadata block returned by the AI.</summary>
    public sealed class ReportMetaDto
    {
        [JsonPropertyName("languages")]
        public string? Languages { get; set; }

        [JsonPropertyName("transcriptQuality")]
        public string? TranscriptQuality { get; set; }

        [JsonPropertyName("diarization")]
        public string? Diarization { get; set; }
    }
}
