using MpdApply.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MpdApply.Services;

public static class GenericPdfGenerator
{
    public static byte[] Generate(FormTemplate template, FormSubmission submission, string? logoBase64, string agencyName)
    {
        var values = submission.Values;
        var schema = template.Schema;

        byte[]? logoBytes = null;
        if (!string.IsNullOrWhiteSpace(logoBase64))
        {
            try
            {
                var raw = logoBase64.Contains(',') ? logoBase64[(logoBase64.IndexOf(',') + 1)..] : logoBase64;
                logoBytes = Convert.FromBase64String(raw);
            }
            catch { }
        }

        // Decode drawn initials if present
        byte[]? initialsImg = null;
        if (values.TryGetValue("InitialsMode", out var mode) && mode == "drawn"
            && values.TryGetValue("InitialsImageData", out var imgData) && !string.IsNullOrWhiteSpace(imgData))
        {
            try
            {
                var raw = imgData.Contains(',') ? imgData[(imgData.IndexOf(',') + 1)..] : imgData;
                initialsImg = Convert.FromBase64String(raw);
            }
            catch { }
        }

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(0.75f, Unit.Inch);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(h =>
                {
                    h.Item().Row(r =>
                    {
                        if (logoBytes != null)
                        {
                            r.ConstantItem(60).PaddingRight(10).AlignMiddle()
                                .Image(logoBytes).FitArea();
                        }
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().AlignCenter().Text(agencyName).FontSize(13).Bold();
                            c.Item().AlignCenter().Text(template.Name).FontSize(11).Bold();
                        });
                    });
                    h.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });

                page.Content().PaddingTop(12).Column(col =>
                {
                    foreach (var pg in schema.Pages)
                    {
                        if (schema.Pages.Count > 1 && pg != schema.Pages.First())
                        {
                            col.Item().PaddingTop(8).PaddingBottom(4)
                                .BorderBottom(1).BorderColor(Colors.Blue.Darken3)
                                .Text(pg.Title).FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                        }

                        foreach (var field in pg.Fields)
                        {
                            values.TryGetValue(field.Key, out var val);

                            switch (field.Type)
                            {
                                case "section_header":
                                    col.Item().PaddingTop(8).PaddingBottom(2)
                                        .BorderBottom(0.5f).BorderColor(Colors.Blue.Darken3)
                                        .Text(field.Label ?? "").FontSize(9).Bold().FontColor(Colors.Blue.Darken3);
                                    break;

                                case "paragraph":
                                    var pdfPara = System.Text.RegularExpressions.Regex.Replace(
                                        field.Content ?? "",
                                        @"\[([^\]]+)\]\((https?://[^\)]+)\)",
                                        "$1 ($2)");
                                    col.Item().PaddingTop(4).Text(pdfPara)
                                        .FontSize(8).FontColor(Colors.Grey.Darken2).Italic();
                                    break;

                                case "initials_panel":
                                case "initials_reminder":
                                    break; // handled via individual initials fields

                                case "initials":
                                    col.Item().PaddingTop(2).Row(r =>
                                    {
                                        var box = r.ConstantItem(40).Border(0.5f)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .AlignCenter().AlignMiddle();
                                        if (initialsImg != null)
                                            box.Height(20).Image(initialsImg).FitArea();
                                        else
                                            box.Text(val ?? "").Bold().FontSize(8);
                                        r.RelativeItem().PaddingLeft(6).Text(field.Label ?? "").FontSize(8);
                                    });
                                    break;

                                case "signature":
                                    if (!string.IsNullOrWhiteSpace(val) && val != "data:,")
                                    {
                                        try
                                        {
                                            var raw = val.Contains(',') ? val[(val.IndexOf(',') + 1)..] : val;
                                            var sigBytes = Convert.FromBase64String(raw);
                                            col.Item().PaddingTop(6).Row(r =>
                                            {
                                                r.RelativeItem(2).Column(sc =>
                                                {
                                                    sc.Item().Text(field.Label ?? "Signature:").Bold();
                                                    sc.Item().Height(50).Image(sigBytes).FitArea();
                                                });
                                                r.RelativeItem().Column(dc =>
                                                {
                                                    dc.Item().Text("Date:").Bold();
                                                    dc.Item().Text(submission.SubmittedAt?.ToLocalTime().ToString("MM/dd/yyyy") ?? "");
                                                });
                                            });
                                        }
                                        catch { }
                                    }
                                    else
                                    {
                                        col.Item().PaddingTop(6).Row(r =>
                                        {
                                            r.RelativeItem(2).BorderBottom(0.5f).PaddingBottom(2).Text(field.Label ?? "Signature:").Bold();
                                            r.ConstantItem(8);
                                            r.RelativeItem().BorderBottom(0.5f).PaddingBottom(2).Text("Date:").Bold();
                                        });
                                    }
                                    break;

                                case "yesno":
                                case "radio":
                                    col.Item().PaddingTop(3).Row(r =>
                                    {
                                        r.AutoItem().Text($"{field.Label}: ").Bold();
                                        r.RelativeItem().Text(val ?? "");
                                    });
                                    break;

                                case "textarea":
                                    col.Item().PaddingTop(3).Column(c =>
                                    {
                                        c.Item().Text(field.Label ?? "").Bold();
                                        c.Item().PaddingLeft(8).Text(val ?? "").FontSize(8);
                                    });
                                    break;

                                default: // text, email, tel, date, select, checkbox
                                    col.Item().PaddingTop(3).Row(r =>
                                    {
                                        r.AutoItem().Text($"{field.Label}: ").Bold();
                                        r.RelativeItem().Text(val ?? "");
                                    });
                                    break;
                            }
                        }
                    }

                    col.Item().PaddingTop(12).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(4)
                        .Text($"Submitted: {submission.SubmittedAt?.ToLocalTime():MM/dd/yyyy h:mm tt}   IP: {submission.IpAddress}")
                        .FontSize(7).FontColor(Colors.Grey.Darken1);
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span($"{agencyName} — {template.Name}   ").FontColor(Colors.Grey.Darken1);
                    t.Span("Page ").FontColor(Colors.Grey.Darken1);
                    t.CurrentPageNumber().FontColor(Colors.Grey.Darken1);
                    t.Span(" of ").FontColor(Colors.Grey.Darken1);
                    t.TotalPages().FontColor(Colors.Grey.Darken1);
                });
            });
        }).GeneratePdf();
    }
}
