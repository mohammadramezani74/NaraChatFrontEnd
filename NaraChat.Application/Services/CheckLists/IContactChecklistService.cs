using NaraChat.Contract.Models.Checklist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaraChat.Application.Services.CheckLists
{
    public interface IContactChecklistService
    {
        Task<ContactChecklistData> GetChecklistAsync(string appUserId, string contactId, CancellationToken cancellationToken = default);
        Task SaveChecklistAsync(string appUserId, string contactId, ContactChecklistData data, CancellationToken cancellationToken = default);
        Task<ContactChecklistUiState> GetUiStateAsync(string appUserId, string contactId, CancellationToken cancellationToken = default);
        Task SaveUiStateAsync(string appUserId, string contactId, ContactChecklistUiState state, CancellationToken cancellationToken = default);
    }
}
