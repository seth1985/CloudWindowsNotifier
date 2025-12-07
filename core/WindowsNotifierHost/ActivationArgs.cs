using System;
using System.Collections.Generic;

namespace WindowsNotifierHost;

internal sealed class ActivationArgs
{
    public string Action { get; }
    public string ModuleId { get; }
    public string? Url { get; }

    private ActivationArgs(string action, string moduleId, string? url)
    {
        Action = action;
        ModuleId = moduleId;
        Url = url;
    }

    public static ActivationArgs? Parse(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return null;
        }

        // Expected format from toast: "action=complete;module=<id>" or "action=link;url=<url>"
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in arguments.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2)
            {
                dict[kv[0].Trim()] = kv[1].Trim();
            }
        }

        if (!dict.TryGetValue("action", out var action))
        {
            return null;
        }

        dict.TryGetValue("module", out var moduleId);
        dict.TryGetValue("url", out var url);

        return new ActivationArgs(action, moduleId ?? string.Empty, url);
    }
}

