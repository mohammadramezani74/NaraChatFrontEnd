namespace NaraChat.Contract.Models.Checklist
{
    public sealed class ContactChecklistUiState
    {
        public bool IsPinned { get; set; } = true;
        public bool IsCollapsed { get; set; }
        public bool IsDetached { get; set; }
        public double? Left { get; set; }
        public double? Top { get; set; }
    }
}
