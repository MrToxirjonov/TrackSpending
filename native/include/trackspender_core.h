#ifndef TRACKSPENDER_CORE_H
#define TRACKSPENDER_CORE_H

#ifdef _WIN32
  #include <wchar.h>
  typedef wchar_t ts_char;
  #ifdef TS_BUILD
    #define TS_API __declspec(dllexport)
  #else
    #define TS_API __declspec(dllimport)
  #endif
#else
  typedef char ts_char;
  #define TS_API __attribute__((visibility("default")))
#endif

#ifdef __cplusplus
extern "C" {
#endif

/* ---- storage (C): load, save and edit the expense list ---- */
TS_API int ts_load(const ts_char *path);   /* returns count, or -1 on error (missing file = 0) */
TS_API int ts_save(const ts_char *path);   /* returns 0 on success */
TS_API int ts_add(int year, int month, int day, double amount,
                  const char *category, const char *note); /* returns new index, or -1 */
TS_API int ts_remove(int index);           /* returns 0 on success */
TS_API int ts_count(void);
TS_API int ts_get(int index, int *year, int *month, int *day, double *amount,
                  char *category, int category_len, char *note, int note_len);

/* ---- statistics (C++): totals per day, month and year ---- */
TS_API double ts_total_day(int year, int month, int day);
TS_API double ts_total_month(int year, int month);
TS_API double ts_total_year(int year);
TS_API int ts_daily_totals(int year, int month, double *out, int len);  /* returns days in month */
TS_API int ts_monthly_totals(int year, double *out, int len);           /* returns 12 */

#ifdef __cplusplus
}
#endif
#endif
