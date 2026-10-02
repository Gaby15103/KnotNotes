using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Desktop.ViewModels;

namespace Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnBrowseFolderClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select KnotNotes Courses Workspace",
            AllowMultiple = false,
        });

        if (folders.Count > 0)
        {
            var folder = folders[0];
            string? localPath = folder.TryGetLocalPath();
            if (!string.IsNullOrEmpty(localPath) && DataContext is MainViewModel vm)
            {
                vm.RootDirectoryPath = localPath;

                if (vm.LoadWorkspaceCommand.CanExecute(null))
                {
                    vm.LoadWorkspaceCommand.Execute(null);
                }
            }
        }
    }
}