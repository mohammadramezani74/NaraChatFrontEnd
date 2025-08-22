namespace NaraChat.Contract.Models.Chat.Channels
{
    public class UserChannelDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
        public bool Equals(UserChannelDto other) => other is not null && Id == other.Id;
        public override bool Equals(object obj) => Equals(obj as UserChannelDto);
        public override int GetHashCode() => Id.GetHashCode();
    }
}
