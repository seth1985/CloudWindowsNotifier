using System;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.InteropServices;

namespace WindowsNotifierTray;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        try { SetCurrentProcessExplicitAppUserModelID("WindowsNotifier"); } catch { }

        ApplicationConfiguration.Initialize();
        var form = new TrayForm();
        form.StartCoreLoop();
        Application.Run(form);
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string AppID);
}
