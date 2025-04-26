

namespace NaraChat.Contract.Models.Chat.Conversation;

public sealed class NotificationModelDto
{
    public string Name { get; set; }
    public string Avatar { get; set; }
    public string Message { get; set; }
    public string Url { get; set; }

    public NotificationModelDto(string name, string avatar, string message, string url)
    {
        Name = name;
        Avatar = avatar;
        Message = message;
        Url = url;
    }
}
