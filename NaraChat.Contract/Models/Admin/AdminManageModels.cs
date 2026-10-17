namespace NaraChat.Contract.Models.Admin
{
    // شکل جواب‌های /api/v1/adminusers، /adminroles و /adminrooms (AdminPanel در بک‌اند).

    public sealed class AdminRoleRef
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class AdminRoleInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Members { get; set; }
    }

    public sealed class AdminUserRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Post { get; set; }
        public string? City { get; set; }
        public string PhoneMasked { get; set; } = string.Empty;
        public List<AdminRoleRef> Roles { get; set; } = new();
        public DateTime? LastSeen { get; set; }
        public bool Online { get; set; }
        public int Messages7d { get; set; }
        public bool IsDisabled { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsAdmin { get; set; }
    }

    public sealed class AdminUsersPage
    {
        public List<AdminUserRow> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<string> Cities { get; set; } = new();
        public int AllCount { get; set; }
        public int ActiveCount { get; set; }
        public int QuietCount { get; set; }
        public int DisabledCount { get; set; }
    }

    public sealed class AdminMembership
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        /// <summary>group | channel | bot</summary>
        public string Kind { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsArchived { get; set; }
    }

    public sealed class AdminAuditItem
    {
        public DateTime At { get; set; }
        public string ActorName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public string? Details { get; set; }
    }

    public sealed class AdminUserDetail
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Post { get; set; }
        public string? City { get; set; }
        public string PhoneMasked { get; set; } = string.Empty;
        public List<AdminRoleRef> Roles { get; set; } = new();
        public DateTime? LastSeen { get; set; }
        public bool Online { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int Messages7d { get; set; }
        public int MessagesTotal { get; set; }
        public bool IsDisabled { get; set; }
        public DateTime? DisabledAt { get; set; }
        public bool IsAdmin { get; set; }
        public List<AdminMembership> Memberships { get; set; } = new();
        public List<AdminAuditItem> Log { get; set; } = new();
        /// <summary>نامی که اول نمایش داده می‌شود.</summary>
        public string NameFirst { get; set; } = string.Empty;
        public string NameLast { get; set; } = string.Empty;
    }

    public sealed class AdminRoomRow
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Members { get; set; }
        public int Messages7d { get; set; }
        public DateTime? LastActivity { get; set; }
        public string? CreatorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsArchived { get; set; }
    }

    public sealed class AdminRoomsPage
    {
        public List<AdminRoomRow> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int AllCount { get; set; }
        public int ArchivedCount { get; set; }
    }

    public sealed class AdminRoomMember
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsCreator { get; set; }
        public bool IsDisabled { get; set; }
    }

    public sealed class AdminRoomDetail
    {
        public Guid Id { get; set; }
        public string Kind { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CreatorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public DateTime? LastActivity { get; set; }
        public int Messages7d { get; set; }
        public int MessagesTotal { get; set; }
        public List<int> Daily14 { get; set; } = new();
        public List<AdminRoomMember> Members { get; set; } = new();
        public List<AdminAuditItem> Log { get; set; } = new();
    }
}
