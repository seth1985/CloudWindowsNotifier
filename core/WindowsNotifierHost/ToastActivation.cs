using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Runtime.InteropServices;

namespace WindowsNotifierHost;

// GUID used for COM activation; must match registry entries we set in installer.
[ClassInterface(ClassInterfaceType.None)]
[ComSourceInterfaces(typeof(INotificationActivationCallback))]
[ComVisible(true)]
[Guid("B6F5E61C-3D8D-4C13-9E72-2B8B7D9274D8")]
public sealed class ToastActivation : NotificationActivator
{
    public override void OnActivated(string arguments, NotificationUserInput userInput, string appUserModelId)
    {
        ActivationHandler.Log($"ToastActivation.OnActivated fired. AUMID='{appUserModelId}', Args='{arguments}'");

        var parsed = ActivationArgs.Parse(arguments);
        if (parsed == null || string.IsNullOrWhiteSpace(parsed.ModuleId))
        {
            ActivationHandler.Log("ToastActivation parse failed or missing module id.");
            return;
        }

        ActivationHandler.Log($"ToastActivation parsed. Action='{parsed.Action}', Module='{parsed.ModuleId}', Url='{parsed.Url ?? ""}'");
        ActivationHandler.ForwardToCore(parsed);
    }
}
