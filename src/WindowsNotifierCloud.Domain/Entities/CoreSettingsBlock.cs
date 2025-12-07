namespace WindowsNotifierCloud.Domain.Entities;

public class CoreSettingsBlock
{
    public int Enabled { get; set; } = 1;
    public int PollingIntervalSeconds { get; set; } = 300;
    public int AutoClearModules { get; set; } = 1;
    public int SoundEnabled { get; set; } = 1;
    public int ExitMenuVisible { get; set; } = 0;
    public int StartStopMenuVisible { get; set; } = 0;
    public int HeartbeatSeconds { get; set; } = 15;
}
