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
        var style  = schema.PdfStyle ?? new PdfStyleDef();

        Color accent;
        try   { accent = Color.FromHex(style.AccentColor); }
        catch { accent = Colors.Blue.Darken3; }

        var fs      = (float)Math.Clamp(style.FontSize, 7, 12);
        var gap     = style.Compact ? 2f : 4f;

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
                page.DefaultTextStyle(x => x.FontSize(fs));

                // ── Header ────────────────────────────────────────────────────
                page.Header().Column(h =>
                {
                    h.Item().Row(r =>
                    {
                        if (logoBytes != null)
                            r.ConstantItem(60).PaddingRight(10).AlignMiddle().Image(logoBytes).FitArea();
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().AlignCenter().Text(agencyName).FontSize(fs + 4).Bold();
                            c.Item().AlignCenter().Text(template.Name).FontSize(fs + 2).Bold();
                        });
                    });
                    h.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(accent);
                });

                // ── Content ───────────────────────────────────────────────────
                page.Content().PaddingTop(12).Column(col =>
                {
                    var firstPage = true;
                    foreach (var pg in schema.Pages)
                    {
                        if (schema.Pages.Count > 1 && !firstPage)
                        {
                            col.Item().PaddingTop(10).PaddingBottom(2)
                                .BorderBottom(1).BorderColor(accent)
                                .Text(pg.Title).FontSize(fs + 1).Bold().FontColor(accent);
                        }
                        firstPage = false;

                        foreach (var row in GroupIntoRows(pg.Fields))
                        {
                            if (row.Count == 1)
                            {
                                var f = row[0];
                                col.Item().PaddingTop(gap)
                                    .Element(el => RenderField(el, f, values, style, accent, fs, submission, initialsImg));
                            }
                            else
                            {
                                col.Item().PaddingTop(gap).Row(r =>
                                {
                                    for (int i = 0; i < row.Count; i++)
                                    {
                                        var f = row[i];
                                        var cell = r.RelativeItem(ColWeight(f.Width));
                                        IContainer content = i < row.Count - 1 ? cell.PaddingRight(8) : cell;
                                        RenderInlineField(content, f, values, style, accent, fs);
                                    }
                                });
                            }
                        }
                    }

                    if (style.ShowMeta)
                        col.Item().PaddingTop(14).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(3)
                            .Text($"Submitted: {submission.SubmittedAt?.ToLocalTime():MM/dd/yyyy h:mm tt}   IP: {submission.IpAddress}")
                            .FontSize(7).FontColor(Colors.Grey.Darken1);
                });

                // ── Footer ─────────────────────────────────────────────────────
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

    // ── Row grouping ──────────────────────────────────────────────────────────
    // Groups fields into rows that mirror the Bootstrap grid widths in the web form.

    static int ColWeight(string width) => width switch
    {
        "half"    => 6,
        "third"   => 4,
        "quarter" => 3,
        _         => 12
    };

    static bool IsBlockField(string type) =>
        type is "section_header" or "paragraph" or "initials_panel" or "initials_reminder" or "initials" or "signature";

    static List<List<FormFieldDef>> GroupIntoRows(List<FormFieldDef> fields)
    {
        var rows    = new List<List<FormFieldDef>>();
        var current = new List<FormFieldDef>();
        int cols    = 0;

        foreach (var f in fields)
        {
            if (IsBlockField(f.Type) || f.Width == "full")
            {
                if (current.Any()) { rows.Add(current); current = []; cols = 0; }
                rows.Add([f]);
            }
            else
            {
                int w = ColWeight(f.Width);
                if (cols + w > 12 && current.Any()) { rows.Add(current); current = []; cols = 0; }
                current.Add(f);
                cols += w;
            }
        }
        if (current.Any()) rows.Add(current);
        return rows;
    }

    // ── Block field (full-width or layout-only) ───────────────────────────────

    static void RenderField(IContainer el, FormFieldDef f, Dictionary<string, string> values,
        PdfStyleDef style, Color accent, float fs, FormSubmission submission, byte[]? initialsImg)
    {
        values.TryGetValue(f.Key, out var val); val ??= "";

        switch (f.Type)
        {
            case "section_header":
                el.PaddingTop(2).BorderBottom(0.75f).BorderColor(accent).PaddingBottom(3)
                    .Text(f.Label ?? "").FontSize(fs).Bold().FontColor(accent);
                break;

            case "paragraph":
                var para = System.Text.RegularExpressions.Regex.Replace(
                    f.Content ?? "", @"\[([^\]]+)\]\((https?://[^\)]+)\)", "$1 ($2)");
                el.Text(para).FontSize(fs - 1f).FontColor(Colors.Grey.Darken2).Italic();
                break;

            case "initials_panel":
            case "initials_reminder":
                break;

            case "initials":
                el.Row(r =>
                {
                    var box = r.ConstantItem(44).Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                        .AlignCenter().AlignMiddle();
                    if (initialsImg != null)
                        box.Height(22).Image(initialsImg).FitArea();
                    else
                        box.Text(val).Bold().FontSize(fs);
                    r.RelativeItem().PaddingLeft(6).Text(f.Label ?? "").FontSize(fs - 1f);
                });
                break;

            case "signature":
                RenderSignature(el, f, val, submission, fs);
                break;

            default:
                RenderInlineField(el, f, values, style, accent, fs);
                break;
        }
    }

    static void RenderSignature(IContainer el, FormFieldDef f, string val, FormSubmission submission, float fs)
    {
        el.Row(r =>
        {
            r.RelativeItem(2).Column(c =>
            {
                c.Item().Text(f.Label ?? "Signature:").FontSize(fs - 1f).Bold().FontColor(Colors.Grey.Darken2);
                if (!string.IsNullOrWhiteSpace(val) && val != "data:,")
                {
                    try
                    {
                        var raw = val.Contains(',') ? val[(val.IndexOf(',') + 1)..] : val;
                        c.Item().Height(50).Image(Convert.FromBase64String(raw)).FitArea();
                    }
                    catch { c.Item().Height(28).BorderBottom(0.75f).BorderColor(Colors.Grey.Medium).Text(""); }
                }
                else
                {
                    c.Item().Height(28).BorderBottom(0.75f).BorderColor(Colors.Grey.Medium).Text("");
                }
            });
            r.ConstantItem(10);
            r.RelativeItem().Column(c =>
            {
                c.Item().Text("Date:").FontSize(fs - 1f).Bold().FontColor(Colors.Grey.Darken2);
                c.Item().BorderBottom(0.75f).BorderColor(Colors.Grey.Medium).PaddingBottom(2)
                    .Text(submission.SubmittedAt?.ToLocalTime().ToString("MM/dd/yyyy") ?? "").FontSize(fs);
            });
        });
    }

    // ── Inline (label + value) field ─────────────────────────────────────────

    static void RenderInlineField(IContainer el, FormFieldDef f, Dictionary<string, string> values,
        PdfStyleDef style, Color accent, float fs)
    {
        values.TryGetValue(f.Key, out var val); val ??= "";
        var labelFs = Math.Max(6f, fs - 1.5f);
        var box     = style.FieldStyle == "box";

        el.Column(c =>
        {
            if (f.Type == "checkbox")
            {
                c.Item().PaddingTop(3).Row(r =>
                {
                    r.ConstantItem(14).AlignMiddle()
                        .Text(val == "true" ? "[X]" : "[ ]").FontSize(fs);
                    r.RelativeItem().Text(f.Label ?? "").FontSize(fs);
                });
                return;
            }

            // Label header (shaded style draws its own label inside each case)
            if (style.FieldStyle != "shaded")
                c.Item().Text(f.Label ?? f.Key).FontSize(labelFs).Bold().FontColor(Colors.Grey.Darken2);

            switch (f.Type)
            {
                case "yesno":
                    if (style.FieldStyle == "shaded")
                        c.Item().Background(Colors.Grey.Lighten4).Padding(2)
                            .Text(f.Label ?? f.Key).FontSize(labelFs).Bold().FontColor(Colors.Grey.Darken2);
                    c.Item().PaddingTop(2).Row(r =>
                    {
                        r.AutoItem().PaddingRight(16)
                            .Text((val == "Yes" ? "(*)" : "( )") + "  Yes").FontSize(fs);
                        r.AutoItem()
                            .Text((val == "No"  ? "(*)" : "( )") + "  No").FontSize(fs);
                    });
                    c.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Text("").FontSize(1);
                    break;

                case "radio":
                    if (style.FieldStyle == "shaded")
                        c.Item().Background(Colors.Grey.Lighten4).Padding(2)
                            .Text(f.Label ?? f.Key).FontSize(labelFs).Bold().FontColor(Colors.Grey.Darken2);
                    c.Item().PaddingTop(2).Column(rc =>
                    {
                        foreach (var opt in f.Options)
                            rc.Item().Text((val == opt ? "(*)" : "( )") + "  " + opt).FontSize(fs);
                    });
                    c.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Text("").FontSize(1);
                    break;

                case "textarea":
                    if (style.FieldStyle == "shaded")
                    {
                        c.Item().Background(Colors.Grey.Lighten4).Padding(2)
                            .Text(f.Label ?? f.Key).FontSize(labelFs).Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().MinHeight(40).Border(0.5f).BorderColor(Colors.Grey.Medium)
                            .Padding(3).Text(val).FontSize(fs);
                    }
                    else if (box)
                        c.Item().MinHeight(40).Border(0.5f).BorderColor(Colors.Grey.Medium)
                            .Padding(3).Text(val).FontSize(fs);
                    else
                        c.Item().MinHeight(40).BorderBottom(0.75f).BorderColor(Colors.Grey.Medium)
                            .PaddingBottom(2).Text(val).FontSize(fs);
                    break;

                default:
                    if (style.FieldStyle == "shaded")
                    {
                        c.Item().Background(Colors.Grey.Lighten4).Padding(2)
                            .Text(f.Label ?? f.Key).FontSize(labelFs).Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().Border(0.5f).BorderColor(Colors.Grey.Medium)
                            .Padding(3).Text(val).FontSize(fs);
                    }
                    else if (box)
                        c.Item().Border(0.5f).BorderColor(Colors.Grey.Medium)
                            .Padding(3).Text(val).FontSize(fs);
                    else
                        c.Item().BorderBottom(0.75f).BorderColor(Colors.Grey.Medium)
                            .PaddingBottom(2).Text(val).FontSize(fs);
                    break;
            }
        });
    }
}
