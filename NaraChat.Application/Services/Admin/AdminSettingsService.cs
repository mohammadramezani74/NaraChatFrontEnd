using NaraChat.Application.Services.Bots;
using NaraChat.Contract.Models.Admin;

namespace NaraChat.Application.Services.Admin
{
    /// <summary>تنظیمات پنل ادمین. استثنا بیرون نمی‌دهد.</summary>
    public interface IAdminSettingsService
    {
        Task<AdminSettingsDto?> GetAsync(CancellationToken ct = default);
        Task<(bool Ok, string Message)> SaveAnnouncementAsync(AnnouncementSetting value, CancellationToken ct = default);
        Task<(bool Ok, string Message)> SaveCreationAsync(CreationSetting value, CancellationToken ct = default);
    }

    public sealed class AdminSettingsService(HttpClient httpClient) : BotApiBase(httpClient), IAdminSettingsService
    {
        private const string Base = "/api/v1/adminsettings";

        public Task<AdminSettingsDto?> GetAsync(CancellationToken ct = default)
            => Get<AdminSettingsDto>(Base, ct);

        public async Task<(bool Ok, string Message)> SaveAnnouncementAsync(AnnouncementSetting value, CancellationToken ct = default)
        {
            var (ok, _, message) = await Send<object>(HttpMethod.Put, $"{Base}/announcement", value, ct);
            return (ok, message);
        }

        public async Task<(bool Ok, string Message)> SaveCreationAsync(CreationSetting value, CancellationToken ct = default)
        {
            var (ok, _, message) = await Send<object>(HttpMethod.Put, $"{Base}/creation", value, ct);
            return (ok, message);
        }
    }

    /// <summary>اطلاعات عمومی برنامه (اعلان فعال و اجازه‌ی ساخت). نتیجه چند ثانیه نگه داشته می‌شود.</summary>
    public interface IAppInfoService
    {
        Task<AppInfoDto> GetAsync(bool force = false, CancellationToken ct = default);
    }

    public sealed class AppInfoService(HttpClient httpClient) : BotApiBase(httpClient), IAppInfoService
    {
        private AppInfoDto? _last;
        private DateTime _at;

        public async Task<AppInfoDto> GetAsync(bool force = false, CancellationToken ct = default)
        {
            if (!force && _last is not null && DateTime.UtcNow - _at < TimeSpan.FromSeconds(20))
                return _last;
            var fresh = await Get<AppInfoDto>("/api/v1/appinfo", ct);
            if (fresh is null)
                return _last ?? new AppInfoDto(); // خطای شبکه: چیزی را پنهان نکن
            _last = fresh;
            _at = DateTime.UtcNow;
            return fresh;
        }
    }
}
