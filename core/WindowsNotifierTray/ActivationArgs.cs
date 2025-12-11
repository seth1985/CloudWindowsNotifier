using System;
using System.Collections.Generic;

namespace WindowsNotifierTray;

internal sealed class ActivationArgs
{
    public string Action { get; }
    public string ModuleId { get; }
    public string? Url { get; }
    public string? Tag { get; }
    public string? Group { get; }
    public string Raw { get; }

    private ActivationArgs(string action, string moduleId, string? url, string? tag, string? group, string raw)
    {
        Action = action;
        ModuleId = moduleId;
        Url = url;
        Tag = tag;
        Group = group;
        Raw = raw;
    }

    public static ActivationArgs? Parse(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return null;
        }

        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var separators = arguments.Contains('&') ? new[] { '&' } : new[] { ';' };
        foreach (var part in arguments.Split(separators, StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2)
            {
                dict[kv[0].Trim()] = Uri.UnescapeDataString(kv[1].Trim());
            }
        }

        if (!dict.TryGetValue("action", out var action))
        {
            return null;
        }

        dict.TryGetValue("module", out var module);
        dict.TryGetValue("url", out var url);
        dict.TryGetValue("tag", out var tag);
        dict.TryGetValue("group", out var group);

        return new ActivationArgs(action, module ?? string.Empty, url, tag, group, arguments);
    }
}
