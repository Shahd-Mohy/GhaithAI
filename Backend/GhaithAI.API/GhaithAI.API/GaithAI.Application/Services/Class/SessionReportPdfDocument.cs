using GhaithAI.API.GaithAI.Application.Constants;
using GhaithAI.API.GaithAI.Application.DTOs.Report;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.RegularExpressions;

namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    /// <summary>
    /// QuestPDF document that renders a clean, professional clinical session report.
    /// Decoration is intentionally minimal: one navy accent colour, plain full-width
    /// text, and a single thin rule under each section heading. Colour is reserved
    /// for where it carries clinical meaning (risk tier). The square GhaithAI logo
    /// is embedded as a letterhead mark at the top-left of the page header.
    /// </summary>
    public sealed class SessionReportPdfDocument : IDocument
    {
        // ── Design tokens ─────────────────────────────────────────────────────
        private const string FontFamily = "Arial";
        private const float BodySize = 10f;

        private const string NavyHex = "#1B3A5C";
        private const string BorderHex = "#CBD5E1";
        private const string SlateHex = "#1E293B";
        private const string MutedHex = "#64748B";

        private readonly PdfReportDataDto _data;
        private readonly byte[]? _logoBytes;
        private readonly bool _isDraft;

        public SessionReportPdfDocument(PdfReportDataDto data, byte[]? logoBytes = null)
        {
            _data = data;
            _logoBytes = logoBytes;
            _isDraft = data.Status is not "Locked" and not "Approved";
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40, Unit.Point);
                page.DefaultTextStyle(x => x.FontFamily(FontFamily).FontSize(BodySize).FontColor(SlateHex));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        // ── PAGE HEADER ───────────────────────────────────────────────────────
        private void ComposeHeader(IContainer c)
        {
            c.Column(col =>
            {
                // Letterhead: square logo (left) + title block (center) + status/meta (right).
                col.Item().PaddingBottom(8).Row(row =>
                {
                    if (_logoBytes is { Length: > 0 })
                    {
                        // Square asset → square slot keeps aspect ratio correct.
                        row.AutoItem().PaddingRight(12).Width(46).Height(46)
                           .Image(_logoBytes).FitArea();
                    }

                    row.RelativeItem().AlignMiddle().Column(t =>
                    {
                        t.Item().Text("Clinical Session Report").SemiBold().FontSize(18).FontColor(NavyHex);
                        t.Item().Text("GhaithAI  ·  AI-Assisted Clinical Decision Support")
                               .FontSize(8.5f).FontColor(MutedHex);
                    });

                    row.AutoItem().AlignRight().Column(meta =>
                    {
                        meta.Item().AlignRight().PaddingBottom(2).Text(tag =>
                        {
                            if (_isDraft)
                            {
                                tag.Span("● DRAFT · PENDING APPROVAL").Bold().FontSize(8).FontColor("#B45309");
                            }
                            else
                            {
                                tag.Span("● APPROVED").Bold().FontSize(8).FontColor("#15803D");
                            }
                        });

                        meta.Item().AlignRight()
                            .Text($"Session Date: {_data.SessionDate:dd MMM yyyy}")
                            .FontSize(8.5f).FontColor(MutedHex);
                        meta.Item().AlignRight()
                            .Text($"Report ID: {_data.ReportId.ToString()[..8].ToUpper()}")
                            .FontSize(8.5f).FontColor(MutedHex);
                    });
                });

                // CS-001 + CS-002 compliance banner — sits directly under the letterhead
                // rule on page 1 (content flows, so it does not repeat on later pages).
                col.Item().BorderTop(1).BorderColor(NavyHex).PaddingTop(6).PaddingBottom(2)
                   .Text(SessionReportConstants.ComplianceHeaderDisclaimer)
                   .FontSize(7.5f).FontColor(MutedHex);
                col.Item()
                   .Text(SessionReportConstants.PatientConsentProcessingNote)
                   .FontSize(7.5f).FontColor(MutedHex).Italic();
            });
        }

        // ── PAGE FOOTER ───────────────────────────────────────────────────────
        private void ComposeFooter(IContainer c)
        {
            c.PaddingTop(8).BorderTop(1).BorderColor(BorderHex).PaddingTop(6).Row(row =>
            {
                row.RelativeItem()
                  .Text(SessionReportConstants.ComplianceFooterDisclaimer)
                  .FontSize(7f).FontColor(MutedHex);

                row.AutoItem().AlignRight().Text(x =>
                {
                    x.Span("Page ").FontSize(8).FontColor(MutedHex);
                    x.CurrentPageNumber().FontSize(8).FontColor(MutedHex);
                    x.Span(" of ").FontSize(8).FontColor(MutedHex);
                    x.TotalPages().FontSize(8).FontColor(MutedHex);
                });
            });
        }

        // ── MAIN CONTENT ──────────────────────────────────────────────────────
        private void ComposeContent(IContainer c)
        {
            c.Column(col =>
            {
                col.Spacing(14);

                // Section 1: Attending Clinician (organisational context first).
                col.Item().Element(ComposeClinicianDetails);

                // Section 2: Patient Demographics & Baseline.
                col.Item().Element(ComposePatientBaseline);

                // Section 3: Risk Assessment (high priority).
                col.Item().Element(ComposeRiskAssessment);

                // Section 4: SOAP.
                col.Item().Element(ComposeSoap);

                if (!string.IsNullOrWhiteSpace(_data.ChiefComplaintPrimary))
                    col.Item().Element(ComposeChiefComplaint);

                if (!string.IsNullOrWhiteSpace(_data.HpiNarrative))
                    col.Item().Element(ComposeHpi);

                if (!string.IsNullOrWhiteSpace(_data.MseSpeechAndMood))
                    col.Item().Element(ComposeMse);

                if (!string.IsNullOrWhiteSpace(_data.FormulationNarrative)
                    || _data.DifferentialConsiderations?.Count > 0)
                {
                    col.Item().Element(ComposeFormulation);
                }

                if (!string.IsNullOrWhiteSpace(_data.PharmacologicalNote))
                    col.Item().Element(ComposePharmacologicalNote);
            });
        }

        // ── SECTIONS ─────────────────────────────────────────────────────────

        private void ComposeClinicianDetails(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "ATTENDING CLINICIAN");

                col.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Clinician: ").SemiBold().FontColor(NavyHex).FontSize(9.5f);
                        t.Span("Dr. " + _data.DoctorFullName).FontColor(SlateHex).FontSize(9.5f);
                    });
                    row.AutoItem().AlignRight().Text(t =>
                    {
                        t.Span("Specialty: ").SemiBold().FontColor(NavyHex).FontSize(9.5f);
                        t.Span(string.IsNullOrWhiteSpace(_data.DoctorSpecialization)
                                ? "Psychiatrist" : _data.DoctorSpecialization)
                         .FontColor(SlateHex).FontSize(9.5f);
                    });
                });
            });
        }

        private void ComposePatientBaseline(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "PATIENT DEMOGRAPHICS & BASELINE");

                col.Item().PaddingTop(4).PaddingBottom(2).Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        left.Spacing(5);
                        FieldCell(left.Item(), "Name", _data.PatientFullName);
                        FieldCell(left.Item(), "Gender", _data.PatientGender);
                        FieldCell(left.Item(), "Age", _data.PatientAge?.ToString());
                        FieldCell(left.Item(), "Session Type", _data.SessionType);
                    });

                    row.ConstantItem(20);

                    row.RelativeItem().Column(right =>
                    {
                        right.Spacing(5);
                        FieldCell(right.Item(), "Stress Level", _data.PatientStressLevel);
                        FieldCell(right.Item(), "Sleep Quality", _data.PatientSleepQuality);
                        FieldCell(right.Item(), "Prior Therapy", _data.PatientHasTherapyHistory ? "Yes" : "No");
                        FieldCell(right.Item(), "Current Meds", _data.PatientTakesMedication ? "Yes" : "No");
                    });
                });

                if (!string.IsNullOrWhiteSpace(_data.PatientConcerns))
                {
                    col.Item().PaddingTop(4).Text(t =>
                    {
                        t.Span("Primary Intake Concern: ").SemiBold().FontColor(NavyHex).FontSize(9);
                        t.Span(_data.PatientConcerns).FontColor(SlateHex).FontSize(9).Italic();
                    });
                }
            });
        }

        private void ComposeRiskAssessment(IContainer c)
        {
            var tier = _data.RiskTier.ToUpperInvariant();
            var riskColor = tier switch
            {
                "CRITICAL" => "#B91C1C",
                "HIGH" => "#C2410C",
                "MEDIUM" => "#D97706",
                _ => "#15803D"
            };

            // Escalation checklists (CS-003 / CS-004) — injected only when the tier demands it.
            string? escalation = tier switch
            {
                "CRITICAL" or "HIGH" => SessionReportConstants.RedEscalationNote,
                "MEDIUM" => SessionReportConstants.OrangeEscalationNote,
                _ => null
            };

            c.Column(col =>
            {
                SectionHeader(col.Item(), "RISK ASSESSMENT");

                col.Item().PaddingTop(4).Text(t =>
                {
                    t.Span("Overall Risk Tier: ").SemiBold().FontColor(NavyHex).FontSize(9.5f);
                    t.Span(tier).Bold().FontColor(riskColor).FontSize(11);
                });

                col.Item().PaddingTop(2).Text(t =>
                {
                    t.Span("Suicidal Ideation: ").SemiBold().FontColor(NavyHex).FontSize(9);
                    t.Span(_data.SiPresent ? "IDENTIFIED" : "NONE IDENTIFIED")
                     .Bold().FontColor(_data.SiPresent ? "#B91C1C" : "#15803D").FontSize(9);
                });

                if (!string.IsNullOrWhiteSpace(_data.SuicidalIdeationDetails))
                {
                    SubHeader(col, "DETAILS");
                    col.Item().Element(ct => RenderTextWithTags(ct, _data.SuicidalIdeationDetails));
                }

                if (!string.IsNullOrWhiteSpace(_data.RiskNarrative))
                {
                    SubHeader(col, "NARRATIVE & PROTECTIVE FACTORS");
                    col.Item().Element(ct => RenderTextWithTags(ct, _data.RiskNarrative));
                }

                if (escalation != null)
                {
                    SubHeader(col, "REQUIRED CLINICIAN ACTIONS");
                    col.Item().Element(ct => RenderMultiLine(ct, escalation, riskColor));
                }
            });
        }

        private void ComposeSoap(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "CLINICAL SUMMARY (SOAP)");

                col.Item().PaddingTop(4).Column(inner =>
                {
                    inner.Spacing(6);
                    SoapItem(inner, "S — Subjective", _data.SoapSubjective);
                    SoapItem(inner, "O — Objective", _data.SoapObjective);
                    SoapItem(inner, "A — Assessment", _data.SoapAssessment);
                    SoapItem(inner, "P — Plan", _data.SoapPlan);
                });
            });
        }

        private void ComposeChiefComplaint(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "CHIEF COMPLAINT");

                col.Item().PaddingTop(4).Column(inner =>
                {
                    inner.Spacing(6);
                    LabeledBlock(inner, "Primary Concern", _data.ChiefComplaintPrimary);
                    LabeledBlock(inner, "Duration", _data.ChiefComplaintDuration);
                    LabeledBlock(inner, "Episode Type", _data.ChiefComplaintEpisodeType);
                    LabeledBlock(inner, "Secondary Complaints", _data.ChiefComplaintSecondary);
                });
            });
        }

        private void ComposeHpi(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "HISTORY OF PRESENTING ILLNESS (HPI)");

                col.Item().PaddingTop(4).Column(inner =>
                {
                    inner.Spacing(6);
                    LabeledBlock(inner, "Narrative", _data.HpiNarrative);
                    LabeledBlock(inner, "Functional Impact", _data.HpiFunctionalImpact);
                    LabeledBlock(inner, "Past Psychiatric & Medical History", _data.HpiPastHistory);
                    LabeledBlock(inner, "Current Medications & Substances", _data.HpiCurrentMedications);
                });
            });
        }

        private void ComposeMse(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "MENTAL STATE EXAMINATION (MSE)");

                col.Item().PaddingTop(4).Column(inner =>
                {
                    inner.Spacing(6);
                    LabeledBlock(inner, "Appearance & Behavior", _data.MseAppearance);
                    LabeledBlock(inner, "Speech, Mood & Affect", _data.MseSpeechAndMood);
                    LabeledBlock(inner, "Thought Process & Content", _data.MseThoughtProcess);
                    LabeledBlock(inner, "Perception & Cognition", _data.MsePerception);
                    LabeledBlock(inner, "Insight & Judgement", _data.MseInsightAndJudgement);
                });
            });
        }

        private void ComposeFormulation(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "CLINICAL FORMULATION");

                col.Item().PaddingTop(4).Column(inner =>
                {
                    inner.Spacing(6);

                    if (!string.IsNullOrWhiteSpace(_data.FormulationNarrative))
                        LabeledBlock(inner, "Narrative", _data.FormulationNarrative);

                    if (_data.DifferentialConsiderations?.Count > 0)
                    {
                        inner.Item().Text("Differential Considerations").SemiBold().FontSize(9.5f).FontColor(NavyHex);
                        inner.Item().Column(list =>
                        {
                            int i = 1;
                            foreach (var item in _data.DifferentialConsiderations)
                            {
                                list.Item().Row(r =>
                                {
                                    r.ConstantItem(16).Text($"{i}.").FontSize(BodySize).FontColor(MutedHex);
                                    r.RelativeItem().Element(ct => RenderTextWithTags(ct, item));
                                });
                                i++;
                            }
                        });
                    }
                });
            });
        }

        private void ComposePharmacologicalNote(IContainer c)
        {
            c.Column(col =>
            {
                SectionHeader(col.Item(), "PHARMACOLOGICAL NOTE");
                col.Item().PaddingTop(4).Element(ct => RenderTextWithTags(ct, _data.PharmacologicalNote));
            });
        }

        // ── UTILITIES ────────────────────────────────────────────────────────

        private static void SectionHeader(IContainer c, string title)
        {
            // Bold navy title with a thin rule beneath it (3pt gap to rule, 6pt after).
            c.PaddingBottom(6).BorderBottom(1).BorderColor(BorderHex).PaddingBottom(3)
                .Text(title).SemiBold().FontSize(12).FontColor(NavyHex);
        }

        private static void SubHeader(ColumnDescriptor c, string title)
        {
            c.Item().PaddingTop(6).PaddingBottom(2)
                .Text(title).SemiBold().FontSize(9).FontColor(NavyHex);
        }

        private static void SoapItem(ColumnDescriptor col, string title, string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            col.Item().Text(title).SemiBold().FontSize(9.5f).FontColor(NavyHex);
            col.Item().Element(ct => RenderTextWithTags(ct, text));
        }

        private static void LabeledBlock(ColumnDescriptor col, string label, string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            col.Item().Text(label).SemiBold().FontSize(9.5f).FontColor(NavyHex);
            col.Item().Element(ct => RenderTextWithTags(ct, text));
        }

        private static void FieldCell(IContainer c, string label, string? value)
        {
            c.Text(t =>
            {
                t.Span(label + ": ").SemiBold().FontColor(NavyHex).FontSize(9);
                t.Span(string.IsNullOrWhiteSpace(value) ? "—" : value).FontColor(SlateHex).FontSize(9);
            });
        }

        /// <summary>
        /// Renders a string containing '\n' line breaks as separate text lines
        /// (used for the multi-line escalation checklists).
        /// </summary>
        private static void RenderMultiLine(IContainer c, string? text, string color)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            c.Column(col =>
            {
                col.Spacing(1);
                foreach (var raw in text.Split('\n'))
                {
                    var line = raw.Trim();
                    if (line.Length == 0) continue;
                    col.Item().Text(line).FontSize(8.5f).FontColor(color);
                }
            });
        }

        /// <summary>
        /// Renders body text, converting provenance tags ([STATED] / [INFERRED…] /
        /// [VERBATIM…]) into small bold markers. No coloured pill backgrounds —
        /// provenance stays readable but unobtrusive.
        /// </summary>
        private static void RenderTextWithTags(IContainer container, string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            container.Text(t =>
            {
                var matches = Regex.Matches(text, @"\[(STATED|INFERRED[^\]]*|VERBATIM[^\]]*)\]");
                int lastIndex = 0;

                foreach (Match match in matches)
                {
                    if (match.Index > lastIndex)
                        t.Span(text.Substring(lastIndex, match.Index - lastIndex))
                         .FontSize(BodySize).FontColor(SlateHex);

                    var tagText = match.Value.Trim('[', ']');
                    t.Span("[" + tagText + "] ").Bold().FontSize(8f).FontColor(MutedHex);

                    lastIndex = match.Index + match.Length;
                }

                if (lastIndex < text.Length)
                    t.Span(text.Substring(lastIndex)).FontSize(BodySize).FontColor(SlateHex);
            });
        }
    }
}
