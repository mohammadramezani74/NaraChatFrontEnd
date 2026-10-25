using System.Globalization;
using NaraChatFrontEnd.Models.BaseModels;

namespace NaraChatFrontEnd.Pages.ChatPages.Components.ChatComponents.Media
{
    /// <summary>
    /// کمکی‌های مشترک پخش‌کننده‌ی ویس و ویدیو، نقشه و ضبط.
    /// همه‌ی اعداد با InvariantCulture ساخته می‌شوند، چون در CSS و URL نباید
    /// جداکننده‌ی اعشار یا رقم فارسی برود؛ رقم فارسی فقط برای نمایش است.
    /// </summary>
    public static class MediaKit
    {
        private const string FaDigits = "۰۱۲۳۴۵۶۷۸۹";

        public static string Fa(string s)
        {
            var chars = s.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (chars[i] >= '0' && chars[i] <= '9') chars[i] = FaDigits[chars[i] - '0'];
                else if (chars[i] == '.') chars[i] = '٫';
            }
            return new string(chars);
        }

        public static long? ParseSize(string? size)
        {
            if (string.IsNullOrWhiteSpace(size)) return null;
            return decimal.TryParse(size, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) && d >= 0
                ? (long)d
                : null;
        }

        public static string FormatBytes(long bytes)
        {
            string[] units = { "بایت", "کیلوبایت", "مگابایت", "گیگابایت" };
            double value = bytes;
            var unit = 0;
            while (value >= 1024 && unit < units.Length - 1) { value /= 1024; unit++; }
            var number = value.ToString(unit == 0 || value >= 100 ? "0" : "0.#", CultureInfo.InvariantCulture);
            return Fa(number) + " " + units[unit];
        }

        /// <summary>رنگ اصلی تم کاربر برای دکمه‌ها و بخش پخش‌شده.</summary>
        public static string Accent(ThemeChanging? theme)
        {
            var name = (theme?.SeenColor ?? MudBlazor.Color.Primary).ToString().ToLowerInvariant();
            return $"var(--mud-palette-{name}, var(--mud-palette-primary))";
        }

        public static string Percent(double fraction)
            => (Math.Clamp(fraction, 0, 1) * 100).ToString("0.#", CultureInfo.InvariantCulture) + "%";

        public static string Coord(double value) => value.ToString("0.######", CultureInfo.InvariantCulture);

        /// <summary>
        /// موج ساختگی ولی ثابت برای هر فایل، تا قبل از دریافت ویس شکل داشته
        /// باشد و با هر رندر عوض نشود. بعد از دریافت، موج واقعی جایش می‌نشیند.
        /// </summary>
        public static double[] PseudoPeaks(Guid id, int count)
        {
            var b = id.ToByteArray();
            var seed = BitConverter.ToUInt32(b, 0) ^ BitConverter.ToUInt32(b, 12);
            var result = new double[count];
            for (var i = 0; i < count; i++)
            {
                seed = unchecked(seed * 1664525u + 1013904223u);
                var r = ((seed >> 8) & 0xFFFF) / 65535.0;
                var envelope = 0.55 + 0.45 * Math.Sin((i + 0.5) / count * Math.PI);
                result[i] = Math.Round(0.18 + 0.72 * r * envelope, 2);
            }
            return result;
        }
    }

    /// <summary>آیکون‌های Material به صورت path برای svg درون‌خطی.</summary>
    public static class NaraIcons
    {
        public const string Play = "M8 5v14l11-7z";
        public const string Pause = "M6 19h4V5H6v14zm8-14v14h4V5h-4z";
        public const string Close = "M19 6.41 17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z";
        public const string Download = "M5 20h14v-2H5v2zM19 9h-4V3H9v6H5l7 7 7-7z";
        public const string Refresh = "M17.65 6.35A7.958 7.958 0 0 0 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08A5.99 5.99 0 0 1 12 18c-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z";
        public const string VolumeUp = "M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.25 2.5-4.02zM14 3.23v2.06c2.89.86 5 3.54 5 6.71s-2.11 5.85-5 6.71v2.06c4.01-.91 7-4.49 7-8.77s-2.99-7.86-7-8.77z";
        public const string VolumeOff = "M16.5 12c0-1.77-1.02-3.29-2.5-4.03v2.21l2.45 2.45c.03-.2.05-.41.05-.63zm2.5 0c0 .94-.2 1.82-.54 2.64l1.51 1.51A8.796 8.796 0 0 0 21 12c0-4.28-2.99-7.86-7-8.77v2.06c2.89.86 5 3.54 5 6.71zM4.27 3 3 4.27 7.73 9H3v6h4l5 5v-6.73l4.25 4.25c-.67.52-1.42.93-2.25 1.18v2.06a8.99 8.99 0 0 0 3.69-1.81L19.73 21 21 19.73l-9-9L4.27 3zM12 4 9.91 6.09 12 8.18V4z";
        public const string Fullscreen = "M7 14H5v5h5v-2H7v-3zm-2-4h2V7h3V5H5v5zm12 7h-3v2h5v-5h-2v3zM14 5v2h3v3h2V5h-5z";
        public const string FullscreenExit = "M5 16h3v3h2v-5H5v2zm3-8H5v2h5V5H8v3zm6 11h2v-3h3v-2h-5v5zm2-11V5h-2v5h5V8h-3z";
        public const string Pip = "M19 11h-8v6h8v-6zm4 8V4.98C23 3.88 22.1 3 21 3H3c-1.1 0-2 .88-2 1.98V19c0 1.1.9 2 2 2h18c1.1 0 2-.9 2-2zm-2 .02H3V4.97h18v14.05z";
        public const string Movie = "M18 4l2 4h-3l-2-4h-2l2 4h-3l-2-4H8l2 4H7L5 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V4h-4z";
        public const string Delete = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z";
        public const string Send = "M2.01 21 23 12 2.01 3 2 10l15 2-15 2z";
        public const string Copy = "M16 1H4c-1.1 0-2 .9-2 2v14h2V3h12V1zm3 4H8c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h11c1.1 0 2-.9 2-2V7c0-1.1-.9-2-2-2zm0 16H8V7h11v14z";
        public const string Check = "M9 16.17 4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z";
        public const string Map = "M20.5 3l-.16.03L15 5.1 9 3 3.36 4.9c-.21.07-.36.25-.36.48V20.5c0 .28.22.5.5.5l.16-.03L9 18.9l6 2.1 5.64-1.9c.21-.07.36-.25.36-.48V3.5c0-.28-.22-.5-.5-.5zM15 19l-6-2.11V5l6 2.11V19z";
        public const string Directions = "M21.71 11.29l-9-9a.996.996 0 0 0-1.41 0l-9 9a.996.996 0 0 0 0 1.41l9 9c.39.39 1.02.39 1.41 0l9-9a.996.996 0 0 0 0-1.41zM14 14.5V12h-4v3H8v-4c0-.55.45-1 1-1h5V7.5l3.5 3.5-3.5 3.5z";
        public const string MyLocation = "M12 8c-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4-1.79-4-4-4zm8.94 3A8.994 8.994 0 0 0 13 3.06V1h-2v2.06A8.994 8.994 0 0 0 3.06 11H1v2h2.06A8.994 8.994 0 0 0 11 20.94V23h2v-2.06A8.994 8.994 0 0 0 20.94 13H23v-2h-2.06zM12 19c-3.87 0-7-3.13-7-7s3.13-7 7-7 7 3.13 7 7-3.13 7-7 7z";
    }

    /// <summary>جواب naraVoice.load.</summary>
    public sealed class VoiceLoadResult
    {
        public double Duration { get; set; }
        public string? DurationText { get; set; }
        public double[]? Peaks { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>جواب naraVideo.load.</summary>
    public sealed class VideoLoadResult
    {
        public bool Ok { get; set; }
        public double Duration { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>جواب naraRecorder.start و stop.</summary>
    public sealed class RecorderResult
    {
        public bool Ok { get; set; }
        public string? Error { get; set; }
        public string? Data { get; set; }
        public string? Ext { get; set; }
        public string? Mime { get; set; }
        public int Seconds { get; set; }
    }

    /// <summary>جواب naraLocation.locate.</summary>
    public sealed class GeoResult
    {
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public int Accuracy { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>نتیجه‌ی پنجره‌ی انتخاب موقعیت.</summary>
    public sealed record LocationPick(double Lat, double Lng);
}
