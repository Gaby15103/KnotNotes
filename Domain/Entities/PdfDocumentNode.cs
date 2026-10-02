namespace Domain.Entities;

public class PdfDocumentNode : IDocumentNode
{
    public string Title => Path.GetFileNameWithoutExtension(FilePath);
    public string FilePath { get; }
    public string ExtractedText { get; private set; }
    public HashSet<string> OutgoingLinks { get; } = new(StringComparer.OrdinalIgnoreCase);

    public PdfDocumentNode(string filePath, string extractedText)
    {
        FilePath = filePath;
        ExtractedText = extractedText;
    }
}