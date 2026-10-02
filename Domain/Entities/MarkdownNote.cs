using System.Text.RegularExpressions;

namespace Domain.Entities;

public class MarkdownNote : IDocumentNode
{
    public string Title { get; private set; }
    public string FilePath { get;  private set; }
    public string Content { get; private set; } = string.Empty;
    public HashSet<string> Tags { get; private set; } = new(StringComparer.OrdinalIgnoreCase); 
    public HashSet<string> OutgoingLinks { get;   private set; }  = new(StringComparer.OrdinalIgnoreCase);
    
    private static readonly Regex WikilinkRegex = new Regex(@"\[\[(.*?)\]\]", RegexOptions.Compiled);
    private static readonly Regex TagRegex = new(@"(?<!\S)#([A-Za-z0-9_-]+)", RegexOptions.Compiled);
    
    public MarkdownNote(string filePath, string initialContent)
    {
        FilePath = filePath;
        Title = Path.GetFileNameWithoutExtension(filePath);
        UpdateContent(initialContent);
    }

    public void UpdateContent(string newContent)
    {
        Content = newContent;
        ParseMetadata();
    }

    private void ParseMetadata()
    {
        OutgoingLinks.Clear();
        Tags.Clear();

        foreach (Match match in WikilinkRegex.Matches(Content))
        {
            if (match.Groups.Count > 1)
            {
                OutgoingLinks.Add(match.Groups[1].Value);
            }
        }

        foreach (Match match in TagRegex.Matches(Content))
        {
            if (match.Groups.Count > 1)
            {
                Tags.Add(match.Groups[1].Value.Trim());
            }
        }
    }
}