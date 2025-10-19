namespace NaraChat.Contract.Models.Checklist
{
    public sealed class ContactChecklistItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Text { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Tag { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Order { get; set; }
    }
}
