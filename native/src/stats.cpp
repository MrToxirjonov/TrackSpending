// TrackSpender core - statistics layer (C++).
// Reads the expense list owned by storage.c and sums it per day, month and year.
#include "trackspender_core.h"
#include <algorithm>

namespace {
template <typename Match>
double sum_where(Match match) {
    double total = 0;
    const int n = ts_count();
    for (int i = 0; i < n; ++i) {
        int y, m, d;
        double amount;
        if (ts_get(i, &y, &m, &d, &amount, nullptr, 0, nullptr, 0) == 0 && match(y, m, d))
            total += amount;
    }
    return total;
}

int days_in_month(int year, int month) {
    static const int days[] = {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
    const bool leap = (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
    return (month == 2 && leap) ? 29 : days[month - 1];
}
}  // namespace

extern "C" {

TS_API double ts_total_day(int year, int month, int day) {
    return sum_where([=](int y, int m, int d) { return y == year && m == month && d == day; });
}

TS_API double ts_total_month(int year, int month) {
    return sum_where([=](int y, int m, int) { return y == year && m == month; });
}

TS_API double ts_total_year(int year) {
    return sum_where([=](int y, int, int) { return y == year; });
}

TS_API int ts_daily_totals(int year, int month, double *out, int len) {
    if (!out || month < 1 || month > 12) return 0;
    std::fill(out, out + len, 0.0);
    const int n = ts_count();
    for (int i = 0; i < n; ++i) {
        int y, m, d;
        double amount;
        if (ts_get(i, &y, &m, &d, &amount, nullptr, 0, nullptr, 0) == 0 &&
            y == year && m == month && d >= 1 && d <= len)
            out[d - 1] += amount;
    }
    return days_in_month(year, month);
}

TS_API int ts_monthly_totals(int year, double *out, int len) {
    if (!out) return 0;
    std::fill(out, out + len, 0.0);
    const int n = ts_count();
    for (int i = 0; i < n; ++i) {
        int y, m, d;
        double amount;
        if (ts_get(i, &y, &m, &d, &amount, nullptr, 0, nullptr, 0) == 0 &&
            y == year && m >= 1 && m <= len)
            out[m - 1] += amount;
    }
    return 12;
}

}  // extern "C"
