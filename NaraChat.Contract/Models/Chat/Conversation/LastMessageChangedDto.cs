using System;

namespace NaraChat.Contract.Models.Chat.Conversation
{
    /// <summary>
    /// آخرین پیام یک گفتگو عوض شده — با حذف پیام یا پاک کردن تاریخچه.
    ///
    /// ScopeId همان کلید مسیریابی پیام‌هاست: شناسه‌ی گفتگو برای خصوصی و گروه،
    /// شناسه‌ی کانال برای کانال.
    /// </summary>
    public sealed class LastMessageChangedDto
    {
        public Guid ScopeId { get; set; }

        /// <summary>شناسه‌ی آخرین پیام باقی‌مانده؛ اگر پیامی نمانده null است.</summary>
        public Guid? MessageId { get; set; }

        /// <summary>متن لیست مکالمات؛ اگر پیامی نمانده خالی است.</summary>
        public string Text { get; set; } = string.Empty;

        public DateTime? SentAt { get; set; }
    }
}
