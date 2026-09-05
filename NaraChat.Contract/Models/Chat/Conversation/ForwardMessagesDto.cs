using System;
using System.Collections.Generic;

namespace NaraChat.Contract.Models.Chat.Conversation
{
    /// <summary>
    /// یک مقصد فوروارد.
    ///
    /// معنی Id به Type بستگی دارد و عمداً همان شناسه‌ای است که ردیف لیست کناری
    /// دارد، تا دیالوگ انتخاب مقصد لازم نباشد چیزی را ترجمه کند:
    ///   Private → شناسه‌ی کاربر مقابل
    ///   group   → شناسه‌ی گروه
    ///   Channel → شناسه‌ی کانال
    /// </summary>
    public sealed class ForwardTargetDto
    {
        public Guid Id { get; set; }
        public ConversationType Type { get; set; }
    }

    public sealed class ForwardMessagesDto
    {
        public List<Guid> MessageIds { get; set; } = new();
        public List<ForwardTargetDto> Targets { get; set; } = new();
    }
}
