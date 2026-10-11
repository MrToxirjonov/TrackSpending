using System.Globalization;

namespace TrackSpender;

public sealed class MainForm : Form
{
    private static readonly string[] Categories = { "Food", "Transport", "Home", "Bills", "Health", "Fun", "Other" };

    private readonly DateTimePicker _addDate = new() { Format = DateTimePickerFormat.Short, Width = 110 };
    private readonly NumericUpDown _amount = new() { DecimalPlaces = 2, Maximum = 1_000_000_000, ThousandsSeparator = true, Width = 110 };
    private readonly ComboBox _category = new() { Width = 110 };
    private readonly TextBox _note = new() { Width = 200, PlaceholderText = "Note (optional)", MaxLength = 120 };

    private readonly ComboBox _scope = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 80 };
    private readonly DateTimePicker _period = new() { Width = 150 };
    private readonly Label _total = new() { AutoSize = true, Font = new Font("Segoe UI Semibold", 12f), Margin = new Padding(16, 3, 0, 0) };

    private readonly ListView _list = new() { View = View.Details, FullRowSelect = true, GridLines = true, MultiSelect = false };
    private readonly BarChart _chart = new() { Dock = DockStyle.Bottom, Height = 170 };

    private readonly AppSettings _settings = AppSettings.Load();
    private readonly Notifier _notifier;
    private bool _quitting;
    private bool _trayHintShown;

    public MainForm()
    {
        Text = "TrackSpender";
        MinimumSize = new Size(760, 520);
        Size = new Size(860, 620);
        Font = new Font("Segoe UI", 9.5f);

        Core.Load();
        _notifier = new Notifier(() => _settings, ShowWindow, Quit);

        _category.Items.AddRange(Categories);
        _category.Text = Categories[0];
        _scope.Items.AddRange(new object[] { "Day", "Month", "Year" });
        _scope.SelectedIndex = 1;

        _list.Dock = DockStyle.Fill;
        _list.Columns.Add("Date", 100);
        _list.Columns.Add("Category", 110);
        _list.Columns.Add("Amount", 110, HorizontalAlignment.Right);
        _list.Columns.Add("Note", 420);

        var addBtn = new Button { Text = "Add expense", AutoSize = true };
        var deleteBtn = new Button { Text = "Delete selected", AutoSize = true };
        var settingsBtn = new Button { Text = "Settings", AutoSize = true };

        var addRow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8, 8, 8, 0) };
        addRow.Controls.AddRange(new Control[] { _addDate, _amount, _category, _note, addBtn });

        var viewRow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
        viewRow.Controls.AddRange(new Control[] { new Label { Text = "View by", AutoSize = true, Margin = new Padding(0, 6, 4, 0) },
            _scope, _period, deleteBtn, settingsBtn, _total });

        // Fill control first, then the docked edges.
        Controls.Add(_list);
        Controls.Add(_chart);
        Controls.Add(viewRow);
        Controls.Add(addRow);

        addBtn.Click += (_, _) => AddExpense();
        deleteBtn.Click += (_, _) => DeleteSelected();
        settingsBtn.Click += (_, _) => { using var f = new SettingsForm(_settings); f.ShowDialog(this); _notifier.CheckBudget(); };
        _scope.SelectedIndexChanged += (_, _) => { ApplyPeriodFormat(); Refresh_(); };
        _period.ValueChanged += (_, _) => Refresh_();
        _list.KeyDown += (_, e) => { if (e.KeyCode == Keys.Delete) DeleteSelected(); };
        _note.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { AddExpense(); e.SuppressKeyPress = true; } };

        ApplyPeriodFormat();
        Refresh_();
    }

    private void ApplyPeriodFormat()
    {
        switch (_scope.SelectedIndex)
        {
            case 0: _period.Format = DateTimePickerFormat.Short; break;
            case 1: _period.Format = DateTimePickerFormat.Custom; _period.CustomFormat = "MMMM yyyy"; break;
            default: _period.Format = DateTimePickerFormat.Custom; _period.CustomFormat = "yyyy"; break;
        }
    }

    private void AddExpense()
    {
        if (_amount.Value <= 0)
        {
            MessageBox.Show(this, "Enter an amount greater than zero.", "TrackSpender", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        try
        {
            Core.Add(_addDate.Value, (double)_amount.Value, _category.Text.Trim(), _note.Text.Trim());
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "TrackSpender", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        _amount.Value = 0;
        _note.Clear();
        _period.Value = _addDate.Value;   // jump to the period you just added to
        Refresh_();
        _notifier.CheckBudget();
        _amount.Focus();
    }

    private void DeleteSelected()
    {
        if (_list.SelectedItems.Count == 0) return;
        var ok = MessageBox.Show(this, "Delete the selected expense?", "TrackSpender",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (ok != DialogResult.Yes) return;
        Core.Remove((int)_list.SelectedItems[0].Tag!);
        Refresh_();
    }

    private void Refresh_()
    {
        var p = _period.Value;
        Func<DateTime, bool> filter;
        double total;
        switch (_scope.SelectedIndex)
        {
            case 0:
                filter = d => d.Date == p.Date;
                total = Core.TotalDay(p);
                _chart.Visible = false;
                break;
            case 1:
                filter = d => d.Year == p.Year && d.Month == p.Month;
                total = Core.TotalMonth(p);
                var days = Core.DailyTotals(p);
                _chart.SetData(days, Enumerable.Range(1, days.Length).Select(i => i.ToString()).ToArray());
                _chart.Visible = true;
                break;
            default:
                filter = d => d.Year == p.Year;
                total = Core.TotalYear(p);
                _chart.SetData(Core.MonthlyTotals(p), CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[..12]);
                _chart.Visible = true;
                break;
        }

        _list.BeginUpdate();
        _list.Items.Clear();
        foreach (var x in Core.Query(filter).OrderByDescending(x => x.Date).ThenByDescending(x => x.Index))
        {
            var item = new ListViewItem(new[] { x.Date.ToString("d"), x.Category, x.Amount.ToString("N2"), x.Note }) { Tag = x.Index };
            _list.Items.Add(item);
        }
        _list.EndUpdate();
        _total.Text = $"Total: {total:N2}";
    }

    private void ShowWindow()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private void Quit()
    {
        _quitting = true;
        Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Closing the window keeps TrackSpender in the tray so reminders still arrive.
        if (!_quitting && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            if (!_trayHintShown)
            {
                _trayHintShown = true;
                _notifier.Show("TrackSpender is still running", "Right-click the tray icon and choose Quit to exit.");
            }
            return;
        }
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _notifier.Dispose();
        base.OnFormClosed(e);
    }
}
