namespace NaraChat.Contract.Models.Admin
{
    // شکل /api/v1/adminbroadcasts (AdminPanel/Broadcasts.cs در بک‌اند).

    public sealed class BroadcastAudience
    {
        /// <summary>all | roles | cities | users</summary>
        public string Mode { get; set; } = "all";
        public List<Guid>? RoleIds { get; set; }
        public List<string>? Cities { get; set; }
        public List<Guid>? UserIds { get; set; }
    }

    public sealed class BroadcastAudiencePreview
    {
        public int Count { get; set; }
        public List<string> Sample { get; set; } = new();
        public string Label { get; set; } = string.Empty;
    }

    public sealed class CreateBroadcastRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        /// <summary>info | urgent | reminder</summary>
        public string Kind { get; set; } = "info";
        public string? ButtonText { get; set; }
        public string? ButtonUrl { get; set; }
        public BroadcastAudience Audience { get; set; } = new();
        /// <summary>null = همین الان</summary>
        public DateTime? ScheduledAt { get; set; }
    }

    public sealed class BroadcastRow
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Kind { get; set; } = "info";
        public string? ButtonText { get; set; }
        public string? ButtonUrl { get; set; }
        public string AudienceLabel { get; set; } = string.Empty;
        /// <summary>scheduled | sending | sent | cancelled | failed</summary>
        public string Status { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int Total { get; set; }
        public int Delivered { get; set; }
        public int Seen { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public string? Error { get; set; }
    }

    public sealed class BroadcastsPage
    {
        public List<BroadcastRow> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string BotName { get; set; } = string.Empty;
        public Guid? BotUserId { get; set; }
    }
}
