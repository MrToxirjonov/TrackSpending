#include "trackspender_core.h"
#include <assert.h>
#include <math.h>
#include <stdio.h>

#ifdef _WIN32
#define TEST_FILE L"ts_test.csv"
#else
#define TEST_FILE "/tmp/ts_test.csv"
#endif

int main(void) {
    double daily[31], monthly[12];
    char cat[32], note[128];
    int y, m, d;
    double a;

    assert(ts_add(2026, 10, 11, 12.50, "Food", "lunch, with coffee") == 0);
    assert(ts_add(2026, 10, 11, 7.50, "Transport", "") == 1);
    assert(ts_add(2026, 3, 2, 100, "", "rent") == 2);
    assert(ts_add(2026, 13, 2, 5, "x", "bad month") == -1);
    assert(ts_add(2026, 1, 2, -5, "x", "bad amount") == -1);

    assert(fabs(ts_total_day(2026, 10, 11) - 20.0) < 1e-9);
    assert(fabs(ts_total_month(2026, 10) - 20.0) < 1e-9);
    assert(fabs(ts_total_year(2026) - 120.0) < 1e-9);
    assert(ts_daily_totals(2026, 10, daily, 31) == 31 && fabs(daily[10] - 20.0) < 1e-9);
    assert(ts_monthly_totals(2026, monthly, 12) == 12 && fabs(monthly[2] - 100.0) < 1e-9);

    assert(ts_save(TEST_FILE) == 0);
    assert(ts_remove(0) == 0 && ts_count() == 2);
    assert(ts_load(TEST_FILE) == 3);
    assert(ts_get(2, &y, &m, &d, &a, cat, 32, note, 128) == 0);
    assert(y == 2026 && m == 3 && d == 2 && fabs(a - 100) < 1e-9);
    printf("category=%s note=%s\nall tests passed\n", cat, note);
    return 0;
}
