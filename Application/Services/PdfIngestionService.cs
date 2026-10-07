using Domain.Entities;
using UglyToad.PdfPig;
using System.Text.RegularExpressions;

namespace Application.Services;

public class PdfIngestionService
{
    public MarkdownNote ConvertPdfToMarkdown(string pdfPath, string outputMarkdownDir)
    {
        string fileName = Path.GetFileNameWithoutExtension(pdfPath).Replace(" ","_");
        string assetsDir = Path.Combine(outputMarkdownDir, "assets");
        string mdFilePath = Path.Combine(outputMarkdownDir, $".KnotNotes/{fileName}.md");

        Directory.CreateDirectory(assetsDir);

        var textBuilder = new System.Text.StringBuilder();
        int imageCount = 0;

        using (var document = PdfDocument.Open(pdfPath))
        {
            foreach (var page in document.GetPages())
            {
                textBuilder.AppendLine($"## Page {page.Number}");
                
                var words = page.GetWords();
                var lines = words
                    .GroupBy(w => Math.Round(w.BoundingBox.Bottom / 4.0) * 4.0)
                    .OrderByDescending(g => g.Key);

                foreach (var line in lines)
                {
                    var sortedWords = line.OrderBy(w => w.BoundingBox.Left).ToList();
                    if (IsTableRow(sortedWords))
                    {
                        string tableRowText = FormatTableRow(sortedWords);
                        textBuilder.AppendLine(tableRowText);
                        continue;
                    }
                    string lineText = string.Join(" ", sortedWords.Select(w => w.Text));
                    
                    if (string.IsNullOrEmpty(lineText)) continue;

                    if (Regex.IsMatch(lineText, @"^\d+(\.\d+)*\s+[A-ZÀ-Ü]"))
                    {
                        textBuilder.AppendLine();
                        string anchor = Regex.Replace(lineText.ToLower(), @"[^\w\s]", "").Replace(" ", "-");
                        textBuilder.AppendLine($"### {lineText} {{#{anchor}}}");
                        textBuilder.AppendLine();
                    }
                    else if (Regex.IsMatch(lineText, @"^\d+$") && lineText.Length <= 3)
                    {
                        continue;
                    }
                    else if (lineText.StartsWith("-") || lineText.StartsWith("•") || Regex.IsMatch(lineText, @"^\d+\)"))
                    {
                        string cleaned = lineText.TrimStart('-', '•', ' ').Trim();
                        textBuilder.AppendLine($"- {cleaned}");
                    }
                    else
                    {
                        textBuilder.AppendLine(lineText);
                    }
                }
                textBuilder.AppendLine();
                
                foreach (var image in page.GetImages())
                {
                    if (image.TryGetBytesAsMemory(out var imageBytes))
                    {
                        imageCount++;
                        string extension = "jpg";
                        var bytesArray = imageBytes.ToArray();
                        
                        if (bytesArray.Length > 4 && bytesArray[0] == 137 && bytesArray[1] == 80 && bytesArray[2] == 78 && bytesArray[3] == 71)
                        {
                            extension = "png";
                        }

                        string imageName = $"{fileName}_p{page.Number}_img{imageCount}.{extension}";
                        string imagePath = Path.Combine(assetsDir, imageName);

                        File.WriteAllBytes(imagePath, bytesArray);
                        textBuilder.AppendLine($"![Figure {imageCount}](assets/{imageName})");
                    }
                }
            }
        }
        string relativePath = Path.GetRelativePath(Path.GetDirectoryName(mdFilePath), pdfPath);
        var markdownTemplate = $"""
                                # Course Document: {fileName}
                                * **Source PDF:** [Open Original]({relativePath})
                                * **Imported:** {DateTime.UtcNow:yyyy-MM-dd}

                                ---

                                ## Extracted Content & Notes
                                {textBuilder}

                                ## Notes & Formulas
                                <!-- Add your manual LaTeX formulas or summary notes here -->

                                ## Related Topics
                                [[Core Concepts]]
                                """;
        
        File.WriteAllText(mdFilePath, markdownTemplate);
        
        return new MarkdownNote(mdFilePath, markdownTemplate);
    }
    private bool IsTableRow(List<UglyToad.PdfPig.Content.Word> words)
    {
        if (words.Count < 3) return false;
        int largeGaps = 0;
        for (int i = 0; i < words.Count - 1; i++)
        {
            double gap = words[i + 1].BoundingBox.Left - words[i].BoundingBox.Right;
            if (gap > 40.0)
            {
                largeGaps++;
            }
        }
        return largeGaps >= 1;
    }

    private string FormatTableRow(List<UglyToad.PdfPig.Content.Word> words)
    {
        var columns = new List<string>();
        var currentColumn = new List<string>();

        for (int i = 0; i < words.Count; i++)
        {
            currentColumn.Add(words[i].Text);
            if (i < words.Count - 1)
            {
                double gap = words[i + 1].BoundingBox.Left - words[i].BoundingBox.Right;
                if (gap > 40.0)
                {
                    columns.Add(string.Join(" ", currentColumn));
                    currentColumn.Clear();
                }
            }
        }
        columns.Add(string.Join(" ", currentColumn));

        return "| " + string.Join(" | ", columns) + " |";
    }
}