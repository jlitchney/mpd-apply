namespace MpdApply.Models;

public class FormTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Description { get; set; }
    public string SchemaJson { get; set; } = """{"pages":[{"title":"","fields":[]}]}""";
    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RecipientEmail { get; set; }  // null = use global RecipientEmail setting

    public FormSchemaDef Schema => FormSchemaDef.Parse(SchemaJson);
}
