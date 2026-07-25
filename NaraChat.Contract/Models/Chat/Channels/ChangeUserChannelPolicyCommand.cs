namespace NaraChat.Contract.Models.Chat.Channels
{
    public sealed record ChangeUserChannelPolicyCommand(Guid channelId, Guid UserId, bool ispromote);
    public sealed record ChangeUserGroupPolicyCommand(Guid conversationId, Guid UserId, bool ispromote);

}
