

namespace NaraChat.Contract.Models.Chat.Conversation;

public sealed class EditedMessageDto
{
    public Guid Id { get; set; }
    public string Message { get; set; }
    public Guid OtherId { get; set; }
    public Guid? ChannelId { get; set; }
    public EditedMessageDto(Guid id, string message, Guid? channelId=null)
    {
        Id = id;
        Message = message;
        ChannelId = channelId;

    }
}
