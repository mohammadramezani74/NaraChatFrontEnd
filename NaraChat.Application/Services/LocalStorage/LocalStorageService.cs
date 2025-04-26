using Microsoft.JSInterop;

namespace NaraChat.Application.Services.LocalStorage
{
    public sealed class LocalStorageService(IJSRuntime runtime) : ILocalStorageService
    {
        private readonly IJSRuntime _runtime=runtime;

        public async Task<string?> GetItemAsync(string key)
        {
            return await _runtime.InvokeAsync<string>("localStorage.getItem", key);
        }

        public async Task RemoveItemAsync(string key)
        {
            await _runtime.InvokeVoidAsync("localStorage.removeItem", key);
        }

        public async Task SetItemAsync(string key, string value)
        {
            await _runtime.InvokeVoidAsync("localStorage.setItem", key, value);
        }
    }
}
