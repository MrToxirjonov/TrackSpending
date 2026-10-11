using System.Runtime.InteropServices;
using System.Text;

namespace TrackSpender;

public sealed record Expense(int Index, DateTime Date, double Amount, string Category, string Note);

/// <summary>Thin wrapper over trackspender_core.dll (storage in C, statistics in C++).</summary>
public static class Core
{
    private const string Dll = "trackspender_core";

    [DllImport(Dll, CharSet = CharSet.Unicode)] private static extern int ts_load(string path);
    [DllImport(Dll, CharSet = CharSet.Unicode)] private static extern int ts_save(string path);
    [DllImport(Dll)] private static extern int ts_add(int y, int m, int d, double amount,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string category, [MarshalAs(UnmanagedType.LPUTF8Str)] string note);
    [DllImport(Dll)] private static extern int ts_remove(int index);
    [DllImport(Dll)] private static extern int ts_count();
    [DllImport(Dll)] private static extern int ts_get(int index, out int y, out int m, out int d, out double amount,
        byte[] category, int categoryLen, byte[] note, int noteLen);
    [DllImport(Dll)] private static extern double ts_total_day(int y, int m, int d);
    [DllImport(Dll)] private static extern double ts_total_month(int y, int m);
    [DllImport(Dll)] private static extern double ts_total_year(int y);
    [DllImport(Dll)] private static extern int ts_daily_totals(int y, int m, double[] output, int len);
    [DllImport(Dll)] private static extern int ts_monthly_totals(int y, double[] output, int len);

    public static string DataFolder { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrackSpender");
    public static string DataFile => Path.Combine(DataFolder, "expenses.csv");

    public static void Load()
    {
        Directory.CreateDirectory(DataFolder);
        if (ts_load(DataFile) < 0) throw new IOException("Could not read " + DataFile);
    }

    public static void Save()
    {
        if (ts_save(DataFile) != 0) throw new IOException("Could not write " + DataFile);
    }

    public static void Add(DateTime date, double amount, string category, string note)
    {
        if (ts_add(date.Year, date.Month, date.Day, amount, category, note) < 0)
            throw new ArgumentException("Enter an amount greater than zero.");
        Save();
    }

    public static void Remove(int index)
    {
        if (ts_remove(index) == 0) Save();
    }

    public static List<Expense> Query(Func<DateTime, bool> filter)
    {
        var result = new List<Expense>();
        var cat = new byte[64];
        var note = new byte[256];
        for (int i = 0, n = ts_count(); i < n; i++)
        {
            if (ts_get(i, out int y, out int m, out int d, out double amount, cat, cat.Length, note, note.Length) != 0) continue;
            var date = new DateTime(y, m, Math.Min(d, DateTime.DaysInMonth(y, m)));
            if (filter(date))
                result.Add(new Expense(i, date, amount, Utf8(cat), Utf8(note)));
        }
        return result;
    }

    public static double TotalDay(DateTime d) => ts_total_day(d.Year, d.Month, d.Day);
    public static double TotalMonth(DateTime d) => ts_total_month(d.Year, d.Month);
    public static double TotalYear(DateTime d) => ts_total_year(d.Year);

    public static double[] DailyTotals(DateTime d)
    {
        var buf = new double[31];
        int days = ts_daily_totals(d.Year, d.Month, buf, buf.Length);
        return buf[..days];
    }

    public static double[] MonthlyTotals(DateTime d)
    {
        var buf = new double[12];
        ts_monthly_totals(d.Year, buf, buf.Length);
        return buf;
    }

    private static string Utf8(byte[] buffer)
    {
        int len = Array.IndexOf(buffer, (byte)0);
        return Encoding.UTF8.GetString(buffer, 0, len < 0 ? buffer.Length : len);
    }
}
