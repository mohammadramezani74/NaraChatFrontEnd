using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using NaraChat.Contract.Models.BaseResponse;
using NaraChat.Contract.Models.ContactTasks;

namespace NaraChat.Application.Services.ContactTasks
{
    public sealed class ContactTaskService(HttpClient httpClient) : IContactTaskService
    {
        private const string Base = "/api/v1/contacttasks";
        private const string GenericError = "ارتباط با سرور برقرار نشد. دوباره تلاش کنید.";

        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http = httpClient;

        public Task<List<ContactTaskCountDto>?> GetCountsAsync(CancellationToken ct = default)
            => Get<List<ContactTaskCountDto>>($"{Base}/counts", ct);

        public Task<ContactTaskListDto?> GetListAsync(Guid contactId, CancellationToken ct = default)
            => Get<ContactTaskListDto>($"{Base}/{contactId}", ct);

        private async Task<T?> Get<T>(string url, CancellationToken ct) where T : class
        {
            try
            {
                using var response = await _http.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                {
                    // برای عیب‌یابی: 404 یعنی بک‌اند هنوز به‌روز نشده، 500 معمولاً
                    // یعنی migration روی دیتابیس اجرا نشده.
                    Console.WriteLine($"[tasks] GET {url} -> {(int)response.StatusCode}");
                    return null;
                }

                var payload = await response.Content.ReadFromJsonAsync<BaseResponseDto<T>>(Json, ct);
                return payload?.isSucceded == true ? payload.result : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[tasks] GET {url} failed: {ex.Message}");
                return null;
            }
        }

        public Task<ContactTaskResult> CreateAsync(Guid contactId, string text, DateTime? dueDate, Guid? sourceMessageId = null, DateTimeOffset? remindAt = null, CancellationToken ct = default)
            => SendForTask(HttpMethod.Post, $"{Base}/create", new
            {
                contactId,
                text,
                dueDate = DateOnlyString(dueDate),
                sourceMessageId,
                remindAt,
            }, ct);

        public Task<ContactTaskResult> UpdateAsync(Guid id, string text, DateTime? dueDate, DateTimeOffset? remindAt, CancellationToken ct = default)
            => SendForTask(HttpMethod.Put, $"{Base}/{id}", new
            {
                text,
                dueDate = DateOnlyString(dueDate),
                remindAt,
            }, ct);

        public Task<ContactTaskResult> SetDoneAsync(Guid id, bool done, CancellationToken ct = default)
            => SendForTask(HttpMethod.Post, $"{Base}/{id}/done?value={(done ? "true" : "false")}", null, ct);

        public async Task<(bool Ok, string Message)> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var (ok, _, message) = await Send<object>(HttpMethod.Delete, $"{Base}/{id}", null, ct);
            return (ok, message);
        }

        public async Task<(bool Ok, string Message)> ClearDoneAsync(Guid contactId, CancellationToken ct = default)
        {
            var (ok, _, message) = await Send<object>(HttpMethod.Delete, $"{Base}/{contactId}/done", null, ct);
            return (ok, message);
        }

        public async Task<(bool Ok, int Imported)> ImportAsync(Guid contactId, List<ImportContactTaskItem> items, CancellationToken ct = default)
        {
            var (ok, count, _) = await Send<int>(HttpMethod.Post, $"{Base}/import", new { contactId, items }, ct);
            return (ok, count);
        }

        // ---------------------------------------------------------------

        private async Task<ContactTaskResult> SendForTask(HttpMethod method, string url, object? body, CancellationToken ct)
        {
            var (ok, task, message) = await Send<ContactTaskDto>(method, url, body, ct);
            return new ContactTaskResult(ok && task is not null, task, message);
        }

        private async Task<(bool Ok, T? Value, string Message)> Send<T>(HttpMethod method, string url, object? body, CancellationToken ct)
        {
            try
            {
                using var request = new HttpRequestMessage(method, url);
                if (body is not null)
                    request.Content = JsonContent.Create(body, options: Json);

                using var response = await _http.SendAsync(request, ct);

                BaseResponseDto<T>? payload = null;
                try
                {
                    payload = await response.Content.ReadFromJsonAsync<BaseResponseDto<T>>(Json, ct);
                }
                catch (Exception) { /* بدنه‌ی غیر JSON، مثلاً خطای IIS */ }

                if (response.IsSuccessStatusCode && payload?.isSucceded == true)
                    return (true, payload.result, payload.message ?? string.Empty);

                return (false, default, string.IsNullOrWhiteSpace(payload?.message) ? GenericError : payload!.message);
            }
            catch (Exception)
            {
                return (false, default, GenericError);
            }
        }

        /// <summary>
        /// سررسید فقط «روز» است؛ با فرمت yyyy-MM-dd فرستاده می‌شود تا هیچ تبدیل
        /// منطقه‌ی زمانی‌ای آن را یک روز جابه‌جا نکند.
        /// </summary>
        private static string? DateOnlyString(DateTime? date)
            => date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
