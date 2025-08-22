namespace NaraChat.Contract.Models.Chat.Channels
{
    public sealed record ChangeUserChannelPolicyCommand(Guid channelId, Guid UserId, bool ispromote);
}
