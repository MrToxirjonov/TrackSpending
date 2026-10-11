/* TrackSpender core - storage layer (plain C).
 * Expenses live in memory and are saved as a simple CSV file:
 *   YYYY-MM-DD,amount,category,note
 */
#include "trackspender_core.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define CAT_MAX 32
#define NOTE_MAX 128

typedef struct {
    int year, month, day;
    double amount;
    char category[CAT_MAX];
    char note[NOTE_MAX];
} expense;

static expense *g_items = NULL;
static int g_count = 0;
static int g_cap = 0;

static FILE *open_file(const ts_char *path, int write) {
#ifdef _WIN32
    return _wfopen(path, write ? L"wb" : L"rb");
#else
    return fopen(path, write ? "wb" : "rb");
#endif
}

static void clean(char *dst, size_t size, const char *src) {
    size_t i = 0;
    if (!src) src = "";
    for (; src[i] && i + 1 < size; i++) {
        char c = src[i];
        dst[i] = (c == ',' || c == '\n' || c == '\r') ? ' ' : c;
    }
    dst[i] = '\0';
}

static int valid_date(int y, int m, int d) {
    return y >= 1900 && y <= 9999 && m >= 1 && m <= 12 && d >= 1 && d <= 31;
}

static int push(const expense *e) {
    if (g_count == g_cap) {
        int cap = g_cap ? g_cap * 2 : 64;
        expense *p = (expense *)realloc(g_items, (size_t)cap * sizeof(expense));
        if (!p) return -1;
        g_items = p;
        g_cap = cap;
    }
    g_items[g_count] = *e;
    return g_count++;
}

TS_API int ts_add(int year, int month, int day, double amount,
                  const char *category, const char *note) {
    expense e;
    if (!valid_date(year, month, day) || !(amount > 0)) return -1;
    e.year = year; e.month = month; e.day = day; e.amount = amount;
    clean(e.category, CAT_MAX, category);
    clean(e.note, NOTE_MAX, note);
    if (e.category[0] == '\0') strcpy(e.category, "General");
    return push(&e);
}

TS_API int ts_remove(int index) {
    if (index < 0 || index >= g_count) return -1;
    memmove(&g_items[index], &g_items[index + 1],
            (size_t)(g_count - index - 1) * sizeof(expense));
    g_count--;
    return 0;
}

TS_API int ts_count(void) { return g_count; }

TS_API int ts_get(int index, int *year, int *month, int *day, double *amount,
                  char *category, int category_len, char *note, int note_len) {
    if (index < 0 || index >= g_count) return -1;
    const expense *e = &g_items[index];
    if (year) *year = e->year;
    if (month) *month = e->month;
    if (day) *day = e->day;
    if (amount) *amount = e->amount;
    if (category && category_len > 0) { strncpy(category, e->category, (size_t)category_len - 1); category[category_len - 1] = '\0'; }
    if (note && note_len > 0) { strncpy(note, e->note, (size_t)note_len - 1); note[note_len - 1] = '\0'; }
    return 0;
}

TS_API int ts_load(const ts_char *path) {
    FILE *f = open_file(path, 0);
    char line[256];
    g_count = 0;
    if (!f) return 0;                       /* first run: no file yet */
    while (fgets(line, sizeof line, f)) {
        expense e;
        char cat[CAT_MAX] = "", note[NOTE_MAX] = "";
        int n = sscanf(line, "%d-%d-%d,%lf,%31[^,\r\n],%127[^\r\n]",
                       &e.year, &e.month, &e.day, &e.amount, cat, note);
        if (n >= 5 && valid_date(e.year, e.month, e.day) && e.amount > 0) {
            clean(e.category, CAT_MAX, cat);
            clean(e.note, NOTE_MAX, note);
            if (push(&e) < 0) { fclose(f); return -1; }
        }
    }
    fclose(f);
    return g_count;
}

TS_API int ts_save(const ts_char *path) {
    FILE *f = open_file(path, 1);
    int i;
    if (!f) return -1;
    for (i = 0; i < g_count; i++) {
        const expense *e = &g_items[i];
        fprintf(f, "%04d-%02d-%02d,%.2f,%s,%s\n",
                e->year, e->month, e->day, e->amount, e->category, e->note);
    }
    return fclose(f) == 0 ? 0 : -1;
}
