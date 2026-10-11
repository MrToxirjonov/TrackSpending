using System.Globalization;

namespace TrackSpender;

/// <summary>Tiny key=value settings file stored next to the expense data.</summary>
public sealed class AppSettings
{
    private static string FilePath => Path.Combine(Core.DataFolder, "settings.ini");

    public decimal MonthlyBudget { get; set; }      // 0 = no budget alerts
    public bool ReminderEnabled { get; set; } = true;
    public int ReminderHour { get; set; } = 20;     // 24h clock

    public static AppSettings Load()
    {
        var s = new AppSettings();
        try
        {
            foreach (var line in File.ReadAllLines(FilePath))
            {
                var parts = line.Split('=', 2);
                if (parts.Length != 2) continue;
                var value = parts[1].Trim();
                switch (parts[0].Trim())
                {
                    case "MonthlyBudget": decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var b); s.MonthlyBudget = b; break;
                    case "ReminderEnabled": s.ReminderEnabled = value == "1"; break;
                    case "ReminderHour": if (int.TryParse(value, out var h)) s.ReminderHour = Math.Clamp(h, 0, 23); break;
                }
            }
        }
        catch (IOException) { /* first run */ }
        return s;
    }

    public void Save()
    {
        Directory.CreateDirectory(Core.DataFolder);
        File.WriteAllLines(FilePath, new[]
        {
            "MonthlyBudget=" + MonthlyBudget.ToString(CultureInfo.InvariantCulture),
            "ReminderEnabled=" + (ReminderEnabled ? "1" : "0"),
            "ReminderHour=" + ReminderHour,
        });
    }
}
