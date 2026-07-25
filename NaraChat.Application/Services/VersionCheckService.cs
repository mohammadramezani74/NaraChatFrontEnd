

using Microsoft.JSInterop;
using System.Globalization;
using System.Net.Http.Json;

namespace NaraChat.Application.Services;

public class VersionCheckService
{
    private readonly IJSRuntime _js;
    private const string baseversion = "1.2.3";
    public string? LatestVersion { get; private set; }

    public VersionCheckService( IJSRuntime js)
    {
      
        _js = js;
    }

    public async Task<bool> IsNewVersionAvailableAsync()
    {
        try
        {
            LatestVersion = baseversion;

            var storedVersion = await _js.InvokeAsync<string>("localStorage.getItem", "app_version");

            if (string.IsNullOrEmpty(storedVersion) || storedVersion != LatestVersion)
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task StoreCurrentVersionAsync()
    {
        if (!string.IsNullOrEmpty(LatestVersion))
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "app_version", LatestVersion);
        }
    }

    private class VersionInfo
    {
        public string Version { get; set; } = string.Empty;
    }
}

