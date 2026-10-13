using System.Globalization;
using NaraChat.Contract.Models.ContactTasks;

namespace NaraChatFrontEnd.Pages.ChatPages.Components.ContactTasks
{
    /// <summary>متن تاریخ‌های کارها به فارسی، بدون وابستگی به فرهنگ مرورگر.</summary>
    public static class TaskDates
    {
        private static readonly PersianCalendar Pc = new();

        public static readonly string[] MonthNames =
        {
            "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
            "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند",
        };

        // به ترتیب DayOfWeek: یکشنبه = 0
        private static readonly string[] WeekDays =
        {
            "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه",
        };

        /// <summary>سرستون‌های تقویم، از شنبه.</summary>
        public static readonly string[] WeekHeader = { "ش", "ی", "د", "س", "چ", "پ", "ج" };

        public static DateTime Today => DateTime.Today;

        public static ContactTaskBucket Bucket(DateTime? due)
        {
            if (due is not DateTime d)
                return ContactTaskBucket.NoDate;

            var diff = (d.Date - Today).Days;
            return diff < 0 ? ContactTaskBucket.Overdue
                 : diff == 0 ? ContactTaskBucket.Today
                 : ContactTaskBucket.Later;
        }

        public static string BucketTitle(ContactTaskBucket bucket) => bucket switch
        {
            ContactTaskBucket.Overdue => "عقب‌افتاده",
            ContactTaskBucket.Today => "امروز",
            ContactTaskBucket.Later => "بعداً",
            _ => "بدون تاریخ",
        };

        /// <summary>«امروز»، «فردا»، «دیروز»، نام روز برای همین هفته، وگرنه «۲۱ مهر».</summary>
        public static string Label(DateTime due)
        {
            var diff = (due.Date - Today).Days;
            switch (diff)
            {
                case 0: return "امروز";
                case 1: return "فردا";
                case -1: return "دیروز";
            }

            if (diff > 1 && diff < 7)
                return WeekDays[(int)due.DayOfWeek];

            return DayMonth(due);
        }

        /// <summary>«سه‌شنبه ۲۱ مهر» — برای راهنمای ابزار و دیالوگ.</summary>
        public static string Full(DateTime date)
            => $"{WeekDays[(int)date.DayOfWeek]} {DayMonth(date)}";

        public static string DayMonth(DateTime date)
        {
            var text = $"{Fa(Pc.GetDayOfMonth(date))} {MonthNames[Pc.GetMonth(date) - 1]}";
            if (Pc.GetYear(date) != Pc.GetYear(Today))
                text += $" {Fa(Pc.GetYear(date))}";
            return text;
        }

        /// <summary>«امروز ۱۴:۳۰»، «فردا ۰۹:۰۰»، «سه‌شنبه ۱۰:۰۰» یا «۲۱ مهر ۱۰:۰۰».</summary>
        public static string ReminderLabel(DateTime local) => $"{Label(local.Date)} {Time(local)}";

        public static string ReminderLabel(DateTimeOffset at) => ReminderLabel(at.ToLocalTime().DateTime);

        public static string Time(DateTime date) => Fa(date.ToString("HH:mm", CultureInfo.InvariantCulture));

        /// <summary>شنبه‌ی هفته‌ی بعد.</summary>
        public static DateTime NextWeek()
        {
            var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)Today.DayOfWeek + 7) % 7;
            return Today.AddDays(daysUntilSaturday == 0 ? 7 : daysUntilSaturday);
        }

        public static string Fa(int number) => Fa(number.ToString(CultureInfo.InvariantCulture));

        public static string Fa(string text)
        {
            var chars = text.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
                if (chars[i] >= '0' && chars[i] <= '9')
                    chars[i] = (char)('۰' + (chars[i] - '0'));
            return new string(chars);
        }

        // ------------------------------------------------- تقویم جلالی

        public static (int Year, int Month) YearMonth(DateTime date)
            => (Pc.GetYear(date), Pc.GetMonth(date));

        public static DateTime FromPersian(int year, int month, int day)
            => Pc.ToDateTime(year, month, day, 0, 0, 0, 0);

        public static int DaysInMonth(int year, int month) => Pc.GetDaysInMonth(year, month);

        /// <summary>چند خانه‌ی خالی قبل از روز اول ماه (هفته از شنبه شروع می‌شود).</summary>
        public static int LeadingBlanks(int year, int month)
            => ((int)FromPersian(year, month, 1).DayOfWeek + 1) % 7;

        public static string MonthTitle(int year, int month) => $"{MonthNames[month - 1]} {Fa(year)}";
    }
}
