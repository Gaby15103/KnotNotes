using Domain.Entities;

namespace Application.Services;

public class WorkspaceWatcherService
{
    private FileSystemWatcher? _watcher;
    private readonly KnowledgeGraph _graph;
    private readonly string _rootPath;
    
    public event EventHandler? WorkspaceChanged;

    public WorkspaceWatcherService(KnowledgeGraph graph, string rootPath)
    {
        _rootPath = rootPath;
        _graph = graph;
        InitializeWatcher();
    }

    private void InitializeWatcher()
    {
        if (!Directory.Exists(_rootPath)) return;

        _watcher = new FileSystemWatcher(_rootPath)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            Filter = "*.*",
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime
        };
        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileCreated;
        _watcher.Deleted += OnFileDeleted;
        _watcher.Renamed += OnFileRenamed;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            string content = File.ReadAllText(e.FullPath);
            var note = new MarkdownNote(e.FullPath, content);
            _graph.AddOrUpdateNode(note);
            WorkspaceChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        // Handle newly created files dynamically
        OnFileChanged(sender, e);
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        // Clean up graph nodes if files are deleted externally
        // (Can be expanded based on your dictionary key mapping)
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        // Handle file renames
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}