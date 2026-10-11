namespace TrackSpender;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Only one TrackSpender at a time (it lives in the tray).
        using var mutex = new Mutex(true, "TrackSpender.SingleInstance", out bool isFirst);
        if (!isFirst) return;

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
