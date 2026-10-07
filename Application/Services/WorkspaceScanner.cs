using System.Text.RegularExpressions;
using Domain.Entities;

namespace Application.Services;

public class WorkspaceScanner
{
    private readonly PdfIngestionService _pdfIngestionService = new();

    public KnowledgeGraph ScanDirectory(string rootPath)
    {
        var graph = new KnowledgeGraph();
        if (!Directory.Exists(rootPath)) return graph;

        var markdownFiles = Directory.GetFiles(rootPath, "*.md", SearchOption.AllDirectories);
        foreach (var mdFile in markdownFiles)
        {
            string content = File.ReadAllText(mdFile);
            var note = new MarkdownNote(mdFile, content);
            graph.AddOrUpdateNode(note);
        }

        var pdfFiles = Directory.GetFiles(rootPath, "*.pdf", SearchOption.AllDirectories);
        foreach (var pdfFile in pdfFiles)
        {
            string dir = Path.GetDirectoryName(pdfFile);
            if (!File.Exists($"{dir}/.KnotNotes"))
            {
                Directory.CreateDirectory($"{dir}/.KnotNotes");
            }
            
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(pdfFile).Replace(" ", "_");
            string expectedMdCompanion = Path.Combine(dir, $".KnotNotes/{fileNameWithoutExt}.md");
            
            IDocumentNode node;

            if (!File.Exists(expectedMdCompanion))
            {
                var companionNote = _pdfIngestionService.ConvertPdfToMarkdown(pdfFile, dir);
                node = companionNote;
            }
            else
            {
                node = new PdfDocumentNode(pdfFile, "Indexed via companion note.");
            }
            graph.AddOrUpdateNode(node);
        }
        return graph;
    }
}