using NaraChat.Application.Services.Bots;
using NaraChat.Contract.Models.Admin;

namespace NaraChat.Application.Services.Admin
{
    /// <summary>پیام همگانی در پنل ادمین. استثنا بیرون نمی‌دهد.</summary>
    public interface IAdminBroadcastService
    {
        Task<BroadcastsPage?> ListAsync(int page, int size, CancellationToken ct = default);
        Task<BroadcastAudiencePreview?> PreviewAsync(BroadcastAudience audience, CancellationToken ct = default);
        Task<(bool Ok, Guid Id, string Message)> CreateAsync(CreateBroadcastRequest request, CancellationToken ct = default);
        Task<(bool Ok, string Message)> CancelAsync(Guid id, CancellationToken ct = default);
    }

    public sealed class AdminBroadcastService(HttpClient httpClient) : BotApiBase(httpClient), IAdminBroadcastService
    {
        private const string Base = "/api/v1/adminbroadcasts";

        public Task<BroadcastsPage?> ListAsync(int page, int size, CancellationToken ct = default)
            => Get<BroadcastsPage>($"{Base}?page={page}&size={size}", ct);

        public async Task<BroadcastAudiencePreview?> PreviewAsync(BroadcastAudience audience, CancellationToken ct = default)
        {
            var (ok, value, _) = await Send<BroadcastAudiencePreview>(HttpMethod.Post, $"{Base}/preview", audience, ct);
            return ok ? value : null;
        }

        public async Task<(bool Ok, Guid Id, string Message)> CreateAsync(CreateBroadcastRequest request, CancellationToken ct = default)
        {
            var (ok, id, message) = await Send<Guid>(HttpMethod.Post, Base, request, ct);
            return (ok, id, message);
        }

        public async Task<(bool Ok, string Message)> CancelAsync(Guid id, CancellationToken ct = default)
        {
            var (ok, _, message) = await Send<object>(HttpMethod.Post, $"{Base}/{id}/cancel", null, ct);
            return (ok, message);
        }
    }
}
