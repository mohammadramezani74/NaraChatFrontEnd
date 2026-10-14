using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using NaraChat.Application.Services.Bots;
using NaraChat.Contract.Models.Bots;
using NaraChat.Contract.Models.Chat.Conversation;

namespace NaraChatFrontEnd.Pages.ChatPages;

/// <summary>
/// رفتار چت با ربات: صفحه‌ی «شروع»، منوی دستورهای «/»، کیبورد ربات و به‌روز شدن
/// پیام‌هایی که ربات عوض می‌کند. پیام متنی به ربات از همان SendMessage معمولی می‌رود.
/// </summary>
public partial class ChatDetail
{
    [Inject] private IBotService BotService { get; set; } = default!;
    [Inject] private BotStore BotStore { get; set; } = default!;

    private BotInfoDto? _botInfo;
    private BotStateDto? _botState;
    private Guid? _botInfoFor;
    private bool _botStarting;
    private bool _botKeyboardHidden;
    private bool _botStoreHooked;

    private bool IsBotChat => OtherUser?.IsBot == true && SelectedChannel is null && SelectedGroup is null;

    private bool BotStarted => _botInfo?.Started == true;

    private List<List<string>>? BotKeyboard => _botState?.Keyboard;

    private string BotPlaceholder => string.IsNullOrWhiteSpace(_botState?.Placeholder)
        ? "پیام یا دستور…"
        : _botState!.Placeholder!;

    private void HookBotStore()
    {
        if (_botStoreHooked) return;
        _botStoreHooked = true;
        BotStore.StateChanged += OnBotStateChanged;
        BotStore.MessageUpdated += OnBotMessageUpdated;
    }

    private void UnhookBotStore()
    {
        if (!_botStoreHooked) return;
        _botStoreHooked = false;
        BotStore.StateChanged -= OnBotStateChanged;
        BotStore.MessageUpdated -= OnBotMessageUpdated;
    }

    /// <summary>بعد از باز شدن یک گفتگوی خصوصی: اگر طرف ربات است، وضعیتش خوانده می‌شود.</summary>
    private async Task LoadBotInfoAsync()
    {
        HookBotStore();

        if (!IsBotChat)
        {
            _botInfo = null;
            _botState = null;
            _botInfoFor = null;
            return;
        }

        var botId = OtherUser!.Id;
        _botInfoFor = botId;
        _botKeyboardHidden = false;

        var info = await BotService.GetInfoAsync(botId);
        if (_botInfoFor != botId)
            return; // در این فاصله چت عوض شده

        _botInfo = info;
        _botState = info is null
            ? null
            : new BotStateDto
            {
                BotUserId = botId,
                Started = info.Started,
                Keyboard = info.Keyboard,
                Placeholder = info.Placeholder,
            };
    }

    private void OnBotStateChanged(Guid botUserId)
    {
        if (!IsBotChat || OtherUser!.Id != botUserId)
            return;

        _botState = BotStore.Get(botUserId);
        if (_botInfo is not null && _botState?.Started == true)
            _botInfo.Started = true;

        _botKeyboardHidden = false;
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnBotMessageUpdated(ChatMessageDto updated)
    {
        var target = _messages.FirstOrDefault(m => m.Id == updated.Id);
        if (target is null)
            return;

        target.Content = updated.Content;
        target.ReplyMarkup = updated.ReplyMarkup;
        _ = InvokeAsync(StateHasChanged);
    }

    private async Task StartBotAsync()
    {
        if (!IsBotChat || _botStarting)
            return;

        _botStarting = true;
        StateHasChanged();
        try
        {
            var (ok, result, message) = await BotService.StartAsync(OtherUser!.Id);
            if (!ok || result is null)
            {
                snackbar.Add(message, Severity.Warning);
                return;
            }

            if (_botInfo is not null)
                _botInfo.Started = true;

            if (Conversation is null || Conversation.id != result.ConversationId)
            {
                // گفتگو تازه ساخته شد؛ پیام‌ها از سرور خوانده می‌شوند.
                await LoadMesages();
                return;
            }

            if (_messages.All(m => m.Id != result.MessageId))
            {
                _messages.Add(new ChatMessageDto
                {
                    Id = result.MessageId,
                    ScopeId = result.ConversationId,
                    Content = "شروع",
                    IsMine = true,
                    SendAt = result.SentAt,
                    SenderName = CurrentUser?.Name,
                    UserId = CurrentUser?.Id ?? Guid.Empty,
                    ConversationType = ConversationType.Private,
                });

                // جواب ربات ممکن است زودتر از جواب این درخواست رسیده باشد.
                _messages.Sort((a, b) => a.SendAt.CompareTo(b.SendAt));
            }

            NeedScrollToBottom();
        }
        finally
        {
            _botStarting = false;
            StateHasChanged();
        }
    }

    /// <summary>دکمه‌ی کیبورد ربات یا دستور «/»: متنش مثل یک پیام عادی فرستاده می‌شود.</summary>
    private async Task SendBotTextAsync(string? text)
    {
        if (string.IsNullOrWhiteSpace(text) || !IsBotChat)
            return;

        NewMessage = text.Trim();
        await SendMessage();
    }

    private async Task OnBotKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
            await SendBotTextAsync(NewMessage);
    }
}
