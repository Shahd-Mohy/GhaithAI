namespace GhaithAI.API.GaithAI.Application.Constants
{
    /// <summary>
    /// Hardcoded legal and compliance texts for the clinical session report PDF.
    /// Sourced from the Langflow Stage 2 System Prompt (CS-001 through CS-004).
    /// These are NOT returned by the AI — the backend injects them into the PDF.
    /// </summary>
    public static class SessionReportConstants
    {
        // ── CS-001: Header Disclaimer ────────────────────────────────────────
        /// <summary>
        /// Shown at the top of page 1, below the report header.
        /// </summary>
        public const string ComplianceHeaderDisclaimer =
            "GhaithAI CLINICAL SUPPORT SYSTEM - NOT A DIRECT MEDICAL DIAGNOSIS. " +
            "THIS AUTO-GENERATED REPORT IS FOR LICENSED CLINICIAN REVIEW ONLY. " +
            "DO NOT DISCLOSE TO PATIENT WITHOUT CLINICAL INTERPRETATION.";

        // ── CS-002: Patient Consent Processing Note ──────────────────────────
        /// <summary>
        /// Confirms the legal basis for AI data processing. Shown below header.
        /// </summary>
        public const string PatientConsentProcessingNote =
            "Patient data processed under explicit consent for AI-assisted " +
            "clinical support, recorded in system logs at the start of the session.";

        // ── CS-003: Red Escalation (HIGH / CRITICAL) ─────────────────────────
        /// <summary>
        /// Injected when RiskTier = HIGH or CRITICAL.
        /// </summary>
        public const string RedEscalationNote =
            "RED ESCALATION: PATIENT IS CLASSIFIED AS HIGH/CRITICAL SUICIDE/SELF-HARM RISK. " +
            "CLINICIAN ACTIONS REQUIRED IMMEDIATELY:\n" +
            "1. Complete safety planning prior to patient departure.\n" +
            "2. Coordinate immediate referral to psychiatric emergency services or " +
            "the Egyptian Mental Health Helpline (16394 / 080-8888332).\n" +
            "3. Document protective measures and emergency contact notification " +
            "in the clinical chart within 2 hours.";

        // ── CS-004: Orange Escalation (MEDIUM) ───────────────────────────────
        /// <summary>
        /// Injected when RiskTier = MEDIUM.
        /// </summary>
        public const string OrangeEscalationNote =
            "ORANGE ESCALATION: PATIENT IS CLASSIFIED AS MEDIUM SUICIDE/SELF-HARM RISK. " +
            "CLINICIAN ACTIONS REQUIRED:\n" +
            "1. Complete collaborative safety plan and review emergency contacts.\n" +
            "2. Schedule close follow-up appointment within 72 hours.\n" +
            "3. Provide patient with crisis contact numbers (16394 / 080-8888332).";

        // ── CS-001: Footer Disclaimer ────────────────────────────────────────
        /// <summary>
        /// Shown in the footer of every page.
        /// </summary>
        public const string ComplianceFooterDisclaimer =
            "This report was compiled by GhaithAI Clinical Support Engine based on " +
            "automated speech-to-text transcript analysis of the recorded session. " +
            "GhaithAI makes no guarantees regarding transcript completeness or " +
            "translation accuracy. The final clinical judgment, diagnosis, and " +
            "treatment plan remain the sole responsibility of the attending licensed " +
            "practitioner. Compiled under Egypt PDPL Law 151/2020 and HIPAA guidelines.";
    }
}
