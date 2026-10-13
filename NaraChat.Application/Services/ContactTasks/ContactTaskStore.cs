using System.Text.Json;
using Microsoft.JSInterop;
using NaraChat.Application.Services.LocalStorage;
using NaraChat.Contract.Models.ContactTasks;

namespace NaraChat.Application.Services.ContactTasks
{
    /// <summary>
    /// وضعیت مشترک کارها بین ستون چت‌ها، سربرگ چت و پنل کارها.
    /// در Blazor WebAssembly سرویس Scoped عملاً یک نمونه برای کل برنامه است.
    /// </summary>
    public sealed class ContactTaskStore(
        IContactTaskService service,
        ILocalStorageService localStorage,
        IJSRuntime js)
    {
        private const string LegacyPrefix = "ne.checklist.";
        private const string LegacyUiPrefix = "ne.checklist.ui.";

        private readonly Dictionary<Guid, ContactTaskCountDto> _counts = new();
        private readonly Dictionary<Guid, HashSet<Guid>> _linkedMessages = new();
        private bool _legacyChecked;

        /// <summary>
        /// چیزی عوض شد. آرگومان: مخاطبی که فهرستش عوض شده (null یعنی فقط شمارش‌ها).
        /// </summary>
        public event Action<Guid?>? Changed;

        /// <summary>از دستگاه یا تب دیگری تغییری رسید؛ پنل باز باید دوباره بخواند.</summary>
        public event Action<Guid>? RemoteChanged;

        /// <summary>درخواست باز شدن پنل (مثلاً بعد از افزودن کار از روی پیام).</summary>
        public event Action? OpenRequested;

        // -------------------------------------------------------- شمارش‌ها

        public int OpenCount(Guid contactId)
            => _counts.TryGetValue(contactId, out var c) ? c.OpenCount : 0;

        public int OverdueCount(Guid contactId)
            => _counts.TryGetValue(contactId, out var c) ? c.OverdueCount : 0;

        public bool CountsLoaded { get; private set; }

        public async Task RefreshCountsAsync()
        {
            var counts = await service.GetCountsAsync();
            if (counts is null)
                return;

            CountsLoaded = true;

            _counts.Clear();
            foreach (var c in counts)
                _counts[c.ContactId] = c;

            Changed?.Invoke(null);
        }

        /// <summary>پنل بعد از هر تغییر، شمارش همان مخاطب را مستقیم به‌روز می‌کند.</summary>
        public void SetSnapshot(Guid contactId, IEnumerable<ContactTaskDto> open)
        {
            var today = DateTime.Today;
            var list = open.ToList();

            _counts[contactId] = new ContactTaskCountDto
            {
                ContactId = contactId,
                OpenCount = list.Count,
                OverdueCount = list.Count(t => t.DueDate is DateTime d && d.Date < today),
            };

            _linkedMessages[contactId] = list
                .Where(t => t.SourceMessageId is not null)
                .Select(t => t.SourceMessageId!.Value)
                .ToHashSet();

            Changed?.Invoke(contactId);
        }

        /// <summary>
        /// پیوند پیام‌ها به کارهای یک مخاطب را (اگر هنوز نداریم) از سرور می‌گیرد تا
        /// برچسب «در کارهای شما» روی پیام‌ها حتی وقتی پنل کارها باز نیست درست باشد.
        /// </summary>
        public async Task EnsureSnapshotAsync(Guid contactId)
        {
            if (_linkedMessages.ContainsKey(contactId))
                return;

            // وقتی شمارش‌ها رسیده و این مخاطب کار بازی ندارد، درخواستی لازم نیست.
            if (CountsLoaded && OpenCount(contactId) == 0)
            {
                _linkedMessages[contactId] = new HashSet<Guid>();
                return;
            }

            var list = await service.GetListAsync(contactId);
            if (list is not null)
                SetSnapshot(contactId, list.Open);
        }

        /// <summary>آیا این پیام به یکی از کارهای باز این مخاطب پیوند خورده است؟</summary>
        public bool IsLinked(Guid contactId, Guid messageId)
            => _linkedMessages.TryGetValue(contactId, out var set) && set.Contains(messageId);

        public void NotifyRemote(Guid contactId)
        {
            // پیوندهای کش‌شده‌ی این مخاطب کهنه شده‌اند.
            if (_linkedMessages.Remove(contactId))
                _ = EnsureSnapshotAsync(contactId);

            RemoteChanged?.Invoke(contactId);
            _ = RefreshCountsAsync();
        }

        private bool _openPending;

        /// <summary>
        /// کارهای مخاطب باز شود: روی دسکتاپ تب «کارها» در ستون کناری، روی موبایل
        /// برگه‌ی پایین. اگر صفحه‌ی چت هنوز ساخته نشده، درخواست می‌ماند تا بسازد.
        /// </summary>
        public void RequestOpen()
        {
            _openPending = true;
            SideTab = ContactSideTab.Tasks;
            OpenRequested?.Invoke();
        }

        public bool ConsumeOpenRequest()
        {
            var pending = _openPending;
            _openPending = false;
            return pending;
        }

        // --------------------------------------------- ستون کناری دسکتاپ

        private ContactSideTab _sideTab = ContactSideTab.Info;

        /// <summary>
        /// کدام تب ستون کناری دسکتاپ نشان داده شود. در طول یک نشست می‌ماند تا
        /// کسی که سراغ کارها رفته با عوض کردن چت دوباره به «مشخصات» پرت نشود.
        /// </summary>
        public ContactSideTab SideTab
        {
            get => _sideTab;
            set
            {
                if (_sideTab == value)
                    return;

                _sideTab = value;
                Changed?.Invoke(null);
            }
        }

        /// <summary>پرش به پیام منبع یک کار (پنل ممکن است بیرون از ChatDetail باشد).</summary>
        public event Action<Guid>? JumpRequested;

        public void RequestJump(Guid messageId) => JumpRequested?.Invoke(messageId);

        /// <summary>یادآوری یک کار رسید.</summary>
        public event Action<TaskReminderDto>? ReminderReceived;

        public void RaiseReminder(TaskReminderDto reminder)
        {
            ReminderReceived?.Invoke(reminder);
            NotifyRemote(reminder.ContactId);
        }

        // ---------------------------------------- انتقال چک‌لیست قدیمی

        /// <summary>
        /// چک‌لیست قدیمی فقط در localStorage همین مرورگر بود
        /// (کلید ne.checklist.{کاربر}.{مخاطب}). یک بار در هر بار باز شدن برنامه
        /// همه را به سرور می‌فرستد و بعد از موفقیت کلید را پاک می‌کند. سرور
        /// شناسه‌ها را حفظ می‌کند، پس اجرای دوباره مورد تکراری نمی‌سازد.
        /// </summary>
        public async Task ImportLegacyAsync(Guid me)
        {
            if (_legacyChecked || me == Guid.Empty)
                return;
            _legacyChecked = true;

            string[] keys;
            try
            {
                keys = await js.InvokeAsync<string[]>("naraLocalKeys", LegacyPrefix) ?? Array.Empty<string>();
            }
            catch (Exception)
            {
                // site.js قدیمی در کش مرورگر؛ دفعه‌ی بعد انجام می‌شود.
                _legacyChecked = false;
                return;
            }

            var imported = 0;

            foreach (var key in keys)
            {
                if (key.StartsWith(LegacyUiPrefix, StringComparison.Ordinal))
                    continue;

                // ne.checklist.{appUserId}.{contactId}
                // اگر بخش کاربر شناسه‌ی کاربر دیگری باشد (حساب دیگری روی همین
                // مرورگر) رد می‌شود؛ اگر اصلاً Guid نباشد، مال همین مرورگر فرض می‌شود.
                var rest = key[LegacyPrefix.Length..];
                var dot = rest.LastIndexOf('.');
                if (dot <= 0 || !Guid.TryParse(rest[(dot + 1)..], out var contactId) || contactId == me)
                    continue;

                var ownerPart = rest[..dot];
                if (Guid.TryParse(ownerPart, out var ownerId) && ownerId != me)
                    continue;

                var items = await ReadLegacyAsync(key);
                if (items is null)
                    continue;

                if (items.Count == 0)
                {
                    await RemoveLegacyAsync(key, rest);
                    continue;
                }

                var (ok, count) = await service.ImportAsync(contactId, items);
                if (!ok)
                    continue;

                imported += count;
                await RemoveLegacyAsync(key, rest);
            }

            if (imported > 0)
                await RefreshCountsAsync();
        }

        private async Task<List<ImportContactTaskItem>?> ReadLegacyAsync(string key)
        {
            try
            {
                var payload = await localStorage.GetItemAsync(key);
                if (string.IsNullOrWhiteSpace(payload))
                    return new List<ImportContactTaskItem>();

                using var doc = JsonDocument.Parse(payload);
                if (!doc.RootElement.TryGetProperty("items", out var arr) || arr.ValueKind != JsonValueKind.Array)
                    return new List<ImportContactTaskItem>();

                var result = new List<ImportContactTaskItem>();
                foreach (var el in arr.EnumerateArray())
                {
                    var text = el.TryGetProperty("text", out var t) ? t.GetString() : null;
                    if (string.IsNullOrWhiteSpace(text))
                        continue;

                    if (!el.TryGetProperty("id", out var idEl) || !Guid.TryParse(idEl.GetString(), out var id))
                        continue;

                    // برچسب قدیمی جای جدایی ندارد؛ به انتهای متن اضافه می‌شود تا گم نشود.
                    var tag = el.TryGetProperty("tag", out var tg) ? tg.GetString() : null;
                    if (!string.IsNullOrWhiteSpace(tag))
                        text = $"{text.Trim()} #{tag.Trim()}";

                    DateTime? due = null;
                    if (el.TryGetProperty("dueDate", out var d) && d.ValueKind == JsonValueKind.String
                        && DateTime.TryParse(d.GetString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
                        due = parsed.Kind == DateTimeKind.Utc ? parsed.ToLocalTime().Date : parsed.Date;

                    DateTime? created = null;
                    if (el.TryGetProperty("createdAt", out var c) && c.ValueKind == JsonValueKind.String
                        && DateTime.TryParse(c.GetString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out var createdAt))
                        created = createdAt;

                    result.Add(new ImportContactTaskItem
                    {
                        Id = id,
                        Text = text,
                        IsDone = el.TryGetProperty("isDone", out var done) && done.ValueKind == JsonValueKind.True,
                        DueDate = due?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                        CreatedAt = created,
                        Order = el.TryGetProperty("order", out var o) && o.TryGetInt32(out var order) ? order : 0,
                    });
                }

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task RemoveLegacyAsync(string key, string rest)
        {
            try
            {
                await localStorage.RemoveItemAsync(key);
                await localStorage.RemoveItemAsync(LegacyUiPrefix + rest);
            }
            catch (Exception) { }
        }
    }
}
