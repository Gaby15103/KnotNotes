using Domain.Entities;
using UglyToad.PdfPig;

namespace Application.Services;

public class PdfIngestionService
{
    public MarkdownNote ConvertPdfToMarkdown(string pdfPath, string outputMarkdownDir)
    {
        string fileName = Path.GetFileNameWithoutExtension(pdfPath);
        string assetsDir = Path.Combine(outputMarkdownDir, "assets");
        string mdFilePath = Path.Combine(outputMarkdownDir, $"{fileName}.md");

        Directory.CreateDirectory(assetsDir);

        var textBuilder = new System.Text.StringBuilder();
        int imageCount = 0;

        using (var document = PdfDocument.Open(pdfPath))
        {
            foreach (var page in document.GetPages())
            {
                textBuilder.AppendLine($"## Page {page.Number}");
                textBuilder.AppendLine(page.Text);
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
        var markdownTemplate = $"""
                                # Course Document: {fileName}
                                * **Source PDF:** [[pdf:{Path.GetFileName(pdfPath)}|Open Original]]
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
}