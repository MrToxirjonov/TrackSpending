# TrackSpender

A small Windows desktop app that tracks what you spend, and shows it by **day, month and year**.
It lives in the system tray and sends Windows notifications: a daily reminder to log your spending,
and a warning as you approach your monthly budget.

Built with three languages, each doing the job it is best at:

| Layer | Language | What it does |
|-------|----------|--------------|
| `native/src/storage.c` | C | Holds the expense list in memory and reads/writes the CSV file |
| `native/src/stats.cpp` | C++ | Totals spending per day, month and year |
| `app/` | C# (.NET 8, WinForms) | Window, tray icon, notifications, settings |

The C and C++ code compile into one DLL (`trackspender_core.dll`) that the C# app calls through P/Invoke.

## Features

- Add an expense with a date, amount, category and note
- View totals and a list by **Day**, **Month** or **Year**, with a bar chart (days of a month, months of a year)
- Windows notifications:
  - evening reminder if you haven't logged anything today
  - alerts at 80% and 100% of your monthly budget
- Closes to the tray so reminders keep working (right-click the tray icon, then Quit)
- Your data is a plain CSV at `%APPDATA%\TrackSpender\expenses.csv`: no account, no cloud

## Build

You need Windows 10/11, [Visual Studio Build Tools](https://visualstudio.microsoft.com/downloads/) with the
"Desktop development with C++" workload, [CMake](https://cmake.org/download/) and the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bat
build.bat
```

This builds the native core, runs its tests, and publishes a self-contained `dist\TrackSpender.exe`
(plus `trackspender_core.dll`). Keep both files together.

## Project layout

```
native/
  include/trackspender_core.h   public C API shared by C, C++ and C#
  src/storage.c                 C: storage
  src/stats.cpp                 C++: statistics
  tests/test_core.c             native tests (run by ctest)
  CMakeLists.txt
app/
  TrackSpender.csproj
  MainForm.cs  BarChart.cs  Notifier.cs  SettingsForm.cs  Settings.cs  NativeCore.cs  Program.cs
build.bat
```

## Data format

One expense per line: `YYYY-MM-DD,amount,category,note`

## License

MIT
