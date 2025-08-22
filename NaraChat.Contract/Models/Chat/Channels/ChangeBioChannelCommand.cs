namespace NaraChat.Contract.Models.Chat.Channels
{
    public sealed record ChangeBioChannelCommand(
      Guid ChannelId,
      string bio
      );
}
