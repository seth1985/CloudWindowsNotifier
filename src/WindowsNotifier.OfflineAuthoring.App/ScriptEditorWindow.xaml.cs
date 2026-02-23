using System.Windows;

namespace WindowsNotifier.OfflineAuthoring.App;

public partial class ScriptEditorWindow : Window
{
    public ScriptEditorWindow(string title, string? initialText)
    {
        InitializeComponent();
        Title = title;
        ScriptTextBox.Text = initialText ?? string.Empty;
    }

    public string ScriptText => ScriptTextBox.Text;

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
