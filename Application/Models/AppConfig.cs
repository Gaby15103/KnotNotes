namespace Application.Models;

public class WorkspaceCategory
{
    public string Name { get; set; } = string.Empty;
    public string Path  { get; set; } = string.Empty;
}

public class AppConfig
{
    public List<WorkspaceCategory> Categories { get; set; } = new();
    
    public bool EnableLiveWatcher { get; set; } = true;
    public bool ExtractImagesFromPdfs { get; set; } = true;
    public string ThemeMode { get; set; } = "Dark";
}