using NaraChat.Contract.Models.ContactTasks;

namespace NaraChat.Application.Services.ContactTasks
{
    /// <summary>
    /// دسترسی به API کارهای شخصی. هیچ متدی استثنا بیرون نمی‌دهد؛ خطا با
    /// Ok=false و یک پیام فارسی برمی‌گردد.
    /// </summary>
    public interface IContactTaskService
    {
        Task<List<ContactTaskCountDto>?> GetCountsAsync(CancellationToken ct = default);
        Task<ContactTaskListDto?> GetListAsync(Guid contactId, CancellationToken ct = default);
        Task<ContactTaskResult> CreateAsync(Guid contactId, string text, DateTime? dueDate, Guid? sourceMessageId = null, DateTimeOffset? remindAt = null, CancellationToken ct = default);
        Task<ContactTaskResult> UpdateAsync(Guid id, string text, DateTime? dueDate, DateTimeOffset? remindAt, CancellationToken ct = default);
        Task<ContactTaskResult> SetDoneAsync(Guid id, bool done, CancellationToken ct = default);
        Task<(bool Ok, string Message)> DeleteAsync(Guid id, CancellationToken ct = default);
        Task<(bool Ok, string Message)> ClearDoneAsync(Guid contactId, CancellationToken ct = default);
        Task<(bool Ok, int Imported)> ImportAsync(Guid contactId, List<ImportContactTaskItem> items, CancellationToken ct = default);
    }

    public sealed record ContactTaskResult(bool Ok, ContactTaskDto? Task, string Message);
}
