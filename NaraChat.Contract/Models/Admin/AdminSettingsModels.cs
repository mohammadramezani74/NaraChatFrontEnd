namespace NaraChat.Contract.Models.Admin
{
    /// <summary>اعلان بالای برنامه. Tone: info | warn | success</summary>
    public sealed class AnnouncementSetting
    {
        public bool Enabled { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Tone { get; set; } = "info";
        public string? LinkText { get; set; }
        public string? LinkUrl { get; set; }
        public DateTime? Until { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public AnnouncementSetting Clone() => (AnnouncementSetting)MemberwiseClone();
    }

    /// <summary>Groups/Channels: all | roles</summary>
    public sealed class CreationSetting
    {
        public string Groups { get; set; } = "all";
        public List<Guid> GroupRoleIds { get; set; } = new();
        public string Channels { get; set; } = "all";
        public List<Guid> ChannelRoleIds { get; set; } = new();

        public CreationSetting Clone() => new()
        {
            Groups = Groups,
            GroupRoleIds = GroupRoleIds.ToList(),
            Channels = Channels,
            ChannelRoleIds = ChannelRoleIds.ToList(),
        };
    }

    public sealed class AdminSettingsDto
    {
        public AnnouncementSetting Announcement { get; set; } = new();
        public CreationSetting Creation { get; set; } = new();
        public List<string> AdminNames { get; set; } = new();
        public DateTime? AnnouncementUpdatedAt { get; set; }
        public string? AnnouncementUpdatedBy { get; set; }
    }

    public sealed class AppInfoDto
    {
        public AnnouncementSetting? Announcement { get; set; }
        public bool CanCreateGroup { get; set; } = true;
        public bool CanCreateChannel { get; set; } = true;
    }
}
