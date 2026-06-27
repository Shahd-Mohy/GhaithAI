namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    /// <summary>
    /// Provides the GhaithAI report letterhead logo as PNG bytes.
    /// Used by both the persisted-report PDF path and the preview endpoint
    /// so the logo renders consistently in every generated PDF.
    /// </summary>
    public interface IReportLogoProvider
    {
        /// <summary>
        /// The logo image bytes, or null when the asset is missing or unreadable
        /// (the report is still generated without a logo in that case).
        /// </summary>
        byte[]? LogoBytes { get; }
    }
}
