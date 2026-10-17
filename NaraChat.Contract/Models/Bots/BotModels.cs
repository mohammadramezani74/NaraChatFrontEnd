using System.Text.Json;

namespace NaraChat.Contract.Models.Bots
{
    // ======================================================================
    // پیام ربات: دکمه‌ها، کارت و یادداشت زیر متن (ChatMessageDto.ReplyMarkup)
    // ======================================================================

    public sealed class BotReplyMarkup
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        public List<List<BotButton>> Rows { get; set; } = new();
        public List<BotCardRow>? Card { get; set; }
        public string? Note { get; set; }

        /// <summary>ok (سبز) یا muted (خاکستری).</summary>
        public string? NoteTone { get; set; }

        public static BotReplyMarkup? Parse(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;
            try
            {
                return JsonSerializer.Deserialize<BotReplyMarkup>(json, Json);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }

    public sealed class BotButton
    {
        public string Text { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;

        /// <summary>null، primary یا back.</summary>
        public string? Style { get; set; }

        /// <summary>دکمه‌ی لینک (پیام همگانی)؛ به‌جای کلیک ربات، لینک در تب تازه باز می‌شود.</summary>
        public string? Url { get; set; }
    }

    public sealed class BotCardRow
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    // ======================================================================
    // ربات از دید کاربر
    // ======================================================================

    public sealed class BotCommandInfo
    {
        public string Command { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public sealed class BotInfoDto
    {
        public Guid BotUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Handle { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Avatar { get; set; }
        public bool IsEnabled { get; set; }
        public bool Started { get; set; }
        public Guid? ConversationId { get; set; }
        public List<BotCommandInfo> Commands { get; set; } = new();
        public List<List<string>>? Keyboard { get; set; }
        public string? Placeholder { get; set; }
    }

    /// <summary>رویداد هاب BotStateChanged: کیبورد ربات عوض شد.</summary>
    public sealed class BotStateDto
    {
        public Guid BotUserId { get; set; }
        public bool Started { get; set; }
        public List<List<string>>? Keyboard { get; set; }
        public string? Placeholder { get; set; }
    }

    public sealed class StartBotResult
    {
        public Guid ConversationId { get; set; }
        public Guid MessageId { get; set; }
        public DateTime SentAt { get; set; }
    }

    // ======================================================================
    // پنل ادمین
    // ======================================================================

    public enum BotAccessMode { All = 0, Roles = 1, Users = 2 }

    public enum BotMenuNodeKind { Menu = 0, Flow = 1 }

    public enum BotMenuDisplay { Inline = 0, Keyboard = 1 }

    public sealed class BotAdminListItem
    {
        public Guid Id { get; set; }
        public Guid BotUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Handle { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public int StartedCount { get; set; }
    }

    public sealed class BotCommandItem
    {
        public string Command { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid NodeId { get; set; }
    }

    public sealed class BotMenuNodeDto
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public int SortOrder { get; set; }
        public string Label { get; set; } = string.Empty;
        public string? Text { get; set; }
        public BotMenuNodeKind Kind { get; set; }
        public BotMenuDisplay Display { get; set; }
        public string? FlowKey { get; set; }
        public Dictionary<string, string>? Params { get; set; }
    }

    public sealed class BotAdminDetail
    {
        public Guid Id { get; set; }
        public Guid BotUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Handle { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Avatar { get; set; }
        public bool IsEnabled { get; set; }
        public BotAccessMode AccessMode { get; set; }
        public List<Guid> AccessList { get; set; } = new();
        public List<BotCommandItem> Commands { get; set; } = new();
        public List<BotMenuNodeDto> Nodes { get; set; } = new();
        public int StartedCount { get; set; }
    }

    public sealed class BotFlowParamInfo
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool Required { get; set; }
        public List<string>? Options { get; set; }
    }

    public sealed class BotFlowInfo
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<BotFlowParamInfo> Params { get; set; } = new();
    }

    public sealed class BotConnectorInfo
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public sealed class BotConnectorTestResult
    {
        public bool Ok { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // ------------------------------------------------ ارسال از بیرون (کلید API)

    public sealed class BotApiKeyItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
    }

    public sealed class BotApiSendItem
    {
        public DateTime At { get; set; }
        public string KeyName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        /// <summary>sending | sent | failed</summary>
        public string Status { get; set; } = string.Empty;

        public string? Error { get; set; }
        public string? ErrorText { get; set; }
        public string? Reference { get; set; }
        public string? Preview { get; set; }
    }

    public sealed class BotApiOverview
    {
        public bool IsOfficial { get; set; }
        public List<BotApiKeyItem> Keys { get; set; } = new();
        public List<BotApiSendItem> Recent { get; set; } = new();
        public int SentToday { get; set; }
        public int FailedToday { get; set; }
    }

    public sealed class CreatedBotApiKey
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
    }

    /// <summary>نقش‌ها (api/v1/roles/GetAll) و کاربرها (users/GetUsersForGroup).</summary>
    public sealed class BotPickItem
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}
