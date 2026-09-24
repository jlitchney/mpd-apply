using System.Text.Json;
using System.Text.Json.Serialization;

namespace MpdApply.Models;

public class FormFieldDef
{
    [JsonPropertyName("key")]     public string Key { get; set; } = "";
    [JsonPropertyName("type")]    public string Type { get; set; } = "text";
    // text, textarea, date, email, tel, select, radio, yesno, checkbox
    // signature, initials, initials_panel, initials_reminder
    // section_header, paragraph

    [JsonPropertyName("label")]       public string? Label { get; set; }
    [JsonPropertyName("content")]     public string? Content { get; set; }
    [JsonPropertyName("required")]    public bool Required { get; set; }
    [JsonPropertyName("options")]     public List<string> Options { get; set; } = [];
    [JsonPropertyName("placeholder")] public string? Placeholder { get; set; }
    [JsonPropertyName("helpText")]    public string? HelpText { get; set; }
    [JsonPropertyName("width")]       public string Width { get; set; } = "full";
    [JsonPropertyName("rows")]        public int Rows { get; set; } = 3;
}

public class FormPageDef
{
    [JsonPropertyName("title")]  public string Title { get; set; } = "";
    [JsonPropertyName("intro")]  public string? Intro { get; set; }
    [JsonPropertyName("fields")] public List<FormFieldDef> Fields { get; set; } = [];
}

public class FormSchemaDef
{
    [JsonPropertyName("pages")] public List<FormPageDef> Pages { get; set; } = [];

    public static FormSchemaDef Parse(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<FormSchemaDef>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
        catch { return new(); }
    }
}
