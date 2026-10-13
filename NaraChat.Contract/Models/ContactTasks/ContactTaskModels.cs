namespace NaraChat.Contract.Models.ContactTasks
{
    public sealed class ContactTaskDto
    {
        public Guid Id { get; set; }
        public Guid ContactId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public bool IsDone { get; set; }
        public DateTime? DoneAt { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? SourceMessageId { get; set; }
        public string? SourcePreview { get; set; }
        public DateTime? SourceSentAt { get; set; }

        /// <summary>زمان یادآوری؛ با آفست می‌آید و با ToLocalTime به ساعت مرورگر تبدیل می‌شود.</summary>
        public DateTimeOffset? RemindAt { get; set; }

        /// <summary>اعلان یادآوری فرستاده شده است.</summary>
        public bool ReminderSent { get; set; }
    }

    /// <summary>رویداد هاب وقتی زمان یادآوری یک کار می‌رسد.</summary>
    public sealed class TaskReminderDto
    {
        public Guid TaskId { get; set; }
        public Guid ContactId { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTimeOffset RemindAt { get; set; }
    }

    /// <summary>تب ستون کناری دسکتاپ: مشخصات مخاطب یا کارها.</summary>
    public enum ContactSideTab
    {
        Info = 0,
        Tasks = 1,
    }

    public sealed class ContactTaskListDto
    {
        public Guid ContactId { get; set; }
        public List<ContactTaskDto> Open { get; set; } = new();
        public List<ContactTaskDto> Done { get; set; } = new();
        public int DoneCount { get; set; }
    }

    public sealed class ContactTaskCountDto
    {
        public Guid ContactId { get; set; }
        public int OpenCount { get; set; }
        public int OverdueCount { get; set; }
    }

    public sealed class ImportContactTaskItem
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public string? DueDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int Order { get; set; }
    }

    /// <summary>گروه‌بندی کارهای باز بر اساس سررسید، به همان ترتیبی که در پنل نشان داده می‌شوند.</summary>
    public enum ContactTaskBucket
    {
        Overdue = 0,
        Today = 1,
        Later = 2,
        NoDate = 3,
    }
}
