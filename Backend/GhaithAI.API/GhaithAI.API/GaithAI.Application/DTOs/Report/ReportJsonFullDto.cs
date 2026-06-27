using System.Text.Json.Serialization;

namespace GhaithAI.API.GaithAI.Application.DTOs.Report
{
    // ─────────────────────────────────────────────────────────────────────────────
    // PDF-ONLY deserialization of the full ReportJson stored in SessionReport.
    // Extends the standard ReportAiOutputDto schema with HPI and MSE sections
    // that are NOT exposed via the API but ARE needed for the PDF export.
    // DO NOT use this class in any API response.
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>Root object for PDF-only full deserialization of ReportJson.</summary>
    public sealed class ReportJsonFullDto
    {
        [JsonPropertyName("soap")]
        public ReportSoapDto Soap { get; set; } = new();

        [JsonPropertyName("riskAssessment")]
        public ReportRiskAssessmentDto RiskAssessment { get; set; } = new();

        [JsonPropertyName("meta")]
        public ReportMetaDto? Meta { get; set; }

        [JsonPropertyName("chiefComplaint")]
        public ReportChiefComplaintDto? ChiefComplaint { get; set; }

        [JsonPropertyName("historyOfPresentingIllness")]
        public ReportHpiDto? HistoryOfPresentingIllness { get; set; }

        [JsonPropertyName("mentalStateExam")]
        public ReportMseDto? MentalStateExam { get; set; }

        [JsonPropertyName("clinicalFormulation")]
        public ReportClinicalFormulationDto? ClinicalFormulation { get; set; }

        [JsonPropertyName("extractionWarnings")]
        public List<string>? ExtractionWarnings { get; set; }
    }

    /// <summary>History of Presenting Illness section from Stage 3 JSON.</summary>
    public sealed class ReportHpiDto
    {
        [JsonPropertyName("hpiNarrative")]
        public string? HpiNarrative { get; set; }

        [JsonPropertyName("functionalImpact")]
        public string? FunctionalImpact { get; set; }

        [JsonPropertyName("pastPsychiatricAndMedicalHistory")]
        public string? PastPsychiatricAndMedicalHistory { get; set; }

        [JsonPropertyName("currentMedicationsAndSubstances")]
        public string? CurrentMedicationsAndSubstances { get; set; }
    }

    /// <summary>Mental State Examination section from Stage 3 JSON.</summary>
    public sealed class ReportMseDto
    {
        [JsonPropertyName("appearanceAndBehavior")]
        public string? AppearanceAndBehavior { get; set; }

        [JsonPropertyName("speechAndMoodAffect")]
        public string? SpeechAndMoodAffect { get; set; }

        [JsonPropertyName("thoughtProcessAndContent")]
        public string? ThoughtProcessAndContent { get; set; }

        [JsonPropertyName("perceptionAndCognition")]
        public string? PerceptionAndCognition { get; set; }

        [JsonPropertyName("insightAndJudgement")]
        public string? InsightAndJudgement { get; set; }
    }
}
