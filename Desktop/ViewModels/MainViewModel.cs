using System.Collections.ObjectModel;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;

namespace Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly WorkspaceScanner _scanner = new();
    private KnowledgeGraph _graph = new();
    
    [ObservableProperty]
    private string _rootDirectoryPath = string.Empty;
    
    [ObservableProperty]
    private IDocumentNode? _selectedNote;

    public ObservableCollection<IDocumentNode> AllNotes { get; } = new();

    [RelayCommand]
    private void LoadWorkspace()
    {
        if (string.IsNullOrWhiteSpace(_rootDirectoryPath))return;

        _graph = _scanner.ScanDirectory(RootDirectoryPath);
        
        AllNotes.Clear();
        foreach (var node in _graph.GetAllNodes)
        {
            AllNotes.Add(node);
        }
    }
}