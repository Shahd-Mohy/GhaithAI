using GhaithAI.API.GaithAI.Application.Constants;
using GhaithAI.API.GaithAI.Application.DTOs.Report;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.RegularExpressions;

namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    public sealed class SessionReportPdfDocument : IDocument
    {
        // ── Design tokens ─────────────────────────────────────────────────────
        private static readonly string FontFamily = "Arial";
        private const float BodySize = 10f;
        
        private static readonly string NavyHex = "#1B3A5C";
        private static readonly string SecondaryBlue = "#2C4C74";
        private static readonly string CardBgHex = "#F8FAFC";
        private static readonly string BorderHex = "#E2E8F0";

        private readonly PdfReportDataDto _data;
        private readonly bool _isDraft;

        public SessionReportPdfDocument(PdfReportDataDto data)
        {
            _data = data;
            _isDraft = data.Status is not "Locked" and not "Approved";
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40, Unit.Point);
                page.DefaultTextStyle(x => x.FontFamily(FontFamily).FontSize(BodySize).FontColor("#1E293B"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);

                if (_isDraft)
                    page.Foreground()
                        .AlignCenter()
                        .AlignMiddle()
                        .Text("DRAFT")
                        .FontSize(110)
                        .Bold()
                        .FontColor("#F1F5F9");
            });
        }

        // ── PAGE HEADER ───────────────────────────────────────────────────────
        private void ComposeHeader(IContainer c)
        {
            c.PaddingBottom(15).BorderBottom(1).BorderColor(NavyHex).PaddingBottom(5).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("GHAITHAI").SemiBold().FontSize(12).FontColor(NavyHex).LetterSpacing(0.05f);
                    col.Item().Text("Clinical Session Report").Bold().FontSize(18).FontColor(NavyHex);
                });
                
                row.AutoItem().AlignRight().Column(col =>
                {
                    if (_isDraft)
                    {
                        col.Item().AlignRight().Background(SecondaryBlue).PaddingVertical(3).PaddingHorizontal(8)
                            .Text("DRAFT · PENDING APPROVAL").Bold().FontSize(8).FontColor(Colors.White);
                    }
                    else
                    {
                        col.Item().AlignRight().Background("#15803D").PaddingVertical(3).PaddingHorizontal(8)
                            .Text("APPROVED").Bold().FontSize(8).FontColor(Colors.White);
                    }
                    col.Item().PaddingTop(4).AlignRight().Text($"Session Date: {_data.SessionDate:dd MMM yyyy}").FontSize(9).FontColor("#64748B");
                    col.Item().AlignRight().Text($"Report ID: {_data.ReportId.ToString()[..8].ToUpper()}").FontSize(9).FontColor("#64748B");
                });
            });
        }

        // ── PAGE FOOTER ───────────────────────────────────────────────────────
        private void ComposeFooter(IContainer c)
        {
            c.PaddingTop(10).BorderTop(1f).BorderColor(BorderHex)
                .Row(row =>
                {
                    row.RelativeItem().Text("CONFIDENTIAL · AI-Assisted Clinical Decision-Support Document. Clinician verification required.")
                        .FontSize(7.5f).FontColor("#64748B").Bold();
                    row.AutoItem().AlignRight()
                        .Text(x =>
                        {
                            x.Span("Page ").FontSize(8).FontColor("#64748B");
                            x.CurrentPageNumber().FontSize(8).FontColor("#64748B");
                            x.Span(" of ").FontSize(8).FontColor("#64748B");
                            x.TotalPages().FontSize(8).FontColor("#64748B");
                        });
                });
        }

        // ── MAIN CONTENT ──────────────────────────────────────────────────────
        private void ComposeContent(IContainer c)
        {
            c.Column(col =>
            {
                col.Spacing(12);

                // Section 1: Demographics & Baseline (NEW)
                col.Item().PaddingTop(5).Element(ComposePatientBaseline);

                // Section 2: Clinician Details
                col.Item().Element(ComposeClinicianDetails);

                // Section 3: Risk Assessment (High Priority - Moved UP)
                col.Item().Element(ComposeRiskAssessment);

                // Section 4: SOAP Note (Moved UP for quick review)
                col.Item().Element(ComposeSoap);

                // Section 5: Chief Complaint
                if (!string.IsNullOrWhiteSpace(_data.ChiefComplaintPrimary))
                    col.Item().Element(ComposeChiefComplaint);

                // Section 6: HPI (History of Presenting Illness)
                if (!string.IsNullOrWhiteSpace(_data.HpiNarrative))
                    col.Item().Element(ComposeHpi);

                // Section 7: MSE
                if (!string.IsNullOrWhiteSpace(_data.MseSpeechAndMood))
                    col.Item().Element(ComposeMse);

                // Section 8: Clinical Formulation
                if (!string.IsNullOrWhiteSpace(_data.FormulationNarrative) || _data.DifferentialConsiderations?.Count > 0)
                    col.Item().Element(ComposeFormulation);

                // Section 9: Legal Disclaimer (Moved to end of document)
                col.Item().PaddingTop(20).Element(ComposeLegalDisclaimerBox);
            });
        }

        // ── SECTIONS ─────────────────────────────────────────────────────────

        private void ComposePatientBaseline(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "PATIENT DEMOGRAPHICS & BASELINE");
                
                col.Item().Background(CardBgHex).Border(1).BorderColor(BorderHex).Padding(10).Row(row =>
                {
                    // Left Column: Identity
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().PaddingBottom(2).Text("IDENTITY").Bold().FontSize(8).FontColor(NavyHex);
                        left.Item().Text($"Name: {_data.PatientFullName}").FontSize(9);
                        left.Item().Text($"Gender: {_data.PatientGender}").FontSize(9);
                        left.Item().Text($"Age: {_data.PatientAge?.ToString() ?? "—"}").FontSize(9);
                    });

                    // Middle Column: Intake Baseline
                    row.RelativeItem().Column(mid =>
                    {
                        mid.Item().PaddingBottom(2).Text("INTAKE BASELINE").Bold().FontSize(8).FontColor(NavyHex);
                        mid.Item().Text($"Stress Level: {_data.PatientStressLevel ?? "—"}").FontSize(9);
                        mid.Item().Text($"Sleep Quality: {_data.PatientSleepQuality ?? "—"}").FontSize(9);
                        
                        var therapyStr = _data.PatientHasTherapyHistory ? "Yes" : "No";
                        var medsStr = _data.PatientTakesMedication ? "Yes" : "No";
                        mid.Item().Text($"Prior Therapy: {therapyStr} | Current Meds: {medsStr}").FontSize(9);
                    });

                    // Right Column: Intake Concerns
                    row.RelativeItem().Column(right =>
                    {
                        right.Item().PaddingBottom(2).Text("PRIMARY INTAKE CONCERN").Bold().FontSize(8).FontColor(NavyHex);
                        right.Item().Text(_data.PatientConcerns ?? "—").FontSize(9).Italic().FontColor("#475569");
                    });
                });
            });
        }

        private void ComposeClinicianDetails(IContainer c)
        {
            c.Background(CardBgHex).Border(1).BorderColor(BorderHex).Padding(10).Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.Span("ATTENDING CLINICIAN: ").Bold().FontSize(8).FontColor(NavyHex);
                    t.Span($"Dr. {_data.DoctorFullName}").FontSize(9);
                });
                row.RelativeItem().AlignRight().Text(t =>
                {
                    t.Span("SPECIALTY: ").Bold().FontSize(8).FontColor(NavyHex);
                    t.Span(string.IsNullOrWhiteSpace(_data.DoctorSpecialization) ? "Psychiatrist" : _data.DoctorSpecialization).FontSize(9);
                });
            });
        }

        private void ComposeRiskAssessment(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "RISK ASSESSMENT");
                
                var riskColor = _data.RiskTier.ToUpperInvariant() switch
                {
                    "CRITICAL" => "#B91C1C",
                    "HIGH" => "#C2410C",
                    "MEDIUM" => "#D97706",
                    _ => "#15803D"
                };

                col.Item().Border(1).BorderColor(riskColor).Padding(10).Column(inner =>
                {
                    inner.Item().Row(row =>
                    {
                        row.RelativeItem().AlignMiddle().Text("OVERALL RISK TIER").Bold().FontSize(10).FontColor(riskColor);
                        row.AutoItem().Background(riskColor).PaddingHorizontal(10).PaddingVertical(4)
                            .Text(_data.RiskTier.ToUpperInvariant()).Bold().FontSize(12).FontColor(Colors.White);
                    });

                    inner.Item().PaddingTop(10).BorderTop(0.5f).BorderColor(BorderHex).PaddingTop(10);

                    SubHeader(inner, "SUICIDAL IDEATION");
                    var siBadgeColor = _data.SiPresent ? "#FEF2F2" : "#F0FDF4";
                    var siTextColor = _data.SiPresent ? "#B91C1C" : "#15803D";
                    
                    inner.Item().PaddingBottom(6).Background(siBadgeColor).Border(1).BorderColor(siTextColor)
                       .PaddingHorizontal(8).PaddingVertical(2).Text(_data.SiPresent ? "IDENTIFIED" : "NONE IDENTIFIED")
                       .Bold().FontSize(9).FontColor(siTextColor);

                    if (!string.IsNullOrWhiteSpace(_data.SuicidalIdeationDetails))
                    {
                        inner.Item().PaddingBottom(12).Background("#FEF2F2").BorderLeft(3).BorderColor("#B91C1C")
                           .Padding(8).Element(ct => RenderTextWithTags(ct, _data.SuicidalIdeationDetails));
                    }

                    if (!string.IsNullOrWhiteSpace(_data.RiskNarrative))
                    {
                        SubHeader(inner, "RISK NARRATIVE & PROTECTIVE FACTORS");
                        inner.Item().Element(ct => RenderTextWithTags(ct, _data.RiskNarrative));
                    }
                });
            });
        }

        private void ComposeSoap(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "CLINICAL SUMMARY (SOAP)");
                col.Item().Border(1).BorderColor(BorderHex).Padding(10).Column(inner =>
                {
                    SoapSection(inner, "S · SUBJECTIVE", _data.SoapSubjective);
                    SoapSection(inner, "O · OBJECTIVE", _data.SoapObjective);
                    SoapSection(inner, "A · ASSESSMENT", _data.SoapAssessment);
                    SoapSection(inner, "P · PLAN", _data.SoapPlan);
                });
            });
        }

        private static void SoapSection(ColumnDescriptor col, string title, string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            col.Item().PaddingTop(4).Background(SecondaryBlue).PaddingHorizontal(8).PaddingVertical(3)
               .Text(title).Bold().FontSize(8).FontColor(Colors.White);
            col.Item().PaddingTop(4).PaddingBottom(12).Element(ct => RenderTextWithTags(ct, text));
        }

        private void ComposeChiefComplaint(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "CHIEF COMPLAINT");
                col.Item().Border(1).BorderColor(BorderHex).Padding(10).Column(inner =>
                {
                    SubHeader(inner, "PRIMARY CONCERN");
                    inner.Item().PaddingBottom(8).Element(ct => RenderTextWithTags(ct, _data.ChiefComplaintPrimary));

                    SubHeader(inner, "DURATION");
                    inner.Item().PaddingBottom(8).Element(ct => RenderTextWithTags(ct, _data.ChiefComplaintDuration));

                    SubHeader(inner, "EPISODE TYPE");
                    inner.Item().PaddingBottom(8).Element(ct => RenderTextWithTags(ct, _data.ChiefComplaintEpisodeType));

                    if (!string.IsNullOrWhiteSpace(_data.ChiefComplaintSecondary))
                    {
                        SubHeader(inner, "SECONDARY COMPLAINTS");
                        inner.Item().PaddingBottom(8).Element(ct => RenderTextWithTags(ct, _data.ChiefComplaintSecondary));
                    }
                });
            });
        }

        private void ComposeHpi(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "HISTORY OF PRESENTING ILLNESS (HPI)");
                col.Item().Border(1).BorderColor(BorderHex).Padding(10).Column(inner =>
                {
                    SubHeader(inner, "HPI NARRATIVE");
                    inner.Item().PaddingBottom(12).Element(ct => RenderTextWithTags(ct, _data.HpiNarrative));

                    SubHeader(inner, "FUNCTIONAL IMPACT");
                    inner.Item().PaddingBottom(12).Element(ct => RenderFunctionalImpactGrid(ct, _data.HpiFunctionalImpact));

                    if (!string.IsNullOrWhiteSpace(_data.HpiPastHistory))
                    {
                        SubHeader(inner, "PAST PSYCHIATRIC & MEDICAL HISTORY");
                        inner.Item().PaddingBottom(8).Element(ct => RenderTextWithTags(ct, _data.HpiPastHistory));
                    }

                    if (!string.IsNullOrWhiteSpace(_data.HpiCurrentMedications))
                    {
                        SubHeader(inner, "CURRENT MEDICATIONS & SUBSTANCES");
                        inner.Item().PaddingBottom(8).Element(ct => RenderTextWithTags(ct, _data.HpiCurrentMedications));
                    }
                });
            });
        }

        private void RenderFunctionalImpactGrid(IContainer c, string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            var sentences = Regex.Split(text, @"(?<=[\.!\?])\s+");
            var dict = new Dictionary<string, string>();

            foreach (var s in sentences)
            {
                if (string.IsNullOrWhiteSpace(s)) continue;
                var lower = s.ToLowerInvariant();
                if (lower.Contains("work") || lower.Contains("code") || lower.Contains("manager") || lower.Contains("academic") || lower.Contains("job"))
                    AppendToDict(dict, "WORK / ACADEMIC", s);
                else if (lower.Contains("sleep") || lower.Contains("bed") || lower.Contains("wake") || lower.Contains("night"))
                    AppendToDict(dict, "SLEEP", s);
                else if (lower.Contains("appetite") || lower.Contains("weight") || lower.Contains("eat"))
                    AppendToDict(dict, "APPETITE", s);
                else if (lower.Contains("energy") || lower.Contains("fatigue") || lower.Contains("gym") || lower.Contains("tired"))
                    AppendToDict(dict, "ENERGY / PSYCHOMOTOR", s);
                else if (lower.Contains("interpersonal") || lower.Contains("fianc") || lower.Contains("family") || lower.Contains("social") || lower.Contains("friend"))
                    AppendToDict(dict, "INTERPERSONAL", s);
                else if (lower.Contains("self-care") || lower.Contains("hygiene") || lower.Contains("groom") || lower.Contains("bath"))
                    AppendToDict(dict, "SELF-CARE", s);
                else if (lower.Contains("cognition") || lower.Contains("focus") || lower.Contains("brain") || lower.Contains("concentrat") || lower.Contains("impaired"))
                    AppendToDict(dict, "COGNITION", s);
                else
                    AppendToDict(dict, "OTHER", s);
            }

            c.Grid(grid =>
            {
                grid.Columns(2);
                grid.Spacing(8);

                foreach (var kvp in dict)
                {
                    grid.Item().Background(CardBgHex).Border(0.5f).BorderColor(BorderHex).Padding(8).Column(col =>
                    {
                        col.Item().PaddingBottom(4).Text(kvp.Key).Bold().FontSize(8).FontColor(NavyHex);
                        RenderTextWithTags(col.Item(), kvp.Value);
                    });
                }
            });
        }

        private static void AppendToDict(Dictionary<string, string> dict, string key, string value)
        {
            if (dict.ContainsKey(key)) dict[key] += " " + value.Trim();
            else dict[key] = value.Trim();
        }

        private void ComposeMse(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "MENTAL STATE EXAMINATION (MSE)");
                col.Item().Border(1).BorderColor(BorderHex).Padding(10).Column(inner =>
                {
                    MseSection(inner, "APPEARANCE & BEHAVIOR", _data.MseAppearance);
                    MseSection(inner, "SPEECH, MOOD & AFFECT", _data.MseSpeechAndMood);
                    MseSection(inner, "THOUGHT PROCESS & CONTENT", _data.MseThoughtProcess);
                    MseSection(inner, "PERCEPTION & COGNITION", _data.MsePerception);
                    MseSection(inner, "INSIGHT & JUDGEMENT", _data.MseInsightAndJudgement);
                });
            });
        }

        private static void MseSection(ColumnDescriptor col, string title, string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            SubHeader(col, title);
            col.Item().PaddingBottom(10).Element(ct => RenderTextWithTags(ct, text));
        }

        private void ComposeFormulation(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "CLINICAL FORMULATION");
                col.Item().Border(1).BorderColor(BorderHex).Padding(10).Column(inner =>
                {
                    if (!string.IsNullOrWhiteSpace(_data.FormulationNarrative))
                    {
                        SubHeader(inner, "FORMULATION NARRATIVE");
                        inner.Item().PaddingBottom(10).Element(ct => RenderTextWithTags(ct, _data.FormulationNarrative));
                    }

                    if (_data.DifferentialConsiderations?.Count > 0)
                    {
                        SubHeader(inner, "DIFFERENTIAL CONSIDERATIONS");
                        inner.Item().PaddingBottom(8).Column(list =>
                        {
                            int i = 1;
                            foreach (var item in _data.DifferentialConsiderations)
                            {
                                list.Item().Row(r =>
                                {
                                    r.ConstantItem(15).Text($"{i}.").FontSize(BodySize).FontColor("#475569");
                                    r.RelativeItem().Element(ct => RenderTextWithTags(ct, item));
                                });
                                i++;
                            }
                        });
                    }
                });
            });
        }

        private void ComposeLegalDisclaimerBox(IContainer c)
        {
            c.Background("#FFFBEB").Border(1.5f).BorderColor("#D97706").Padding(12).Column(warn =>
            {
                warn.Item().Text("⚠ AI-GENERATED CLINICAL DECISION-SUPPORT DOCUMENT")
                    .Bold().FontColor("#B45309").FontSize(11);
                warn.Item().PaddingTop(4).Text("MANDATORY CLINICIAN REVIEW REQUIRED BEFORE ANY CLINICAL USE")
                    .Bold().FontColor("#B45309").FontSize(10);
                warn.Item().PaddingTop(8).Text(SessionReportConstants.ComplianceHeaderDisclaimer)
                    .FontSize(8.5f).FontColor("#78350F");
                
                if (!string.IsNullOrWhiteSpace(_data.PharmacologicalNote))
                {
                    warn.Item().PaddingTop(8).Text("Pharmacological Note:")
                        .Bold().FontSize(8.5f).FontColor("#78350F");
                    warn.Item().PaddingTop(2).Text(_data.PharmacologicalNote)
                        .FontSize(8.5f).FontColor("#78350F");
                }
            });
        }

        // ── UTILITIES ────────────────────────────────────────────────────────

        private static void SectionHeader(IContainer c, string title)
        {
            c.Background(NavyHex).Padding(6).Row(row =>
            {
                row.RelativeItem().AlignMiddle().Text(title).Bold().FontSize(11).FontColor(Colors.White);
                row.AutoItem().AlignMiddle().Background("#3A5A80").PaddingVertical(2).PaddingHorizontal(6)
                   .Text("AI-ASSISTED CONTENT")
                   .FontSize(7).FontColor(Colors.White);
            });
        }

        private static void SubHeader(ColumnDescriptor c, string title)
        {
            c.Item().PaddingBottom(4).Text(title).Bold().FontSize(9).FontColor(NavyHex);
        }

        private static void RenderTextWithTags(IContainer container, string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            
            container.Text(t =>
            {
                // Parse tags like [STATED], [INFERRED...], [VERBATIM...]
                var matches = Regex.Matches(text, @"\[(STATED|INFERRED[^\]]*|VERBATIM[^\]]*)\]");
                int lastIndex = 0;
                
                foreach (Match match in matches)
                {
                    if (match.Index > lastIndex)
                    {
                        t.Span(text.Substring(lastIndex, match.Index - lastIndex)).FontSize(BodySize);
                    }
                    
                    var tagText = match.Value.Trim('[', ']');
                    if (tagText.StartsWith("STATED")) {
                        t.Span(" " + tagText + " ").BackgroundColor("#E2E8F0").FontColor("#334155").FontSize(7.5f).Bold();
                    } else if (tagText.StartsWith("VERBATIM")) {
                        t.Span(" " + tagText + " ").BackgroundColor("#FFEDD5").FontColor("#C2410C").FontSize(7.5f).Bold();
                    } else if (tagText.StartsWith("INFERRED")) {
                        t.Span(" " + tagText + " ").BackgroundColor("#F3E8FF").FontColor("#7E22CE").FontSize(7.5f).Bold();
                    } else {
                        t.Span(match.Value).FontSize(BodySize);
                    }
                    
                    lastIndex = match.Index + match.Length;
                }
                
                if (lastIndex < text.Length)
                {
                    t.Span(text.Substring(lastIndex)).FontSize(BodySize);
                }
            });
        }
    }
}
