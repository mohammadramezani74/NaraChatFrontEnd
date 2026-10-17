using System.Globalization;
using NaraChat.Contract.Utilities.PersianUtlities;

namespace NaraChatFrontEnd.Pages.Admin
{
    /// <summary>قالب‌بندی مشترک صفحه‌های پنل ادمین: عدد و تاریخ فارسی، زمان نسبی، حروف اول نام.</summary>
    public static class AdminFormat
    {
        private static readonly PersianCalendar Pc = new();

        private static readonly string[] Months =
            { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };

        public static string Num(int n) => n.ToString("N0", CultureInfo.InvariantCulture).Replace(",", "٬").ToPersianNumber();

        public static string Date(DateTime? at)
        {
            if (at is not DateTime d || d.Year < 1900) return "—";
            return Pc.GetDayOfMonth(d).ToString().ToPersianNumber() + " " + Months[Pc.GetMonth(d) - 1] + " " + Pc.GetYear(d).ToString().ToPersianNumber();
        }

        public static string Clock(DateTime at) => at.ToString("HH:mm", CultureInfo.InvariantCulture).ToPersianNumber();

        /// <summary>«همین الان»، «۵ دقیقه پیش»، «امروز ۱۰:۲۰»، «دیروز»، «۱۲ روز پیش»، یا تاریخ.</summary>
        public static string Ago(DateTime? at, bool online = false)
        {
            if (online) return "آنلاین";
            if (at is not DateTime d || d.Year < 1900) return "هرگز";
            var now = DateTime.Now;
            var span = now - d;
            if (span.TotalMinutes < 1) return "همین الان";
            if (span.TotalMinutes < 60) return ((int)span.TotalMinutes).ToString().ToPersianNumber() + " دقیقه پیش";
            if (d.Date == now.Date) return "امروز " + Clock(d);
            if (d.Date == now.Date.AddDays(-1)) return "دیروز";
            if (span.TotalDays < 30) return ((int)span.TotalDays).ToString().ToPersianNumber() + " روز پیش";
            return Date(d);
        }

        public static string Initials(string? name)
        {
            var parts = (name ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => "؟",
                1 => parts[0][..1],
                _ => parts[0][..1] + "‌" + parts[1][..1],
            };
        }

        public static string Phone(string? phone) => string.IsNullOrWhiteSpace(phone) ? "—" : phone.ToPersianNumber();

        /// <summary>CSV با BOM تا Excel فارسی را درست باز کند؛ خروجی base64 برای triggerDownload.</summary>
        public static string CsvBase64(IEnumerable<IEnumerable<string?>> rows)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var row in rows)
            {
                sb.Append(string.Join(",", row.Select(Cell)));
                sb.Append("\r\n");
            }
            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return Convert.ToBase64String(bytes);

            static string Cell(string? value)
            {
                value ??= string.Empty;
                // جلوی فرمول‌سازی در Excel (=، +، -، @) گرفته می‌شود.
                if (value.Length > 0 && "=+-@".Contains(value[0])) value = "'" + value;
                return value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
            }
        }

        /// <summary>تاریخ و ساعت شمسی برای خروجی.</summary>
        public static string DateTimeText(DateTime? at) => at is DateTime d ? Date(d) + " " + Clock(d) : string.Empty;

        private static readonly string[] Tones = { "is-blue", "is-orange", "is-green", "is-yellow" };

        /// <summary>رنگ ثابت آواتار برای هر شناسه.</summary>
        public static string Tone(Guid id) => Tones[(id.GetHashCode() & 0x7fffffff) % Tones.Length];
    }
}
