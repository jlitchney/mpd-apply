using System.Text;
using System.Text.Json;

namespace MpdApply.Services;

public class ClaudeService(IConfiguration config, ILogger<ClaudeService> logger)
{
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string Model = "claude-sonnet-4-6";

    private const string SystemPrompt = """
        You are a form schema generator. Given the text of a PDF form, create a JSON schema for a digital web form.

        The schema format is:
        {
          "pages": [
            {
              "title": "Page Title",
              "intro": "Optional introductory paragraph shown above the fields",
              "fields": [ ... ]
            }
          ]
        }

        Field object format:
        { "key": "snake_case_key", "type": "...", "label": "...", "content": "...", "required": false,
          "options": [], "width": "full", "rows": 3, "placeholder": "", "helpText": "" }

        Field types:
        - text          single line text
        - textarea      multi-line text (set rows for height)
        - date          date picker
        - email         email address
        - tel           phone number
        - select        dropdown — populate options array
        - radio         radio buttons — populate options array
        - yesno         Yes / No radio buttons
        - checkbox      single checkbox
        - signature     canvas signature pad
        - initials      individual initial-stamp button — "label" = the acknowledgement text
        - initials_panel  initials setup panel (type OR draw); place once at top of first page containing initials fields
        - initials_reminder  compact reminder bar; use on subsequent pages that also contain initials fields
        - section_header  visual divider — use "label" for title
        - paragraph     read-only text — use "content" for the text (use \n for line breaks)

        Rules:
        - snake_case keys, unique per form
        - Split naturally multi-page or multi-section forms into separate pages entries
        - Preserve all legal/disclaimer text as paragraph fields
        - Preserve all section headings as section_header fields
        - Signature lines → signature type
        - Initial/check boxes for acknowledgements → initials type
        - If initials fields exist, include initials_panel at the top of the first page that has them
        - If a later page also has initials, start that page with initials_reminder
        - Yes/No questions → yesno type
        - width is one of: full, half, third, quarter — infer from field size in the original
        - Return ONLY valid JSON, no markdown, no explanation
        """;

    public async Task<string?> ConvertPdfTextToSchema(string pdfText)
    {
        var apiKey = config["Claude:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("Claude:ApiKey not configured — cannot convert PDF");
            return null;
        }

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("x-api-key", apiKey);
        http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var body = JsonSerializer.Serialize(new
        {
            model = Model,
            max_tokens = 8000,
            system = SystemPrompt,
            messages = new[]
            {
                new { role = "user", content = $"Convert this PDF form text into the JSON schema:\n\n{pdfText}" }
            }
        });

        try
        {
            var response = await http.PostAsync(ApiUrl,
                new StringContent(body, Encoding.UTF8, "application/json"));
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Claude API error {Status}: {Body}", response.StatusCode, json);
                return null;
            }

            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? "";

            // Strip markdown fences if present
            text = text.Trim();
            if (text.StartsWith("```")) text = text[text.IndexOf('\n')..].TrimStart();
            if (text.EndsWith("```")) text = text[..text.LastIndexOf("```")].TrimEnd();

            return text.Trim();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Claude API call failed");
            return null;
        }
    }
}
