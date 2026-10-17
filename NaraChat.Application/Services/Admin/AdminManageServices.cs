using NaraChat.Application.Services.Bots;
using NaraChat.Contract.Models.Admin;

namespace NaraChat.Application.Services.Admin
{
    /// <summary>کاربران و نقش‌ها در پنل ادمین. استثنا بیرون نمی‌دهد.</summary>
    public interface IAdminUsersService
    {
        Task<AdminUsersPage?> ListAsync(int page, int size, string? search, string? status, string? city, Guid? roleId, CancellationToken ct = default);
        Task<AdminUserDetail?> GetAsync(Guid id, CancellationToken ct = default);
        Task<(bool Ok, string? Phone, string Message)> RevealPhoneAsync(Guid id, CancellationToken ct = default);
        /// <summary>action: disable | enable | sessions</summary>
        Task<(bool Ok, string Message)> SetAccessAsync(Guid id, string action, string? reason, CancellationToken ct = default);
        Task<(bool Ok, string Message)> SetRolesAsync(Guid id, List<Guid> roleIds, CancellationToken ct = default);
        Task<(bool Ok, string Message)> RenameAsync(Guid id, string firstName, string lastName, CancellationToken ct = default);
        Task<List<AdminRoleInfo>?> RolesAsync(CancellationToken ct = default);
        Task<(bool Ok, string Message)> CreateRoleAsync(string name, CancellationToken ct = default);
        Task<(bool Ok, string Message)> RenameRoleAsync(Guid id, string name, CancellationToken ct = default);
        Task<(bool Ok, string Message)> DeleteRoleAsync(Guid id, CancellationToken ct = default);
        /// <summary>خروجی Excel فعلی کاربران (/api/v1/users/ExportUsers)؛ null یعنی نشد.</summary>
        Task<(byte[] Bytes, string FileName, string ContentType)?> ExportAsync(string? search, CancellationToken ct = default);
    }

    public sealed class AdminUsersService(HttpClient httpClient) : BotApiBase(httpClient), IAdminUsersService
    {
        private const string Base = "/api/v1/adminusers";
        private const string Roles = "/api/v1/adminroles";

        public Task<AdminUsersPage?> ListAsync(int page, int size, string? search, string? status, string? city, Guid? roleId, CancellationToken ct = default)
        {
            var q = new List<string> { $"page={page}", $"size={size}" };
            if (!string.IsNullOrWhiteSpace(search)) q.Add("search=" + Uri.EscapeDataString(search.Trim()));
            if (!string.IsNullOrWhiteSpace(status) && status != "all") q.Add("status=" + Uri.EscapeDataString(status));
            if (!string.IsNullOrWhiteSpace(city)) q.Add("city=" + Uri.EscapeDataString(city));
            if (roleId is Guid r) q.Add("roleId=" + r);
            return Get<AdminUsersPage>($"{Base}?{string.Join("&", q)}", ct);
        }

        public Task<AdminUserDetail?> GetAsync(Guid id, CancellationToken ct = default)
            => Get<AdminUserDetail>($"{Base}/{id}", ct);

        public Task<(bool Ok, string? Phone, string Message)> RevealPhoneAsync(Guid id, CancellationToken ct = default)
            => Send<string>(HttpMethod.Post, $"{Base}/{id}/phone", null, ct);

        public async Task<(bool Ok, string Message)> SetAccessAsync(Guid id, string action, string? reason, CancellationToken ct = default)
        {
            var (ok, _, message) = await Send<object>(HttpMethod.Post, $"{Base}/{id}/access", new { action, reason }, ct);
            return (ok, message);
        }

        public async Task<(bool Ok, string Message)> SetRolesAsync(Guid id, List<Guid> roleIds, CancellationToken ct = default)
        {
            var (ok, _, message) = await Send<object>(HttpMethod.Put, $"{Base}/{id}/roles", new { roleIds }, ct);
            return (ok, message);
        }

        public async Task<(bool Ok, string Message)> RenameAsync(Guid id, string firstName, string lastName, CancellationToken ct = default)
        {
            var (ok, _, message) = await Send<object>(HttpMethod.Put, $"{Base}/{id}/name", new { firstName, lastName }, ct);
            return (ok, message);
        }

        public Task<List<AdminRoleInfo>?> RolesAsync(CancellationToken ct = default)
            => Get<List<AdminRoleInfo>>(Roles, ct);

        public async Task<(bool Ok, string Message)> CreateRoleAsync(string name, CancellationToken ct = default)
        {
            var (ok, _, message) = await Send<object>(HttpMethod.Post, Roles, new { name }, ct);
            return (ok, message);
        }

        public async Task<(bool Ok, string Message)> RenameRoleAsync(Guid id, string name, CancellationToken ct = default)
        {
            var (ok, _, message) = await Send<object>(HttpMethod.Put, $"{Roles}/{id}", new { name }, ct);
            return (ok, message);
        }

        public async Task<(bool Ok, string Message)> DeleteRoleAsync(Guid id, CancellationToken ct = default)
        {
            var (ok, _, message) = await Send<object>(HttpMethod.Delete, $"{Roles}/{id}", null, ct);
            return (ok, message);
        }

        public async Task<(byte[] Bytes, string FileName, string ContentType)?> ExportAsync(string? search, CancellationToken ct = default)
        {
            try
            {
                var url = "/api/v1/users/ExportUsers" + (string.IsNullOrWhiteSpace(search) ? string.Empty : "?Search=" + Uri.EscapeDataString(search.Trim()));
                using var response = await Http.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                    return null;

                var bytes = await response.Content.ReadAsByteArrayAsync(ct);
                var name = response.Content.Headers.ContentDisposition?.FileNameStar
                           ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                           ?? "users.xlsx";
                var type = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                return (bytes, name, type);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>گروه‌ها و کانال‌ها در پنل ادمین؛ kind: group | channel.</summary>
    public interface IAdminRoomsService
    {
        Task<AdminRoomsPage?> ListAsync(string kind, int page, int size, string? search, string? status, CancellationToken ct = default);
        Task<AdminRoomDetail?> GetAsync(string kind, Guid id, CancellationToken ct = default);
        /// <summary>action: archive | restore | remove-member</summary>
        Task<(bool Ok, string Message)> ActAsync(string kind, Guid id, string action, Guid? memberId, string? reason, CancellationToken ct = default);
        Task<(bool Ok, string Message)> EditAsync(string kind, Guid id, string title, string? description, CancellationToken ct = default);
    }

    public sealed class AdminRoomsService(HttpClient httpClient) : BotApiBase(httpClient), IAdminRoomsService
    {
        private const string Base = "/api/v1/adminrooms";

        public Task<AdminRoomsPage?> ListAsync(string kind, int page, int size, string? search, string? status, CancellationToken ct = default)
        {
            var q = new List<string> { $"page={page}", $"size={size}" };
            if (!string.IsNullOrWhiteSpace(search)) q.Add("search=" + Uri.EscapeDataString(search.Trim()));
            if (!string.IsNullOrWhiteSpace(status) && status != "all") q.Add("status=" + Uri.EscapeDataString(status));
            return Get<AdminRoomsPage>($"{Base}/{kind}?{string.Join("&", q)}", ct);
        }

        public Task<AdminRoomDetail?> GetAsync(string kind, Guid id, CancellationToken ct = default)
            => Get<AdminRoomDetail>($"{Base}/{kind}/{id}", ct);

        public async Task<(bool Ok, string Message)> EditAsync(string kind, Guid id, string title, string? description, CancellationToken ct = default)
        {
            var (ok, _, message) = await Send<object>(HttpMethod.Put, $"{Base}/{kind}/{id}", new { title, description }, ct);
            return (ok, message);
        }

        public async Task<(bool Ok, string Message)> ActAsync(string kind, Guid id, string action, Guid? memberId, string? reason, CancellationToken ct = default)
        {
            var (ok, _, message) = await Send<object>(HttpMethod.Post, $"{Base}/{kind}/{id}", new { action, memberId, reason }, ct);
            return (ok, message);
        }
    }
}
