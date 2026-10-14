using System.Net.Http.Json;
using System.Text.Json;
using NaraChat.Contract.Models.BaseResponse;
using NaraChat.Contract.Models.Bots;
using NaraChat.Contract.Models.Chat.Conversation;

namespace NaraChat.Application.Services.Bots
{
    /// <summary>ربات‌ها از دید کاربر. هیچ متدی استثنا بیرون نمی‌دهد.</summary>
    public interface IBotService
    {
        Task<BotInfoDto?> GetInfoAsync(Guid botUserId, CancellationToken ct = default);
        Task<(bool Ok, StartBotResult? Result, string Message)> StartAsync(Guid botUserId, CancellationToken ct = default);
        Task<(bool Ok, string Message)> PressAsync(Guid messageId, string data, CancellationToken ct = default);
        Task<bool> IsAdminAsync(CancellationToken ct = default);
    }

    /// <summary>پنل ادمین ربات‌ها (/api/v1/botadmin).</summary>
    public interface IBotAdminService
    {
        Task<List<BotAdminListItem>?> ListAsync(CancellationToken ct = default);
        Task<BotAdminDetail?> GetAsync(Guid id, CancellationToken ct = default);
        Task<(bool Ok, Guid Id, string Message)> CreateAsync(string name, string handle, string? description, CancellationToken ct = default);
        Task<(bool Ok, BotAdminDetail? Bot, string Message)> UpdateAsync(BotAdminDetail bot, CancellationToken ct = default);
        Task<(bool Ok, BotAdminDetail? Bot, string Message)> SaveMenuAsync(Guid id, List<BotMenuNodeDto> nodes, CancellationToken ct = default);
        Task<List<BotFlowInfo>?> FlowsAsync(CancellationToken ct = default);
        Task<List<BotConnectorInfo>?> ConnectorsAsync(CancellationToken ct = default);
        Task<BotConnectorTestResult> TestConnectorAsync(string key, CancellationToken ct = default);
        Task<List<BotPickItem>?> RolesAsync(CancellationToken ct = default);
        Task<List<BotPickItem>?> SearchUsersAsync(string? search, CancellationToken ct = default);
    }

    /// <summary>پایه‌ی مشترک: درخواست و خواندن BaseResponseDto بدون استثنا.</summary>
    public abstract class BotApiBase(HttpClient httpClient)
    {
        protected const string GenericError = "ارتباط با سرور برقرار نشد. دوباره تلاش کنید.";
        protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
        protected readonly HttpClient Http = httpClient;

        protected async Task<T?> Get<T>(string url, CancellationToken ct) where T : class
        {
            try
            {
                using var response = await Http.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[bots] GET {url} -> {(int)response.StatusCode}");
                    return null;
                }

                var payload = await response.Content.ReadFromJsonAsync<BaseResponseDto<T>>(Json, ct);
                return payload?.isSucceded == true ? payload.result : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[bots] GET {url} failed: {ex.Message}");
                return null;
            }
        }

        protected async Task<(bool Ok, T? Value, string Message)> Send<T>(HttpMethod method, string url, object? body, CancellationToken ct)
        {
            try
            {
                using var request = new HttpRequestMessage(method, url);
                if (body is not null)
                    request.Content = JsonContent.Create(body, options: Json);

                using var response = await Http.SendAsync(request, ct);

                BaseResponseDto<T>? payload = null;
                try
                {
                    payload = await response.Content.ReadFromJsonAsync<BaseResponseDto<T>>(Json, ct);
                }
                catch (Exception) { /* بدنه‌ی غیر JSON */ }

                if (response.IsSuccessStatusCode && payload?.isSucceded == true)
                    return (true, payload.result, payload.message ?? string.Empty);

                return (false, default, string.IsNullOrWhiteSpace(payload?.message) ? GenericError : payload!.message);
            }
            catch (Exception)
            {
                return (false, default, GenericError);
            }
        }
    }

    public sealed class BotService(HttpClient httpClient) : BotApiBase(httpClient), IBotService
    {
        private const string Base = "/api/v1/bots";

        public Task<BotInfoDto?> GetInfoAsync(Guid botUserId, CancellationToken ct = default)
            => Get<BotInfoDto>($"{Base}/{botUserId}", ct);

        public Task<(bool Ok, StartBotResult? Result, string Message)> StartAsync(Guid botUserId, CancellationToken ct = default)
            => Send<StartBotResult>(HttpMethod.Post, $"{Base}/{botUserId}/start", null, ct);

        public async Task<(bool Ok, string Message)> PressAsync(Guid messageId, string data, CancellationToken ct = default)
        {
            var (ok, _, message) = await Send<object>(HttpMethod.Post, $"{Base}/callback", new { messageId, data }, ct);
            return (ok, message);
        }

        public async Task<bool> IsAdminAsync(CancellationToken ct = default)
        {
            try
            {
                var result = await Http.GetFromJsonAsync<AdminAccessDto>($"{Base}/admin-access", Json, ct);
                return result?.IsAdmin == true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private sealed class AdminAccessDto
        {
            public bool IsAdmin { get; set; }
        }
    }

    public sealed class BotAdminService(HttpClient httpClient) : BotApiBase(httpClient), IBotAdminService
    {
        private const string Base = "/api/v1/botadmin";

        public Task<List<BotAdminListItem>?> ListAsync(CancellationToken ct = default)
            => Get<List<BotAdminListItem>>($"{Base}/", ct);

        public Task<BotAdminDetail?> GetAsync(Guid id, CancellationToken ct = default)
            => Get<BotAdminDetail>($"{Base}/{id}", ct);

        public async Task<(bool Ok, Guid Id, string Message)> CreateAsync(string name, string handle, string? description, CancellationToken ct = default)
        {
            var (ok, id, message) = await Send<Guid>(HttpMethod.Post, $"{Base}/", new { name, handle, description }, ct);
            return (ok, id, message);
        }

        public Task<(bool Ok, BotAdminDetail? Bot, string Message)> UpdateAsync(BotAdminDetail bot, CancellationToken ct = default)
            => Send<BotAdminDetail>(HttpMethod.Put, $"{Base}/{bot.Id}", new
            {
                name = bot.Name,
                description = bot.Description,
                isEnabled = bot.IsEnabled,
                accessMode = bot.AccessMode,
                accessList = bot.AccessList,
                commands = bot.Commands,
            }, ct);

        public Task<(bool Ok, BotAdminDetail? Bot, string Message)> SaveMenuAsync(Guid id, List<BotMenuNodeDto> nodes, CancellationToken ct = default)
            => Send<BotAdminDetail>(HttpMethod.Put, $"{Base}/{id}/menu", new { nodes }, ct);

        public Task<List<BotFlowInfo>?> FlowsAsync(CancellationToken ct = default)
            => Get<List<BotFlowInfo>>($"{Base}/flows", ct);

        public Task<List<BotConnectorInfo>?> ConnectorsAsync(CancellationToken ct = default)
            => Get<List<BotConnectorInfo>>($"{Base}/connectors", ct);

        public async Task<BotConnectorTestResult> TestConnectorAsync(string key, CancellationToken ct = default)
        {
            var (ok, result, message) = await Send<BotConnectorTestResult>(
                HttpMethod.Post, $"{Base}/connectors/{Uri.EscapeDataString(key)}/test", null, ct);
            return ok && result is not null ? result : new BotConnectorTestResult { Ok = false, Message = message };
        }

        public Task<List<BotPickItem>?> RolesAsync(CancellationToken ct = default)
            => Get<List<BotPickItem>>("/api/v1/roles/GetAll", ct);

        public Task<List<BotPickItem>?> SearchUsersAsync(string? search, CancellationToken ct = default)
            => Get<List<BotPickItem>>("/api/v1/users/GetUsersForGroup" +
                                      (string.IsNullOrWhiteSpace(search) ? string.Empty : "?search=" + Uri.EscapeDataString(search)), ct);
    }

    /// <summary>
    /// وضعیت زنده‌ی ربات‌ها در مرورگر: کیبورد هر ربات و پیام‌هایی که ربات عوض کرده.
    /// Chat.razor رویدادهای هاب را اینجا می‌ریزد و ChatDetail گوش می‌دهد.
    /// </summary>
    public sealed class BotStore
    {
        private readonly Dictionary<Guid, BotStateDto> _states = new();

        public event Action<Guid>? StateChanged;
        public event Action<ChatMessageDto>? MessageUpdated;

        public BotStateDto? Get(Guid botUserId)
            => _states.TryGetValue(botUserId, out var state) ? state : null;

        public void SetState(BotStateDto state)
        {
            _states[state.BotUserId] = state;
            StateChanged?.Invoke(state.BotUserId);
        }

        public void PublishMessageUpdate(ChatMessageDto message) => MessageUpdated?.Invoke(message);
    }
}
