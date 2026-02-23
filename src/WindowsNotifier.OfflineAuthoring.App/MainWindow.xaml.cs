using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using Win32 = Microsoft.Win32;
using WindowsNotifier.OfflineAuthoring.App.ViewModels;

namespace WindowsNotifier.OfflineAuthoring.App;

public partial class MainWindow : Window
{
    private static readonly TimeSpan AutosaveInterval = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan RecoveryMaxAge = TimeSpan.FromDays(7);
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;
    private bool _allowClose;
    private readonly DispatcherTimer _autosaveTimer;
    private bool _isRecoveryPromptActive;
    private bool _isClosingPromptActive;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        _autosaveTimer = new DispatcherTimer
        {
            Interval = AutosaveInterval
        };
        _autosaveTimer.Tick += AutosaveTimer_Tick;
    }

    private async void NewProject_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteWithUnsavedCheckAsync(() =>
        {
            ViewModel.NewProject();
            return Task.CompletedTask;
        });
    }

    private async void OpenProject_Click(object sender, RoutedEventArgs e)
    {
        if (!await EnsureCanDiscardUnsavedChangesAsync())
        {
            return;
        }

        var dialog = new Win32.OpenFileDialog
        {
            Filter = "Windows Notifier Project (*.wnproj)|*.wnproj|JSON files (*.json)|*.json|All files (*.*)|*.*",
            InitialDirectory = ViewModel.ProjectsRootFolder,
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == true)
        {
            await ViewModel.LoadProjectAsync(dialog.FileName);
        }
    }

    private async void ImportFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!await EnsureCanDiscardUnsavedChangesAsync())
        {
            return;
        }

        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "Select a module folder containing manifest.json"
        };

        if (dialog.ShowDialog() == Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
        {
            await ViewModel.ImportModuleFolderAsync(dialog.SelectedPath);
        }
    }

    private async void LoadSelected_Click(object sender, RoutedEventArgs e)
    {
        if (!await EnsureCanDiscardUnsavedChangesAsync())
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(ViewModel.SelectedProjectFile))
        {
            await ViewModel.LoadProjectAsync(ViewModel.SelectedProjectFile);
        }
    }

    private async void SaveProject_Click(object sender, RoutedEventArgs e)
    {
        var saved = await ViewModel.SaveAsync();
        if (!saved)
        {
            SaveAsProject_Click(sender, e);
        }
    }

    private async void SaveAsProject_Click(object sender, RoutedEventArgs e)
    {
        var suggestedName = string.IsNullOrWhiteSpace(ViewModel.ModuleId)
            ? "module.wnproj"
            : $"{ViewModel.ModuleId}.wnproj";

        var dialog = new Win32.SaveFileDialog
        {
            Filter = "Windows Notifier Project (*.wnproj)|*.wnproj|JSON files (*.json)|*.json|All files (*.*)|*.*",
            InitialDirectory = ViewModel.ProjectsRootFolder,
            FileName = suggestedName,
            AddExtension = true,
            DefaultExt = ".wnproj"
        };

        if (dialog.ShowDialog(this) == true)
        {
            await ViewModel.SaveAsAsync(dialog.FileName);
        }
    }

    private void RefreshProjects_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RefreshProjectFiles();
    }

    private void BrowseIcon_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Win32.OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.ico)|*.png;*.jpg;*.jpeg;*.ico|All files (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == true)
        {
            ViewModel.SetIconSourcePath(dialog.FileName);
        }
    }

    private void BrowseHero_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Win32.OpenFileDialog
        {
            Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == true)
        {
            ViewModel.SetHeroSourcePath(dialog.FileName);
        }
    }

    private void Validate_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ValidateCurrent();
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ExportAsync();
    }

    private async void Deploy_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.DeployAsync();
    }

    private async void ExportIntune_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ExportIntunePackageAsync();
    }

    private void OpenOutputFolder_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenLastOutputFolder();
    }

    private void ReminderUp_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.IncrementReminderHours();
    }

    private void ReminderDown_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.DecrementReminderHours();
    }

    private void LoadConditionalScript_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Win32.OpenFileDialog
        {
            Filter = "PowerShell scripts (*.ps1)|*.ps1|All files (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == true)
        {
            ViewModel.LoadConditionalScriptFromFile(dialog.FileName);
        }
    }

    private void LoadDynamicScript_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Win32.OpenFileDialog
        {
            Filter = "PowerShell scripts (*.ps1)|*.ps1|All files (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == true)
        {
            ViewModel.LoadDynamicScriptFromFile(dialog.FileName);
        }
    }

    private void OpenConditionalEditor_Click(object sender, RoutedEventArgs e)
    {
        var editor = new ScriptEditorWindow("Conditional Script Editor", ViewModel.ConditionalScriptBody)
        {
            Owner = this
        };

        if (editor.ShowDialog() == true)
        {
            ViewModel.ConditionalScriptBody = editor.ScriptText;
            ViewModel.SelectedType = WindowsNotifier.OfflineAuthoring.Core.Models.OfflineModuleType.Conditional;
        }
    }

    private void OpenDynamicEditor_Click(object sender, RoutedEventArgs e)
    {
        var editor = new ScriptEditorWindow("Dynamic Script Editor", ViewModel.DynamicScriptBody)
        {
            Owner = this
        };

        if (editor.ShowDialog() == true)
        {
            ViewModel.DynamicScriptBody = editor.ScriptText;
            ViewModel.SelectedType = WindowsNotifier.OfflineAuthoring.Core.Models.OfflineModuleType.Dynamic;
        }
    }

    private async void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_allowClose)
        {
            return;
        }

        if (_isClosingPromptActive)
        {
            return;
        }

        e.Cancel = true;
        _isClosingPromptActive = true;
        var canClose = false;
        try
        {
            canClose = await EnsureCanDiscardUnsavedChangesAsync();
        }
        finally
        {
            _isClosingPromptActive = false;
        }

        if (canClose)
        {
            _allowClose = true;
            _ = Dispatcher.BeginInvoke(new Action(Close), DispatcherPriority.Normal);
        }

        e.Cancel = true;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.CleanupStaleRecoverySnapshotsAsync(RecoveryMaxAge);
        await PromptRecoveryAsync();
        _autosaveTimer.Start();
    }

    private async void Window_Deactivated(object sender, EventArgs e)
    {
        await ViewModel.SaveRecoverySnapshotAsync();
    }

    private async void AutosaveTimer_Tick(object? sender, EventArgs e)
    {
        await ViewModel.SaveRecoverySnapshotAsync();
    }

    private async Task ExecuteWithUnsavedCheckAsync(Func<Task> action)
    {
        if (!await EnsureCanDiscardUnsavedChangesAsync())
        {
            return;
        }

        await action();
    }

    private async Task<bool> EnsureCanDiscardUnsavedChangesAsync()
    {
        if (!ViewModel.IsDirty)
        {
            return true;
        }

        var choice = System.Windows.MessageBox.Show(
            "You have unsaved changes. Save before continuing?",
            "Unsaved Changes",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);

        if (choice == MessageBoxResult.Cancel)
        {
            return false;
        }

        if (choice == MessageBoxResult.No)
        {
            return true;
        }

        if (!await ViewModel.SaveAsync())
        {
            var suggestedName = string.IsNullOrWhiteSpace(ViewModel.ModuleId)
                ? "module.wnproj"
                : $"{ViewModel.ModuleId}.wnproj";

            var dialog = new Win32.SaveFileDialog
            {
                Filter = "Windows Notifier Project (*.wnproj)|*.wnproj|JSON files (*.json)|*.json|All files (*.*)|*.*",
                InitialDirectory = ViewModel.ProjectsRootFolder,
                FileName = suggestedName,
                AddExtension = true,
                DefaultExt = ".wnproj"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return false;
            }

            if (!await ViewModel.SaveAsAsync(dialog.FileName))
            {
                return false;
            }
        }

        return true;
    }

    private async Task PromptRecoveryAsync()
    {
        if (_isRecoveryPromptActive)
        {
            return;
        }

        _isRecoveryPromptActive = true;
        try
        {
            var snapshot = await ViewModel.GetRecoverySnapshotInfoAsync();
            if (snapshot == null)
            {
                return;
            }

            var projectPart = string.IsNullOrWhiteSpace(snapshot.ProjectPath)
                ? "unsaved draft"
                : $"project '{snapshot.ProjectPath}'";
            var message =
                $"A recovery snapshot from {snapshot.SavedAtUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss} was found for {projectPart}.{Environment.NewLine}" +
                "Do you want to restore it?";

            var choice = System.Windows.MessageBox.Show(
                message,
                "Recovery Snapshot Found",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (choice == MessageBoxResult.Yes)
            {
                var restored = await ViewModel.RestoreRecoverySnapshotAsync();
                if (!restored)
                {
                    await ViewModel.ClearRecoverySnapshotAsync();
                    System.Windows.MessageBox.Show(
                        "Recovery snapshot could not be restored and was removed.",
                        "Recovery Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            else
            {
                await ViewModel.ClearRecoverySnapshotAsync();
            }
        }
        finally
        {
            _isRecoveryPromptActive = false;
        }
    }

    private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.S)
        {
            SaveAsProject_Click(sender, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
        {
            SaveProject_Click(sender, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        if (e.Key == Key.F5)
        {
            Validate_Click(sender, new RoutedEventArgs());
            e.Handled = true;
            return;
        }
    }
}
