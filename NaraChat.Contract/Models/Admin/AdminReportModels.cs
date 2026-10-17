namespace NaraChat.Contract.Models.Admin
{
    // شکل /api/v1/adminreports (AdminPanel/Reports.cs در بک‌اند).

    public sealed class UsageDay
    {
        public DateTime Day { get; set; }
        public int Private { get; set; }
        public int Group { get; set; }
        public int Channel { get; set; }
        public int ActiveSenders { get; set; }
        public int BotMessages { get; set; }
        public int NewGroups { get; set; }
        public int NewChannels { get; set; }
        public int Total => Private + Group + Channel;
    }

    public sealed class UsageReport
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TotalMessages { get; set; }
        public int TotalPrivate { get; set; }
        public int TotalGroup { get; set; }
        public int TotalChannel { get; set; }
        public int ActiveSendersTotal { get; set; }
        public int AvgActiveSenders { get; set; }
        public int BotMessages { get; set; }
        public int NewGroups { get; set; }
        public int NewChannels { get; set; }
        public List<UsageDay> Days { get; set; } = new();
        public List<int> ByHour { get; set; } = new();
        /// <summary>شنبه اول.</summary>
        public List<int> ByWeekday { get; set; } = new();
        public List<DashboardSlice> ByCity { get; set; } = new();
    }

    public sealed class InactiveUserRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? City { get; set; }
        public string PhoneMasked { get; set; } = string.Empty;
        public DateTime? LastSeen { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public sealed class InactiveUsersPage
    {
        public List<InactiveUserRow> Items { get; set; } = new();
        public int Total { get; set; }
        public int Days { get; set; }
    }

    public sealed class AuditRow
    {
        public DateTime At { get; set; }
        public Guid ActorId { get; set; }
        public string ActorName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public Guid? TargetId { get; set; }
        public string TargetName { get; set; } = string.Empty;
        public string? Details { get; set; }
    }

    public sealed class AuditActor
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class AuditPage
    {
        public List<AuditRow> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<AuditActor> Actors { get; set; } = new();
    }
}
