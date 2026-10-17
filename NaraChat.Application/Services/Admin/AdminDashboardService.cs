using NaraChat.Application.Services.Bots;
using NaraChat.Contract.Models.Admin;

namespace NaraChat.Application.Services.Admin
{
    /// <summary>داشبورد پنل ادمین (/api/v1/admindashboard). استثنا بیرون نمی‌دهد؛ خطا = null.</summary>
    public interface IAdminDashboardService
    {
        Task<AdminDashboardDto?> GetAsync(int days, CancellationToken ct = default);
    }

    public sealed class AdminDashboardService(HttpClient httpClient) : BotApiBase(httpClient), IAdminDashboardService
    {
        public Task<AdminDashboardDto?> GetAsync(int days, CancellationToken ct = default)
            => Get<AdminDashboardDto>($"/api/v1/admindashboard?days={(days >= 30 ? 30 : 7)}", ct);
    }
}
