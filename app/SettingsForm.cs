namespace TrackSpender;

public sealed class SettingsForm : Form
{
    private readonly NumericUpDown _budget = new() { DecimalPlaces = 2, Maximum = 1_000_000_000, ThousandsSeparator = true, Width = 140 };
    private readonly CheckBox _remind = new() { Text = "Remind me to log spending at", AutoSize = true };
    private readonly NumericUpDown _hour = new() { Minimum = 0, Maximum = 23, Width = 50 };

    public SettingsForm(AppSettings s)
    {
        Text = "Settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        _budget.Value = s.MonthlyBudget;
        _remind.Checked = s.ReminderEnabled;
        _hour.Value = s.ReminderHour;

        var save = new Button { Text = "Save", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        AcceptButton = save;
        CancelButton = cancel;

        var layout = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false };
        layout.Controls.Add(new Label { Text = "Monthly budget (0 = no alerts)", AutoSize = true });
        layout.Controls.Add(_budget);
        layout.Controls.Add(new Label { Text = " ", AutoSize = true });
        var row = new FlowLayoutPanel { AutoSize = true };
        row.Controls.AddRange(new Control[] { _remind, _hour, new Label { Text = ":00", AutoSize = true, Margin = new Padding(0, 6, 0, 0) } });
        layout.Controls.Add(row);
        layout.Controls.Add(new Label { Text = " ", AutoSize = true });
        var buttons = new FlowLayoutPanel { AutoSize = true };
        buttons.Controls.AddRange(new Control[] { save, cancel });
        layout.Controls.Add(buttons);
        Controls.Add(layout);

        FormClosing += (_, e) =>
        {
            if (DialogResult != DialogResult.OK) return;
            s.MonthlyBudget = _budget.Value;
            s.ReminderEnabled = _remind.Checked;
            s.ReminderHour = (int)_hour.Value;
            s.Save();
        };
    }
}
