// مسیر: NaraChat.Application/Services/Upload/BrowserUploader.cs
//
// نسخه‌ی نهایی. کل فایل را با این جایگزین کن.
// این نسخه به شیء سراسری window.naraUpload وصل می‌شود که مستقیم داخل
// index.html تعریف شده — دیگر هیچ فایل js جداگانه‌ای لازم نیست.

using Microsoft.JSInterop;
using NaraChat.Application.Services.Auth;
using NaraChat.Application.Services.TokenProvider;
using NaraChat.Application.Settings;

namespace NaraChat.Application.Services.Upload
{
    public sealed record BrowserFileHandle(string Handle, string Name, long Size, string ContentType);

    public sealed record BrowserUploadResponse(int Status, string Body, string? Error)
    {
        public bool IsSuccess => Status >= 200 && Status < 300;
        public bool IsAborted => Error == "aborted";
    }

    public sealed record DownloadResult(bool Ok, int Status, bool Aborted = false);

    public interface IBrowserUploader
    {
        Task<BrowserFileHandle?> CaptureAsync(string hostElementId);

        Task<BrowserUploadResponse> SendAsync(
            string handle,
            string relativeUrl,
            IDictionary<string, string> fields,
            Action<int, long, long> onProgress);

        Task AbortAsync(string handle);
        Task ReleaseAsync(string handle);

        Task<bool> DownloadAsync(
            string relativeUrl,
            string fallbackName,
            string downloadId,
            Action<int, long, long>? onProgress = null);

        Task AbortDownloadAsync(string downloadId);

        Task<bool> InitDropZoneAsync<T>(string zoneElementId, DotNetObjectReference<T> componentRef)
            where T : class;

        Task DisposeDropZoneAsync(string zoneElementId);
    }

    public sealed class BrowserUploader(
        IJSRuntime js,
        ITokenService tokenService,
        IAuthService authService) : IBrowserUploader
    {
        private readonly IJSRuntime _js = js;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IAuthService _authService = authService;

        // ------------------------------------------------------------ آپلود

        public async Task<BrowserFileHandle?> CaptureAsync(string hostElementId)
            => await _js.InvokeAsync<BrowserFileHandle?>("naraUpload.capture", hostElementId);

        public async Task<BrowserUploadResponse> SendAsync(
            string handle,
            string relativeUrl,
            IDictionary<string, string> fields,
            Action<int, long, long> onProgress)
        {
            using var proxy = DotNetObjectReference.Create(new ProgressProxy(onProgress));

            var url = BuildUrl(relativeUrl);
            var token = await _tokenService.GetAccessTokenAsync();

            var response = await _js.InvokeAsync<BrowserUploadResponse>(
                "naraUpload.send", handle, url, token, fields, proxy);

            // چون از HttpClient رد نمی‌شویم، JwtAuthorizationMessageHandler اجرا
            // نمی‌شود و رفرش توکن باید دستی انجام شود.
            if (response.Status == 401)
            {
                var newToken = await TryRefreshTokenAsync();
                if (newToken is not null)
                {
                    response = await _js.InvokeAsync<BrowserUploadResponse>(
                        "naraUpload.send", handle, url, newToken, fields, proxy);
                }
            }

            return response;
        }

        public async Task AbortAsync(string handle)
            => await _js.InvokeVoidAsync("naraUpload.abort", handle);

        public async Task ReleaseAsync(string handle)
            => await _js.InvokeVoidAsync("naraUpload.release", handle);

        // ------------------------------------------------------------ دانلود

        public async Task<bool> DownloadAsync(
            string relativeUrl,
            string fallbackName,
            string downloadId,
            Action<int, long, long>? onProgress = null)
        {
            var url = BuildUrl(relativeUrl);
            var token = await _tokenService.GetAccessTokenAsync();

            // اگر کسی درصد نخواهد، null می‌فرستیم و JS بدون گزارش ادامه می‌دهد
            using var proxy = onProgress is null
                ? null
                : DotNetObjectReference.Create(new ProgressProxy(onProgress));

            var result = await _js.InvokeAsync<DownloadResult>(
                "naraUpload.download", url, token, fallbackName, proxy, downloadId);

            if (result.Status == 401)
            {
                var newToken = await TryRefreshTokenAsync();
                if (newToken is not null)
                {
                    result = await _js.InvokeAsync<DownloadResult>(
                        "naraUpload.download", url, newToken, fallbackName, proxy, downloadId);
                }
            }

            return result.Ok;
        }

        public async Task AbortDownloadAsync(string downloadId)
            => await _js.InvokeVoidAsync("naraUpload.abortDownload", downloadId);

        // ------------------------------------------------------ درگ‌اند‌دراپ

        public async Task<bool> InitDropZoneAsync<T>(
            string zoneElementId, DotNetObjectReference<T> componentRef) where T : class
            => await _js.InvokeAsync<bool>("naraDropZone.init", zoneElementId, componentRef);

        public async Task DisposeDropZoneAsync(string zoneElementId)
            => await _js.InvokeVoidAsync("naraDropZone.dispose", zoneElementId);

        // ------------------------------------------------------------ کمکی

        private static string BuildUrl(string relativeUrl)
            => SiteSettings.ApiUrl.TrimEnd('/') + "/" + relativeUrl.TrimStart('/');

        private async Task<string?> TryRefreshTokenAsync()
        {
            var refreshToken = await _tokenService.GetReffreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken)) return null;

            var newToken = await _authService.RefreshTokenAsync(refreshToken);
            return string.IsNullOrEmpty(newToken) ? null : newToken;
        }

        private sealed class ProgressProxy(Action<int, long, long> callback)
        {
            private readonly Action<int, long, long> _callback = callback;

            [JSInvokable]
            public void OnUploadProgress(int percent, long loaded, long total)
                => _callback(percent, loaded, total);

            [JSInvokable]
            public void OnDownloadProgress(int percent, long loaded, long total)
                => _callback(percent, loaded, total);
        }
    }
}