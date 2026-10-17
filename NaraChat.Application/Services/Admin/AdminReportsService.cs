using System.Globalization;
using NaraChat.Application.Services.Bots;
using NaraChat.Contract.Models.Admin;

namespace NaraChat.Application.Services.Admin
{
    /// <summary>گزارش‌های پنل ادمین. استثنا بیرون نمی‌دهد؛ خطا = null.</summary>
    public interface IAdminReportsService
    {
        Task<UsageReport?> UsageAsync(DateTime from, DateTime to, CancellationToken ct = default);
        Task<InactiveUsersPage?> InactiveAsync(int days, int take, CancellationToken ct = default);
        Task<AuditPage?> AuditAsync(int page, int size, string? area, Guid? actorId, DateTime? from, DateTime? to, string? search, CancellationToken ct = default);
    }

    public sealed class AdminReportsService(HttpClient httpClient) : BotApiBase(httpClient), IAdminReportsService
    {
        private const string Base = "/api/v1/adminreports";

        private static string D(DateTime d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        public Task<UsageReport?> UsageAsync(DateTime from, DateTime to, CancellationToken ct = default)
            => Get<UsageReport>($"{Base}/usage?from={D(from)}&to={D(to)}", ct);

        public Task<InactiveUsersPage?> InactiveAsync(int days, int take, CancellationToken ct = default)
            => Get<InactiveUsersPage>($"{Base}/inactive?days={days}&take={take}", ct);

        public Task<AuditPage?> AuditAsync(int page, int size, string? area, Guid? actorId, DateTime? from, DateTime? to, string? search, CancellationToken ct = default)
        {
            var q = new List<string> { $"page={page}", $"size={size}" };
            if (!string.IsNullOrWhiteSpace(area) && area != "all") q.Add("area=" + Uri.EscapeDataString(area));
            if (actorId is Guid a) q.Add("actorId=" + a);
            if (from is DateTime f) q.Add("from=" + D(f));
            if (to is DateTime t) q.Add("to=" + D(t));
            if (!string.IsNullOrWhiteSpace(search)) q.Add("search=" + Uri.EscapeDataString(search.Trim()));
            return Get<AuditPage>($"{Base}/audit?{string.Join("&", q)}", ct);
        }
    }
}
