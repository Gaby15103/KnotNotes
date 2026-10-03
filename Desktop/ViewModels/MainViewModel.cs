using System.Collections.ObjectModel;
using System.IO;
using Application.Models;
using Application.Services;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;

namespace Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly WorkspaceScanner _scanner = new();
    private readonly ConfigManager _configManager = new();
    private WorkspaceWatcherService? _watcherService;
    private KnowledgeGraph _graph = new();
    
    [ObservableProperty]
    private bool _isSettingsOpen;
    
    [ObservableProperty]
    private WorkspaceCategory? _selectedCategory;
    
    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    [ObservableProperty]
    private string _newCategoryPath = string.Empty;
    
    [ObservableProperty]
    private IDocumentNode? _selectedNote;
    
    [ObservableProperty]
    private bool _enableLiveWatcher = true;

    [ObservableProperty]
    private bool _extractImagesFromPdfs = true;
    
    public ObservableCollection<IDocumentNode> AllNotes { get; } = new();
    public ObservableCollection<WorkspaceCategory> Categories { get; } = new();

    public MainViewModel()
    {
        LoadConfiguration();
        LoadAllWorkspaces();
    }

    [RelayCommand]
    private void LoadConfiguration()
    {
        var config = _configManager.LoadConfig();
        Categories.Clear();
        foreach (var cat in config.Categories)
        {
            Categories.Add(cat);
        }
        
        EnableLiveWatcher = config.EnableLiveWatcher;
        ExtractImagesFromPdfs = config.ExtractImagesFromPdfs;
    }
    
    partial void OnEnableLiveWatcherChanged(bool value) => SavePreferences();
    partial void OnExtractImagesFromPdfsChanged(bool value) => SavePreferences();
    
    private void SavePreferences()
    {
        var config = _configManager.LoadConfig();
        config.EnableLiveWatcher = EnableLiveWatcher;
        config.ExtractImagesFromPdfs = ExtractImagesFromPdfs;
        _configManager.SaveConfig(config);
    }

    [RelayCommand]
    private void LoadAllWorkspaces()
    {
        _graph = new KnowledgeGraph();
        
        foreach (var cat in Categories)
        {
            if (!string.IsNullOrEmpty(cat.Path) && Directory.Exists(cat.Path))
            {
                var partialGraph = _scanner.ScanDirectory(cat.Path);
                foreach (var node in partialGraph.GetAllNodes)
                {
                    _graph.AddOrUpdateNode(node);
                }
            }
        }

        RefreshNoteList();
    }
    
    [RelayCommand]
    private void ToggleSettings()
    {
        IsSettingsOpen = !IsSettingsOpen;
        if (!IsSettingsOpen)
        {
            LoadAllWorkspaces();
        }
    }

    [RelayCommand]
    private void AddCategory(string path)
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName) || string.IsNullOrWhiteSpace(path)) return;

        var config = _configManager.LoadConfig();
        config.Categories.Add(new WorkspaceCategory { Name = NewCategoryName.Trim(), Path = path });
        _configManager.SaveConfig(config);

        NewCategoryName = string.Empty;
        NewCategoryPath = string.Empty;
        LoadConfiguration();
    }

    [RelayCommand]
    private void DeleteCategory(WorkspaceCategory category)
    {
        var config = _configManager.LoadConfig();
        config.Categories.RemoveAll(c => c.Name == category.Name && c.Path == category.Path);
        _configManager.SaveConfig(config);

        LoadConfiguration();
    }
    
    private void RefreshNoteList()
    {
        AllNotes.Clear();
        foreach (var node in _graph.GetAllNodes)
        {
            AllNotes.Add(node);
        }
    }

    public void Dispose()
    {
        _watcherService?.Dispose();
    }
}