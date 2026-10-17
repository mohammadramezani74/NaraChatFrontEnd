namespace NaraChat.Contract.Models.Admin
{
    // شکل جواب /api/v1/admindashboard (AdminPanel/AdminDashboard.cs در بک‌اند).

    public sealed class AdminDashboardDto
    {
        public DashboardKpis Kpis { get; set; } = new();
        public List<DashboardDayPoint> Trend { get; set; } = new();
        public List<DashboardSlice> UsersByCity { get; set; } = new();
        public DashboardActiveSplit Activity7Days { get; set; } = new();
        public List<DashboardSlice> MessageTypes { get; set; } = new();
        public List<DashboardTopItem> TopUsers { get; set; } = new();
        public List<DashboardTopItem> TopGroups { get; set; } = new();
        public List<DashboardTopItem> TopChannels { get; set; } = new();
        public List<DashboardActivity> Recent { get; set; } = new();
        public DashboardBots Bots { get; set; } = new();
        public DashboardOnline Online { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public sealed class DashboardKpis
    {
        public int Messages24h { get; set; }
        public int MessagesPrev24h { get; set; }
        public int Users { get; set; }
        public int NewUsersWeek { get; set; }
        public int Groups { get; set; }
        public int NewGroupsWeek { get; set; }
        public int Channels { get; set; }
        public int NewChannelsWeek { get; set; }
    }

    public sealed class DashboardDayPoint
    {
        public DateTime Day { get; set; }
        public int Private { get; set; }
        public int Group { get; set; }
        public int Channel { get; set; }
    }

    public sealed class DashboardSlice
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public sealed class DashboardActiveSplit
    {
        public int Active { get; set; }
        public int Inactive { get; set; }
    }

    public sealed class DashboardTopItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public sealed class DashboardActivity
    {
        /// <summary>user | group | channel | bot</summary>
        public string Kind { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public DateTime At { get; set; }
    }

    public sealed class DashboardBots
    {
        public int Count { get; set; }
        public int Enabled { get; set; }
        public int StartedUsers { get; set; }
        public int MessagesToday { get; set; }
    }

    public sealed class DashboardOnline
    {
        public int Count { get; set; }
        public List<DashboardOnlineUser> Users { get; set; } = new();
    }

    public sealed class DashboardOnlineUser
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
