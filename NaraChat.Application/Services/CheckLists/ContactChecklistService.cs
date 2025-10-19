using NaraChat.Application.Services.LocalStorage;
using NaraChat.Contract.Models.Checklist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NaraChat.Application.Services.CheckLists
{
    public sealed class ContactChecklistService(ILocalStorageService localStorage) : IContactChecklistService
    {
        private const string ChecklistPrefix = "ne.checklist";
        private const string UiPrefix = "ne.checklist.ui";
        private readonly ILocalStorageService _localStorage = localStorage;
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public async Task<ContactChecklistData> GetChecklistAsync(string appUserId, string contactId, CancellationToken cancellationToken = default)
        {
            var key = BuildKey(ChecklistPrefix, appUserId, contactId);
            var payload = await _localStorage.GetItemAsync(key);
            if (string.IsNullOrWhiteSpace(payload))
            {
                return new ContactChecklistData();
            }

            try
            {
                var data = JsonSerializer.Deserialize<ContactChecklistData>(payload, _serializerOptions);
                return data ?? new ContactChecklistData();
            }
            catch
            {
                await _localStorage.RemoveItemAsync(key);
                return new ContactChecklistData();
            }
        }

        public async Task SaveChecklistAsync(string appUserId, string contactId, ContactChecklistData data, CancellationToken cancellationToken = default)
        {
            var key = BuildKey(ChecklistPrefix, appUserId, contactId);
            data.LastUpdated = DateTime.UtcNow;
            var payload = JsonSerializer.Serialize(data, _serializerOptions);
            await _localStorage.SetItemAsync(key, payload);
        }

        public async Task<ContactChecklistUiState> GetUiStateAsync(string appUserId, string contactId, CancellationToken cancellationToken = default)
        {
            var key = BuildKey(UiPrefix, appUserId, contactId);
            var payload = await _localStorage.GetItemAsync(key);
            if (string.IsNullOrWhiteSpace(payload))
            {
                return new ContactChecklistUiState();
            }

            try
            {
                var state = JsonSerializer.Deserialize<ContactChecklistUiState>(payload, _serializerOptions);
                return state ?? new ContactChecklistUiState();
            }
            catch
            {
                await _localStorage.RemoveItemAsync(key);
                return new ContactChecklistUiState();
            }
        }

        public async Task SaveUiStateAsync(string appUserId, string contactId, ContactChecklistUiState state, CancellationToken cancellationToken = default)
        {
            var key = BuildKey(UiPrefix, appUserId, contactId);
            var payload = JsonSerializer.Serialize(state, _serializerOptions);
            await _localStorage.SetItemAsync(key, payload);
        }

        private static string BuildKey(string prefix, string appUserId, string contactId)
        {
            return $"{prefix}.{appUserId}.{contactId}";
        }
    }
}
