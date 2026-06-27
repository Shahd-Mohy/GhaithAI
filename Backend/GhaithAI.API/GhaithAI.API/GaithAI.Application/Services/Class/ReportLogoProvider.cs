using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Hosting;

namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    /// <summary>
    /// Loads the GhaithAI report letterhead logo (wwwroot/assets/logo1.png) once
    /// and caches the bytes for the lifetime of the process. Registered as a
    /// singleton so both the report-PDF and preview-PDF paths share one read.
    /// </summary>
    public sealed class ReportLogoProvider : IReportLogoProvider
    {
        private readonly ILogger<ReportLogoProvider> _logger;
        private readonly Lazy<byte[]?> _lazyLogo;

        public ReportLogoProvider(IWebHostEnvironment env, ILogger<ReportLogoProvider> logger)
        {
            _logger = logger;
            _lazyLogo = new Lazy<byte[]?>(() => Load(env.WebRootPath));
        }

        public byte[]? LogoBytes => _lazyLogo.Value;

        private byte[]? Load(string? webRootPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(webRootPath))
                {
                    _logger.LogWarning("WebRootPath is empty; report logo will not be embedded.");
                    return null;
                }

                var path = Path.Combine(webRootPath, "assets", "logo1.png");
                if (!File.Exists(path))
                {
                    _logger.LogWarning("Report logo not found at {Path}; PDF will render without it.", path);
                    return null;
                }

                return File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load the report logo; PDF will render without it.");
                return null;
            }
        }
    }
}
