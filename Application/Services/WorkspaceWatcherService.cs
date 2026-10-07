using Application.Models;
using Domain.Entities;

namespace Application.Services;

public class WorkspaceWatcherService
{
    private readonly List<FileSystemWatcher> _watchers = new();
    private readonly KnowledgeGraph _graph;
    private readonly AppConfig _config;
    
    public event EventHandler? WorkspaceChanged;
    public WorkspaceWatcherService(KnowledgeGraph graph, AppConfig config)
    {
        _graph = graph;
        _config = config;
        InitializeWatcher();
    }

    private void InitializeWatcher()
    {
        if (!_config.EnableLiveWatcher) return;
        
        foreach (var category in _config.Categories)
        {
            if (!Directory.Exists(category.Path))
            {
                continue; 
            }
            
            var watcher = new FileSystemWatcher(category.Path)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                Filter = "*.*",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime
            };

            watcher.Changed += OnFileChanged;
            watcher.Created += OnFileCreated;
            watcher.Deleted += OnFileDeleted;
            watcher.Renamed += OnFileRenamed;
            
            _watchers.Add(watcher);
        }
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
        foreach (var watcher in _watchers)
        {
            watcher.Dispose();
        }
        _watchers.Clear();
    }
}