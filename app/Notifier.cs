namespace TrackSpender;

/// <summary>Tray icon plus Windows notifications (daily reminder and budget alerts).</summary>
public sealed class Notifier : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 30_000 };
    private readonly Func<AppSettings> _settings;
    private DateTime _lastReminder = DateTime.MinValue;
    private DateTime _lastBudgetAlertMonth = DateTime.MinValue;
    private int _lastBudgetLevel;

    public Notifier(Func<AppSettings> settings, Action open, Action quit)
    {
        _settings = settings;
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open TrackSpender", null, (_, _) => open());
        menu.Items.Add("Quit", null, (_, _) => quit());

        _icon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "TrackSpender",
            ContextMenuStrip = menu,
            Visible = true,
        };
        _icon.DoubleClick += (_, _) => open();

        _timer.Tick += (_, _) => CheckReminder();
        _timer.Start();
    }

    public void Show(string title, string text, ToolTipIcon kind = ToolTipIcon.Info)
        => _icon.ShowBalloonTip(6000, title, text, kind);

    /// <summary>Evening nudge if nothing has been logged today.</summary>
    private void CheckReminder()
    {
        var s = _settings();
        var now = DateTime.Now;
        if (!s.ReminderEnabled || now.Hour < s.ReminderHour || _lastReminder.Date == now.Date) return;
        _lastReminder = now;
        if (Core.TotalDay(now) > 0) return;
        Show("TrackSpender", "You haven't logged any spending today. Add it now?");
    }

    /// <summary>Warn at 80% and 100% of the monthly budget, once per level per month.</summary>
    public void CheckBudget()
    {
        var budget = (double)_settings().MonthlyBudget;
        if (budget <= 0) return;
        var now = DateTime.Now;
        if (_lastBudgetAlertMonth.Year != now.Year || _lastBudgetAlertMonth.Month != now.Month)
        {
            _lastBudgetAlertMonth = now;
            _lastBudgetLevel = 0;
        }
        double spent = Core.TotalMonth(now);
        int level = spent >= budget ? 2 : spent >= budget * 0.8 ? 1 : 0;
        if (level <= _lastBudgetLevel) return;
        _lastBudgetLevel = level;
        if (level == 2)
            Show("Budget reached", $"You've spent {spent:N2} of your {budget:N2} budget this month.", ToolTipIcon.Warning);
        else
            Show("Almost at your budget", $"You've used {spent / budget:P0} of this month's budget ({spent:N2} of {budget:N2}).", ToolTipIcon.Warning);
    }

    public void Dispose()
    {
        _timer.Dispose();
        _icon.Visible = false;
        _icon.Dispose();
    }
}
