

namespace NaraChat.Contract.Models.Chat.Conversation;

public sealed class EditedMessageDto
{
    public Guid Id { get; set; }
    public string Message { get; set; }
    public Guid OtherId { get; set; }
    public EditedMessageDto(Guid id, string message)
    {
        Id = id;
        Message = message;
    }
}
