using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using NaraChat.Application.Services;
using NaraChat.Application.Services.ChatServices.Conversation;
using NaraChat.Application.Services.Upload;
using NaraChat.Contract.Models.BaseResponse;
using NaraChat.Contract.Models.Chat.Conversation;
using NaraChat.Contract.Models.Users;
using NaraChat.Contract.Utilities.FilesExtensions;
using NaraChatFrontEnd.Models.BaseModels;
using NaraChatFrontEnd.Pages.ChatPages.Components.DialogComponents;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;
using static NaraChatFrontEnd.Pages.ChatPages.Components.DialogComponents.UploadDialogComponent;

namespace NaraChatFrontEnd.Pages.ChatPages;

public partial class ChatDetail : ComponentBase, IAsyncDisposable
{
    #region parameters & fields
    [Inject] private IBrowserUploader Uploader { get; set; } = default!;
    private DateTime? _oldestCursor;      // تاریخ قدیمی‌ترین پیامی که داریم
    private bool _hasMoreOlder = true;    // آیا صفحه‌ی قدیمی‌تری هست
    private bool _isLoadingOlder;
    [Parameter] public EventCallback<Guid> OnChatDeleted { get; set; }
    public int UploadPercent { get; set; }
    private List<PinnedMessageDto> _pinned = new();
    private int _pinnedIndex = 0;
    public string UploadSizeText { get; set; } = string.Empty;
    private string? _activeUploadHandle;
    [CascadingParameter(Name = "Theme")] public ThemeChanging ThemeCascading { get; set; }
    private MudTextField<string> messageInput;

    private CancellationTokenSource? _cancellationTokenSource;

    public bool Loading { get; set; } = false;
    private bool isRecording { get; set; } = false;
    private ElementReference _channelScrollHost;
    private ElementReference _chatScrollHost;
    private bool _boundToChannel;
    [Parameter]  public UserDto? SelectedChannel { get; set; } = null;
    [Parameter] public UserDto? SelectedGroup { get; set; } = null;

    private DotNetObjectReference<ChatDetail>? objRef;
    private DotNetObjectReference<ChatDetail>? objRef2;

    public bool IsUploading { get; set; } = false;
    public UserAvatar Avatars { get; set; } = new();

    [Parameter] public Guid id { get; set; }
    DateTime? lastDate = null;

    [Parameter, Display(Name = "ایونت سین پیام کاربر")]
    public EventCallback<MessageSeenDto> OnMessageSeen { get; set; }

    [Display(Name = "المنت بادی مسیج برای اسکرول")]
    private ElementReference _chatContainer;
    [Display(Name = "المنت بادی مسیج برای اسکرول")]
    private ElementReference _channelContainer;
    [Display(Name = "کاربر فعلی")]
    public UserDto? CurrentUser { get; set; }

    [Display(Name = "مکالمه انتخابی")]
    public PrivateConversationDto? Conversation { get; set; } = new();

    [Display(Name = "لیست پیام ها")]
    private readonly List<ChatMessageDto> _messages = new();

    [Display(Name = "کارد پیکر ایموجی")]
    private bool ShowEmojiPicker = false;

    [Parameter, Display(Name = "کاربر انتخاب شده")]
    public UserDto? OtherUser { get; set; } = null;

    [Parameter, Display(Name = "ایونت ارسال پیام جدید به ساید مقابل")]
    public EventCallback IncommingMessageRecieved { get; set; }

    [Parameter, Display(Name = "پیام دریافتی جدید ساید مقابل")]
    public ChatMessageDto? NewIncomingMessage { get; set; }

    [Parameter, Display(Name = "پیام دریافتی جدید ساید مقابل")]
    public EditedMessageDto? NewIncomingEditedMessage { get; set; }

    [Parameter, Display(Name = "ایونت بستن سشن مکالمه")]
    public EventCallback<bool> OnCancel { get; set; }

    [Parameter, Display(Name = "لیست پیام های دیده شده")]
    public List<Guid>? MessagedSeened { get; set; }

    [Display(Name = "پیام ورودی نوار")]
    private string NewMessage = "";

    [Display(Name = "هندل همزمانی")]
    private Guid? PreviousSelectedUserId = null;
    private Guid? PreviousSelectedChannelId = null;
    private Guid? PreviousSelectedGroupId = null;


    [Display(Name = "ایونت کم شدن تعداد پیغام ها")]
    [Parameter] public EventCallback<(Guid otherUserId, int MessagesCountSeened)> ReduceMessageSeenedCount { get; set; }
    [Parameter]
    public Guid? DeletedMessageId { get; set; }
    public bool EditMode { get; set; } = false;
    public EditedMessageDto? EditedMessage { get; set; } = null;
    [Parameter] public Guid? ClearedHistoryId { get; set; }
    [Parameter] public EventCallback OnHistoryCleared { get; set; }

    public bool ReplyMode { get; set; } = false;

    [Parameter] public string? MyAvatar { get; set; }

    public ChatMessageDto? SelectedMessageToReply { get; set; } = null;

    [Parameter] public EventCallback OnSendMessageSession { get; set; }

    [Parameter] public bool IsMobile { get; set; }

    [Parameter] public EventCallback BackToUsersList { get; set; }

    [Parameter] public EventCallback<Guid> SendReaction { get; set; }

    [Parameter] public EventCallback NullReaction { get; set; }

    [Parameter] public TypingReactionDto? reactionType { get; set; } = null;

    [Parameter] public EventCallback<MessageReaction> MessageReactioncallBack { get; set; }

    [Parameter] public MessageReaction? EmojiReaction { get; set; }

    [Parameter] public EventCallback<MessageReaction> NullEmojiReactioncallBack { get; set; }

    [Parameter] public EventCallback ClearMissedMessageUser { get; set; }

    [Parameter] public List<ChatMessageDto> MissedUserMessage { get; set; }
    [Parameter] public EventCallback<(Guid OtherId,DateTime now, string Message)> UpdateTimeList { get; set; }

    public string? ReactionTitle { get; set; } = null;

    [Parameter] public EventCallback<ChatMessageDto> changeUserLastMessage { get; set; }

    [Parameter] public bool LoadScroll { get; set; }

    [Parameter] public EventCallback FalseScroll { get; set; }

    [Parameter] public EventCallback trueScroll { get; set; }

    // اگر لازم بود:
    [Parameter] public EventCallback<UserDto?> GoToSettingInMobile { get; set; }
    [Parameter] public EventCallback<string> joinchannelevent { get; set; }
    [Parameter] public Guid? PinChangedScopeId { get; set; }
    public int MessageCount { get; set; } = 15;
    public bool IsloadedOldMessages { get; set; } = false;

    // ✅ پیش‌نویس هر گفتگو (کلید: OtherUser.Id)
    private readonly Dictionary<Guid, string> _drafts = new();

    // typing indicator (optional)
    private System.Timers.Timer? typingTimer;
    private const int TypingDelay = 1000;

    // اسکرول تجمیعی
    private bool _pendingScrollToBottom;

    private bool isProcessing = false;
    [Parameter]
    public int channelcount { get; set; }

    private bool _searchOpen;
    private string _searchTerm = string.Empty;
    private List<SearchHitDto> _searchResults = new();
    private DateTime? _searchCursor;
    private bool _searchHasMore;
    private bool _searchBusy;
    private int _searchActiveIndex = -1;      // نتیجه‌ای که الان روی آن هستیم

    private Guid? _highlightedMessageId;      // پیامی که بعد از پرش هایلایت می‌شود
    private bool _inJumpMode;                 // در حالت پرش، virtualization خاموش است

    private System.Timers.Timer? _searchDebounce;
    private CancellationTokenSource? _searchCts;


    #endregion
    //private static MarkupString LinkifyMentions(string content)
    //{
    //    if (string.IsNullOrWhiteSpace(content))
    //        return content;
    //    var result = Regex.Replace(content, @"@([A-Za-z0-9_]+)", "<a href='/@$1'>@$1</a>");
    //    return new MarkupString(result);
    //}
    #region lifecycle

    protected override async Task OnInitializedAsync()
    {
        CurrentUser = await ((CustomAuthenticationStateProvider)stateprovider).GetUserInfoAsync();
        objRef ??= DotNetObjectReference.Create(this);
        objRef2 ??= DotNetObjectReference.Create(this);
        await OnSendMessageSession.InvokeAsync();
  
    }
    private async Task UploadChannelFileWithProgress(BrowserFileHandle handle, string caption)
    {
        // دقت کن: نام فیلد اینجا channelId است نه conversationId
        var fields = new Dictionary<string, string>
        {
            ["channelId"] = SelectedChannel!.Id.ToString(),
            ["caption"] = caption
        };

        await RunUploadAsync(handle, "/api/v1/channel/UploadFile", fields, caption);
    }

    public async Task cancelUpload()
    {
        if (_activeUploadHandle is not null)
            await Uploader.AbortAsync(_activeUploadHandle);

        _cancellationTokenSource?.Cancel();   // برای مسیر قدیمیِ پیام صوتی
        IsUploading = false;
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = { "بایت", "کیلوبایت", "مگابایت", "گیگابایت" };
        double value = bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1) { value /= 1024; unit++; }
        return $"{value:0.#} {units[unit]}";
    }
    private async Task OnTogglePinRequested((Guid MessageId, bool Pin) request)
    {
        await TogglePinMessage(request.MessageId, request.Pin);
    }
    protected override async Task OnParametersSetAsync()
    {

        if (isProcessing) return;
        isProcessing = true;
        if (PinChangedScopeId is not null && PinChangedScopeId == CurrentScopeId)
        {
            PinChangedScopeId = null;
            await LoadPinnedAsync();
        }
        try
            {
            HandleDeletedMessage();
            await HandleClearedHistory();
            await HandleReactionAsync();
            await HandleEmojiReactionAsync();
            HandleEditedMessage();
            await HandleNewIncomingMessageAsync();
            await HandleMissedMessagesAsync();
            HandleSeenMessages();

            await HandleUserChangedAsync();

            await HandleScrollAsync();
        }
        finally
        {
            isProcessing = false;
        }
    }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        objRef ??= DotNetObjectReference.Create(this);
        objRef2 ??= DotNetObjectReference.Create(this);
        await js.InvokeVoidAsync("initScrollListener", 1, objRef);
        if (SelectedChannel != null)
        {
            await js.InvokeVoidAsync("initScrollListener", 2, objRef2);
         
        }
        if (firstRender)
        {
         
        
           if(SelectedChannel != null)
            {
                await js.InvokeVoidAsync("initScrollListener", 2, objRef); 
            }
            else
                await js.InvokeVoidAsync("initScrollListener", 1, objRef);
            NeedScrollToBottom();
        }

        if (_pendingScrollToBottom)
        {
            _pendingScrollToBottom = false;
            await Task.Delay(50);
            if (SelectedChannel != null)
            {
 
                await js.InvokeVoidAsync("scrollToBottom", 1);
            }
            else
                await js.InvokeVoidAsync("scrollToBottom", 1);
            await MarkVisibleIncomingAsSeenAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_cancellationTokenSource is not null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
            typingTimer?.Stop();
            typingTimer?.Dispose();

            try { await js.InvokeVoidAsync("disposeScrollListener", _chatContainer); } catch { /* ignore */ }

            objRef?.Dispose();

            objRef2?.Dispose();
        }
        catch { /* ignore */ }
    }

    #endregion

    #region JS helpers

    private void NeedScrollToBottom() => _pendingScrollToBottom = true;

    //[JSInvokable]
    //public async Task LoadMoreMessages()
    //{
    //    if (_messages.Count >= MessageCount)
    //    {
    //        IsloadedOldMessages = true;

    //        var previousScrollHeight = await js.InvokeAsync<int>("getScrollHeight", _chatContainer);

    //        MessageCount += 15;

    //        await LoadMoreMesages(MessageCount);

    //        StateHasChanged();

    //        await Task.Delay(50);

    //        var newScrollHeight = await js.InvokeAsync<int>("getScrollHeight", _chatContainer);
    //        var scrollDiff = newScrollHeight - previousScrollHeight;

    //        await js.InvokeVoidAsync("setScrollPosition", _chatContainer, scrollDiff);
    //    }
    //    else
    //    {
    //        MessageCount = 15;
    //    }
    //}

    [JSInvokable]
    public async Task LoadMoreChannelMessages()
    {
        if (_messages.Count >= MessageCount)
        {
            IsloadedOldMessages = true;

            var previousScrollHeight = await js.InvokeAsync<int>("getScrollHeight", _chatContainer);

            MessageCount += 15;

            await LoadMoreChannelMesages(MessageCount);

            StateHasChanged();

            await Task.Delay(50);

            var newScrollHeight = await js.InvokeAsync<int>("getScrollHeight", _chatContainer);
            var scrollDiff = newScrollHeight - previousScrollHeight;

            await js.InvokeVoidAsync("setScrollPosition", _chatContainer, scrollDiff);
        }
        else
        {
            MessageCount = 15;
        }
    }

    [JSInvokable]
    public async Task ReceiveLocationFromJS(object coords)
    {
        var json = JsonSerializer.Serialize(coords);
        var coordinates = JsonSerializer.Deserialize<LocationModel>(json);
        var location = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            Content = " لوکیشن من  👇📍",
            IsMine = true,
            Type = MessageType.Location,
            SendAt = DateTime.Now,
            Latitude = coordinates.latitude,
            Longitude = coordinates.longitude,
        };

        await SendMessage(location);
    }

    #endregion

    #region UI actions

    private async Task GetLocation()
    {
        await js.InvokeVoidAsync("startLocationWatcher", objRef);
    }

    private async Task RecordVoice()
    {
        bool hasMic = await js.InvokeAsync<bool>("checkAudioDevices");
        if (!hasMic)
        {
            ErrorMessage("🚨  هیچ میکروفونی متصل نیست یا اجازه میکروفون را به مرورگر نداده اید!");
            return;
        }

        isRecording = true;
        await js.InvokeVoidAsync("startRecording");
    }

    private async Task StopRecording()
    {
        isRecording = false;
        var file = await js.InvokeAsync<string>("stopRecording");
        byte[] audioData = Convert.FromBase64String(file);
        IBrowserFile browserFile = new CustomBrowserFile(audioData, "recorded_audio.wav", "audio/wav");
        await UploadFile(browserFile, string.Empty);
        StateHasChanged();
    }

    private async Task HandleCancelClick()
    {
        var userHasMessages = _messages.Any();
        var shouldRemoveFromChatsList = !userHasMessages;
        await OnCancel.InvokeAsync(shouldRemoveFromChatsList);
    }

    private void BindEmoji(string emoji) => NewMessage += emoji;

    public async Task CloseEmojies() => ShowEmojiPicker = false;

    public async Task OnEditMessageHandler(EditedMessageDto messageDto)
    {
        EditedMessage = messageDto;
        NewMessage = messageDto.Message;
        EditMode = true;
    }

    public async Task DeleteTargetMessage(Guid MessageId)
    {
        var target = _messages.FirstOrDefault(m => m.Id == MessageId);
        if (target != null) _messages.Remove(target);
    }

    public async Task HandleReplyMessage(ChatMessageDto message)
    {
        ReplyMode = true;
        SelectedMessageToReply = message;
    }

    public string? HandelParentMessage(ChatMessageDto message)
    {
        if (message.ParentId is null) return null;

        // اگر والد در همین صفحه لود شده باشد متن به‌روزش را بگیر
        // (ممکن است بعد از ارسال ویرایش شده باشد)، وگرنه از پیش‌نمایش سرور.
        var loaded = _messages.FirstOrDefault(x => x.Id == message.ParentId.Value);
        return loaded?.Content ?? message.ParentContent;
    }

    public string? HandelParentSender(ChatMessageDto message)
    {
        if (message.ParentId is null) return null;

        var loaded = _messages.FirstOrDefault(x => x.Id == message.ParentId.Value);
        return loaded?.SenderName ?? message.ParentSenderName;
    }

    private void SuccessMessage()
    {
        snackbar.Clear();
        snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
        snackbar.Add("عملیات با موفقیت انجام شد", Severity.Success);
    }

    private void ErrorMessage(string message)
    {
        snackbar.Clear();
        snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
        snackbar.Add(message, Severity.Error);
    }

    public async Task BackToUserList() => await BackToUsersList.InvokeAsync();

    public async Task SendReactions(MessageReaction reaction)
        => await MessageReactioncallBack.InvokeAsync(reaction);

    private void OnUserTyping()
    {
        typingTimer?.Stop();
        typingTimer?.Dispose();

        typingTimer = new System.Timers.Timer(TypingDelay);
        typingTimer.Elapsed += async (sender, args) =>
        {
            typingTimer?.Stop();
            typingTimer?.Dispose();
            // optionally: notify "stopped typing"
        };
        typingTimer.AutoReset = false;
        typingTimer.Start();
    }

    public async Task ReactionToAnotherUser(KeyboardEventArgs e)
    {
        if (e.Code == "Space")
        {
            if (OtherUser != null)
                await SendReaction.InvokeAsync(OtherUser.Id);
        }

        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await messageInput.BlurAsync();
            if (!string.IsNullOrWhiteSpace(NewMessage))
            {
                await SendMessage();
                await messageInput.FocusAsync();
            }
        }
    }

    public async Task uploadDialog()
    {
        var dialog = await DialogService.ShowAsync<UploadDialogComponent>();
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is UploadDialogComponent.UploadResult r)
        {
            await UploadFileWithProgress(r.Handle, r.Caption ?? string.Empty);
        }
    }

    public async Task uploadFileDialog()
    {
        var dialog = await DialogService.ShowAsync<UploadDialogComponent>();
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is UploadDialogComponent.UploadResult r)
        {
            await UploadGroupFileWithProgress(r.Handle, r.Caption ?? string.Empty);
        }
    }

    public async Task uploadChannelDialog()
    {
        var dialog = await DialogService.ShowAsync<UploadDialogComponent>();
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is UploadDialogComponent.UploadResult r)
        {
            await UploadChannelFileWithProgress(r.Handle, r.Caption ?? string.Empty);
        }
    }
    public async Task gotoDetailForMobile(UserDto? chosenUser)
    {
        if (GoToSettingInMobile.HasDelegate)
            await GoToSettingInMobile.InvokeAsync(chosenUser);
        else if (BackToUsersList.HasDelegate)
            await BackToUsersList.InvokeAsync();
    }

    #endregion

    #region data loading & mutations

    private async Task LoadMesages()
    {
        _messages.Clear();
        _oldestCursor = null;
        _hasMoreOlder = true;

        Conversation = await chatService.ReadyOrCreateConversationBy(OtherUser!.Id);
        var other = Conversation!.users.First(c => c.Id != CurrentUser!.Id);

        var page = await messageService.LoadMessages(Conversation!.id);

        if (page is not null && page.Items.Count > 0)
        {
            _messages.AddRange(page.Items);
            _oldestCursor = page.NextCursor;
            _hasMoreOlder = page.HasMore;

            var messageIds = page.Items.Where(x => !x.IsSeen).Select(m => m.Id).ToList();
            if (messageIds.Count > 0)
                await HandleMessageSeen(
                    new MessageSeenDto(messageIds, other.Id, Conversation.id, CurrentUser!.Id));

            await ReduceMessageSeenedCount.InvokeAsync((OtherUser.Id, 1100));

            NeedScrollToBottom();
            await trueScroll.InvokeAsync();
        }

        if (OtherUser != null && _drafts.TryGetValue(OtherUser.Id, out var draft))
            NewMessage = draft;
    }


    // ---- اسکرول به بالا ----

    [JSInvokable]                       // ← بدون این، JS نمی‌تواند صدایش بزند
    public async Task LoadMoreMessages()
    {
        if (_isLoadingOlder) return;
        _isLoadingOlder = true;
        IsloadedOldMessages = true;

        try
        {
            var previousHeight = await js.InvokeAsync<int>("getScrollHeight", _chatContainer);
            List<ChatMessageDto> fresh;

            if (SelectedGroup != null)
            {
                // گروه هنوز روی صفحه‌بندی قدیمی است تا وقتی بک‌اندش را هم مهاجرت بدهیم
                if (!_hasMoreOlder) return;

                MessageCount += 15;
                var messages = await chatService.LoadGroupMessages(SelectedGroup.Id, MessageCount);
                if (messages is null) return;

                var known = new HashSet<Guid>(_messages.Select(m => m.Id));
                fresh = messages.Where(m => !known.Contains(m.Id)).ToList();
                _hasMoreOlder = fresh.Count > 0;
            }
            else
            {
                if (!_hasMoreOlder) return;

                Conversation ??= await chatService.ReadyOrCreateConversationBy(OtherUser!.Id);
                var page = await messageService.LoadMessages(Conversation!.id, _oldestCursor);
                if (page is null) return;

                _hasMoreOlder = page.HasMore;
                if (page.NextCursor.HasValue)
                    _oldestCursor = page.NextCursor;

                var known = new HashSet<Guid>(_messages.Select(m => m.Id));
                fresh = page.Items.Where(m => !known.Contains(m.Id)).ToList();
            }

            if (fresh.Count > 0)
            {
                _messages.InsertRange(0, fresh);
                _messages.Sort((a, b) => a.SendAt.CompareTo(b.SendAt));
            }

            StateHasChanged();
            await Task.Delay(50);

            var newHeight = await js.InvokeAsync<int>("getScrollHeight", _chatContainer);
            await js.InvokeVoidAsync("setScrollPosition", _chatContainer, newHeight - previousHeight);
        }
        finally
        {
            _isLoadingOlder = false;
        }
    }

    public async Task LoadChannelMesages(int count = 15)
    {
        _messages.Clear();



        var messages = await _channelservice.LoadChannelMessages(SelectedChannel!.Id, count);
        if (messages?.Any() == true)
        {
            foreach (var m in messages)
                m.Content = m.Content;
            _messages.AddRange(messages);

            var messageIds = messages.Where(x => !x.IsSeen).Select(m => m.Id).ToList();

      
            await trueScroll.InvokeAsync();
            NeedScrollToBottom();
        }
        if (_drafts.TryGetValue(SelectedChannel.Id, out var draft))
            NewMessage = draft;
    }
    public async Task LoadGroupMesages(int count = 15)
    {
        _messages.Clear();



        var messages = await chatService.LoadGroupMessages(SelectedGroup!.Id, count);
        if (messages?.Any() == true)
        {
            try
            {

 
            foreach (var m in messages) { 
                m.Content = m.Content;}
            }
            catch (Exception ex)
            {
                Console.WriteLine("OK Ok  Ok "+ex.Message);
                throw ex;
            }
            _messages.AddRange(messages);

            var messageIds = messages.Where(x => !x.IsSeen).Select(m => m.Id).ToList();


            await trueScroll.InvokeAsync();
            NeedScrollToBottom();
        }
        if (_drafts.TryGetValue(SelectedGroup.Id, out var draft))
            NewMessage = draft;
        Loading = false;
    }



    private async Task LoadMoreChannelMesages(int count = 15)
    {
   
        var messages = await _channelservice.LoadChannelMessages(SelectedChannel.Id, count);

        var known = new HashSet<Guid>(_messages.Select(m => m.Id));
        var newMessages = messages.Where(m => !known.Contains(m.Id)).ToList();

        if (newMessages.Any())
        {
            foreach (var m in newMessages)
                m.Content = m.Content;
            _messages.AddRange(newMessages);
            _messages.Sort((a, b) => a.SendAt.CompareTo(b.SendAt));
        }
    }

    private async Task SendMessage(ChatMessageDto? Mapmessage = null)
    {
        if (string.IsNullOrWhiteSpace(NewMessage) && Mapmessage == null)
        {
            ErrorMessage("ارسال مسیج با مشکل مواجه شد");
            return;
        }

        var me = Conversation!.users.First(c => c.Id == CurrentUser!.Id);
        var other = Conversation!.users.First(c => c.Id != CurrentUser!.Id);

        if (OtherUser!.IsBlocked || CurrentUser!.IsBlocked || OtherUser.OtherUserBlocked)
        {
            ErrorMessage("این مکالمه مسدود شده است و امکان ارسال پیام وجود ندارد.");
            return;
        }

        var messagenew = NewMessage;
        Guid? ParentId = null;
        if (ReplyMode)
            ParentId = SelectedMessageToReply!.Id;

        var newMessage = Mapmessage ?? new ChatMessageDto
        {
            Id = Guid.Empty,
            SendAt = DateTime.Now,
            SenderName = CurrentUser!.Name,
            IsMine = true,
            Content = messagenew,
            Type = 0,
            UserId = CurrentUser!.Id,
            ParentId = ParentId,
        };

        if (Mapmessage != null)
        {
            newMessage.ParentId = ParentId;
            newMessage.UserId = CurrentUser!.Id;
            newMessage.IsMine = true;
            newMessage.SenderName = CurrentUser!.Name;
        }

        if (!EditMode)
            _messages.Add(newMessage);

        await messageInput.FocusAsync(); NewMessage = string.Empty;
        StateHasChanged();

        await OnSendMessageSession.InvokeAsync();

        if (EditMode)
        {
            EditedMessage!.Message = messagenew;
            var response = await messageService.EditMessageAsync(EditedMessage!);
            if (response.Item1) SuccessMessage();
            else ErrorMessage(response.message);

            EditMode = false;

            var msg = _messages.Single(x => x.Id == EditedMessage.Id);
            msg.Content = EditedMessage.Message;
            messagenew = string.Empty;
            msg.isEdited = true;
            return;
        }

        var result = await messageService.SendMessageAsync(Conversation!.id, newMessage.Content, ParentId, newMessage.Latitude, newMessage.Longitude);
        if (result.Item1)
        {
            await changeUserLastMessage.InvokeAsync();
            newMessage.Id = result.MessageId;

        
            if (OtherUser != null) _drafts.Remove(OtherUser.Id);

            NeedScrollToBottom();

            NewMessage = string.Empty;
            SelectedMessageToReply = null;
            ReplyMode = false;
            await UpdateTimeList.InvokeAsync((OtherUser.Id, DateTime.Now,newMessage.Content));
        }
    }
    private async Task SendChannelMessage(ChatMessageDto? Mapmessage = null)
    {
        if (string.IsNullOrWhiteSpace(NewMessage) && Mapmessage == null)
        {
            ErrorMessage("ارسال مسیج با مشکل مواجه شد");
            return;
        }


        var messagenew = NewMessage;
        Guid? ParentId = null;
        if (ReplyMode)
            ParentId = SelectedMessageToReply!.Id;

        var newMessage = Mapmessage ?? new ChatMessageDto
        {
            Id = Guid.Empty,
            SendAt = DateTime.Now,
            SenderName = CurrentUser!.Name,
            IsMine = true,
            Content = messagenew,
            Type = 0,
            UserId = CurrentUser!.Id,
            ParentId = ParentId,
        };

        if (Mapmessage != null)
        {
            newMessage.ParentId = ParentId;
            newMessage.UserId = CurrentUser!.Id;
            newMessage.IsMine = true;
            newMessage.SenderName = CurrentUser!.Name;
        }
        newMessage.Content = newMessage.Content;

        NewMessage=string.Empty;
     
        if (!EditMode)
            _messages.Add(newMessage);

        await messageInput.FocusAsync();
        StateHasChanged();

        await OnSendMessageSession.InvokeAsync();

        if (EditMode)
        {
            EditedMessage!.Message = messagenew;
            var response = await messageService.EditMessageAsync(EditedMessage!);
            if (response.Item1) SuccessMessage();
            else ErrorMessage(response.message);

            EditMode = false;

            var msg = _messages.Single(x => x.Id == EditedMessage.Id);
            msg.Content =EditedMessage.Message;
            msg.Content = EditedMessage.Message;
            messagenew = string.Empty;
            msg.isEdited = true;
            return;
        }

        var result = await _channelservice.SendMessageForChannelAsync(SelectedChannel.Id, newMessage.Content, ParentId);
        if (result.Item1)
        {
            await changeUserLastMessage.InvokeAsync();
            newMessage.Id = result.MessageId;

            if (SelectedChannel != null) _drafts.Remove(SelectedChannel.Id);

            NeedScrollToBottom();

            NewMessage = string.Empty;
            SelectedMessageToReply = null;
            ReplyMode = false;
            await UpdateTimeList.InvokeAsync((SelectedChannel.Id, DateTime.Now, newMessage.Content));
        }
    }
    private async Task SendGroupMessage(ChatMessageDto? Mapmessage = null)
    {
        if (string.IsNullOrWhiteSpace(NewMessage) && Mapmessage == null)
        {
            ErrorMessage("ارسال مسیج با مشکل مواجه شد");
            return;
        }

        var messagenew = NewMessage;
        Guid? ParentId = null;
        if (ReplyMode)
            ParentId = SelectedMessageToReply!.Id;

        var newMessage = Mapmessage ?? new ChatMessageDto
        {
            Id = Guid.Empty,
            SendAt = DateTime.Now,
            SenderName = CurrentUser!.Name,
            IsMine = true,
            Content = messagenew,
            Type = 0,
            UserId = CurrentUser!.Id,
            ParentId = ParentId,
        };

        if (Mapmessage != null)
        {
            newMessage.ParentId = ParentId;
            newMessage.UserId = CurrentUser!.Id;
            newMessage.IsMine = true;
            newMessage.SenderName = CurrentUser!.Name;
        }
        newMessage.Content = newMessage.Content;

        NewMessage = string.Empty;

        if (!EditMode)
            _messages.Add(newMessage);

        await messageInput.FocusAsync();
        StateHasChanged();

        await OnSendMessageSession.InvokeAsync();

        if (EditMode)
        {
            EditedMessage!.Message = messagenew;
            var response = await messageService.EditMessageAsync(EditedMessage!);
            if (response.Item1) SuccessMessage();
            else ErrorMessage(response.message);

            EditMode = false;

            var msg = _messages.Single(x => x.Id == EditedMessage.Id);
            msg.Content = EditedMessage.Message;
            msg.Content = EditedMessage.Message;
            messagenew = string.Empty;
            msg.isEdited = true;
            return;
        }

        var result = await chatService.SendMessageForGroupAsync(SelectedGroup.Id, newMessage.Content, ParentId);
        if (result.Item1)
        {
            await changeUserLastMessage.InvokeAsync();
            newMessage.Id = result.MessageId;

            if (SelectedChannel != null) _drafts.Remove(SelectedGroup.Id);

            NeedScrollToBottom();

            NewMessage = string.Empty;
            SelectedMessageToReply = null;
            ReplyMode = false;
            await UpdateTimeList.InvokeAsync((SelectedGroup.Id, DateTime.Now, newMessage.Content));
        }
        else
        {
            _messages.Remove(newMessage);
            snackbar.Add(result.message,Severity.Error);
        }
    }

    private async Task AddMessageToUi()
    {
        await OnSendMessageSession.InvokeAsync();

        Guid? ParentId = null;
        if (ReplyMode) ParentId = SelectedMessageToReply!.Id;

        var other = Conversation!.users.First(c => c.Id != CurrentUser!.Id);

        var newMessage = new ChatMessageDto
        {
            Id = Guid.Empty,
            SendAt = DateTime.Now,
            SenderName = CurrentUser!.Name,
            IsMine = true,
            Content = NewMessage,
            Type = 0,
            UserId = CurrentUser!.Id,
            ParentId = ParentId,
        };
        newMessage.Content = newMessage.Content;
        _messages.Add(newMessage);

        NeedScrollToBottom();

        await messageInput.FocusAsync();
        NewMessage = string.Empty;
        SelectedMessageToReply = null;
        ReplyMode = false;
        await trueScroll.InvokeAsync();

        var newMessageForUsers = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            SendAt = DateTime.Now,
            SenderName = CurrentUser!.Name,
            IsMine = true,
            Content = NewMessage,
            Type = 0,
            UserId = other.Id,
            ParentId = ParentId,
        };
        await changeUserLastMessage.InvokeAsync(newMessageForUsers);

        if (EditMode)
        {
            EditedMessage!.Message = NewMessage;
            var response = await messageService.EditMessageAsync(EditedMessage!);
            if (response.Item1) SuccessMessage();
            else ErrorMessage(response.message);

            EditMode = false;

            var message = _messages.Single(x => x.Id == EditedMessage.Id);
            message.Content = EditedMessage.Message;
            message.Content = EditedMessage.Message;
            NewMessage = string.Empty;
            message.isEdited = true;
            return;
        }

        var result = await messageService.SendMessageAsync(Conversation!.id, NewMessage, ParentId);
        if (result.Item1)
        {
            newMessage.Id = result.MessageId;
            StateHasChanged();
        }
    }

    private async Task UploadFile(IBrowserFile file, string caption)
    {
        try
        {
            if (file is null) return;

            IsUploading = true;
            StateHasChanged();

            _cancellationTokenSource = new CancellationTokenSource();
            var extension = Path.GetExtension(file.Name);
            var contentType = string.IsNullOrEmpty(file.ContentType) ? "application/octet-stream" : file.ContentType;

            var uploaddto = new UploadFileDto(
                Conversation!.id,
                caption,
                new StreamContent(file.OpenReadStream(100 * 1024 * 1024, _cancellationTokenSource.Token)),
                contentType,
                extension,
                file.Name
            );

            var result = await messageService.UploadChatFileAsync(uploaddto, _cancellationTokenSource.Token);
            if (result.Item1)
            {
                var me = Conversation!.users.First(c => c.Id == CurrentUser!.Id);
                var newMessage = new ChatMessageDto
                {
                    Id = result.result!.MessageId,
                    SendAt = DateTime.Now,
                    SenderName = me!.Name,
                    IsMine = true,
                    Content = caption,
                    Type = (MessageType)result.result.MessageType,
                    UserId = CurrentUser!.Id,
                    FileContent = new ChatFilesDto
                    {
                        FileId = result.result!.FileId,
                        FileName = file.Name,
                        FileSize = file.Size.ToString()
                    }
                };
                _messages.Add(newMessage);

                IsUploading = false;

                NeedScrollToBottom();

                NewMessage = string.Empty;
                SelectedMessageToReply = null;
                ReplyMode = false;
            }
        }
        catch (OperationCanceledException)
        {
            SuccessMessage();
            IsUploading = false;
        }
        catch (Exception)
        {
            ErrorMessage("آپلود فایل با خطا مواجه شد لطفا مجدد تلاش فرمایید!");
            IsUploading = false;
        }
    }
    private async Task UploadGroupFile(IBrowserFile file, string caption)
    {
        try
        {
            if (file is null) return;

            IsUploading = true;
            StateHasChanged();

            _cancellationTokenSource = new CancellationTokenSource();
            var extension = Path.GetExtension(file.Name);
            var contentType = string.IsNullOrEmpty(file.ContentType) ? "application/octet-stream" : file.ContentType;

            var uploaddto = new UploadFileDto(
                SelectedGroup!.Id,
                caption,
                new StreamContent(file.OpenReadStream(100 * 1024 * 1024, _cancellationTokenSource.Token)),
                contentType,
                extension,
                file.Name
            );

            var result = await chatService.UploadGroupFileAsync(uploaddto, _cancellationTokenSource.Token);
            if (result.Item1)
            {
                var me =CurrentUser;
                var newMessage = new ChatMessageDto
                {
                    Id = result.result!.MessageId,
                    SendAt = DateTime.Now,
                    SenderName = me!.Name,
                    IsMine = true,
                    Content = caption,
                    Type = (MessageType)result.result.MessageType,
                    UserId = CurrentUser!.Id,
                    FileContent = new ChatFilesDto
                    {
                        FileId = result.result!.FileId,
                        FileName = file.Name,
                        FileSize = file.Size.ToString()
                    }
                };
                _messages.Add(newMessage);

                IsUploading = false;

                NeedScrollToBottom();

                NewMessage = string.Empty;
                SelectedMessageToReply = null;
                ReplyMode = false;
            }
        }
        catch (OperationCanceledException)
        {
            SuccessMessage();
            IsUploading = false;
        }
        catch (Exception)
        {
            ErrorMessage("آپلود فایل با خطا مواجه شد لطفا مجدد تلاش فرمایید!");
            IsUploading = false;
        }
    }
    private async Task UploadChannelFile(IBrowserFile file, string caption)
    {
        try
        {
            if (file is null) return;

            IsUploading = true;
            StateHasChanged();

            _cancellationTokenSource = new CancellationTokenSource();
            var extension = Path.GetExtension(file.Name);
            var contentType = string.IsNullOrEmpty(file.ContentType) ? "application/octet-stream" : file.ContentType;

            var uploaddto = new UploadFileDto(
                SelectedChannel!.Id,
                caption,
                new StreamContent(file.OpenReadStream(100 * 1024 * 1024, _cancellationTokenSource.Token)),
                contentType,
                extension,
                file.Name
            );

            var result = await messageService.UploadChannelFileAsync(uploaddto, _cancellationTokenSource.Token);
            if (result.Item1)
            {
                var me = Conversation!.users.First(c => c.Id == CurrentUser!.Id);
                var newMessage = new ChatMessageDto
                {
                    Id = result.result!.MessageId,
                    SendAt = DateTime.Now,
                    SenderName = me!.Name,
                    IsMine = true,
                    Content = caption,
                    Type = (MessageType)result.result.MessageType,
                    UserId = CurrentUser!.Id,
                    FileContent = new ChatFilesDto
                    {
                        FileId = result.result!.FileId,
                        FileName = file.Name,
                        FileSize = file.Size.ToString()
                    }
                };
                _messages.Add(newMessage);

                IsUploading = false;

                NeedScrollToBottom();

                NewMessage = string.Empty;
                SelectedMessageToReply = null;
                ReplyMode = false;
            }
        }
        catch (OperationCanceledException)
        {
            SuccessMessage();
            IsUploading = false;
        }
        catch (Exception)
        {
           
            IsUploading = false;
        }
    }



    public class MessageComparer : IEqualityComparer<ChatMessageDto>
    {
        public bool Equals(ChatMessageDto x, ChatMessageDto y) => x.Id == y.Id;
        public int GetHashCode(ChatMessageDto obj) => obj.Id.GetHashCode();
    }

    #endregion

    #region handlers pack
    private Guid? CurrentScopeId =>
    SelectedChannel?.Id ?? SelectedGroup?.Id ?? Conversation?.id;
    private async Task LoadPinnedAsync()
    {
        if (CurrentScopeId is null) return;

        _pinned = await messageService.GetPinnedMessages(CurrentScopeId.Value) ?? new();
        _pinnedIndex = 0;
        StateHasChanged();
    }
    public async Task TogglePinMessage(Guid messageId, bool pin)
    {
        var (ok, message) = await messageService.TogglePin(messageId, pin);

        if (!ok)
        {
            ErrorMessage(message);
            return;
        }

        snackbar.Add(message, Severity.Success);
        await LoadPinnedAsync();
    }


    // کلیک روی نوار پین — از همان JumpToMessage جستجو استفاده می‌کند
    private async Task GoToPinned()
    {
        if (ActivePin is null) return;

        var target = ActivePin.Id;

        // اگر چند پین داریم، هر کلیک به بعدی می‌رود — رفتار تلگرام
        if (_pinned.Count > 1)
            _pinnedIndex = (_pinnedIndex + 1) % _pinned.Count;

        await JumpToMessage(target);
    }

    private string PinPreview(PinnedMessageDto pin)
    {
        if (!string.IsNullOrWhiteSpace(pin.Content))
            return pin.Content.Length > 70 ? pin.Content[..70] + "…" : pin.Content;

        return pin.FileName ?? "پیوست";
    }
    /// <summary>فقط مدیران می‌توانند پین کنند. در چت خصوصی هر دو طرف.</summary>
    private bool CanPin =>
        (SelectedChannel is null && SelectedGroup is null && Conversation is not null)
        || CanManageCurrentChat;

    private PinnedMessageDto? ActivePin =>
        _pinned.Count > 0 && _pinnedIndex < _pinned.Count ? _pinned[_pinnedIndex] : null;
    private void HandleDeletedMessage()
    {
        if (DeletedMessageId != null)
        {
            var targetMessage = _messages.FirstOrDefault(m => m.Id == DeletedMessageId.Value);
            if (targetMessage != null)
                _messages.Remove(targetMessage);

            DeletedMessageId = null;
        }
    }

    private async Task HandleReactionAsync()
    {
        if (reactionType != null)
        {
            await GetReaction();
            reactionType = null;
            await NullReaction.InvokeAsync();
        }
    }

    private async Task HandleEmojiReactionAsync()
    {
        if (EmojiReaction != null)
        {
            var message = _messages.FirstOrDefault(m => m.Id == EmojiReaction.MessageId);
            if (message != null)
            {
                message.Reaction = EmojiReaction.Reaction;
                StateHasChanged();
            }
            EmojiReaction = null;
            await NullEmojiReactioncallBack.InvokeAsync();
        }
    }

    private void HandleEditedMessage()
    {
        if (NewIncomingEditedMessage != null)
        {
            var targetMessage = _messages.FirstOrDefault(m => m.Id == NewIncomingEditedMessage.Id);
            if (targetMessage != null)
            {
                targetMessage.Content = NewIncomingEditedMessage.Message;
 
                targetMessage.isEdited = true;
            }
            NewIncomingEditedMessage = null;
        }
    }

    private async Task HandleNewIncomingMessageAsync()
    {
        if (NewIncomingMessage != null)
        {
            var name = CurrentUser.Name;
            NewIncomingMessage.Content = NewIncomingMessage.Content;
         if(_messages.Any(x=>x.ConversationType==NewIncomingMessage.ConversationType))
            _messages.Add(NewIncomingMessage);
            await IncommingMessageRecieved.InvokeAsync();
            NewIncomingMessage = null;

            NeedScrollToBottom();
            StateHasChanged();
        }
    }

    private async Task HandleMissedMessagesAsync()
    {
        if (MissedUserMessage?.Count > 0)
        {
            var known = new HashSet<Guid>(_messages.Select(m => m.Id));
            var newMessages = MissedUserMessage
                .Where(m => !known.Contains(m.Id))
                .OrderBy(x => x.SendAt)
                .ToList();

            if (newMessages.Count > 0)
            {
                foreach (var m in newMessages)
                    m.Content = m.Content;
                _messages.AddRange(newMessages);
                await ClearMissedMessageUser.InvokeAsync();

                NeedScrollToBottom();
            }
        }
    }
    public async Task JoinToChannelEvent(string UserName)
    {
      await  joinchannelevent.InvokeAsync(UserName);
    }

    private void HandleSeenMessages()
    {
        if (MessagedSeened?.Count > 0)
        {
            foreach (var chatMessage in _messages.Where(m => MessagedSeened.Contains(m.Id)))
                chatMessage.IsSeen = true;
        }
    }

    private async Task HandleUserChangedAsync()
    {
        if (SelectedChannel != null && SelectedChannel.Id != PreviousSelectedChannelId)
        {
            if (PreviousSelectedUserId.HasValue)
                _drafts[PreviousSelectedUserId.Value] = NewMessage ?? string.Empty;
            if (PreviousSelectedChannelId.HasValue)
                _drafts[PreviousSelectedChannelId.Value] = NewMessage ?? string.Empty;
            if (PreviousSelectedGroupId.HasValue)
                _drafts[PreviousSelectedGroupId.Value] = NewMessage ?? string.Empty;

            PreviousSelectedChannelId = SelectedChannel.Id;
            PreviousSelectedUserId = null;
            PreviousSelectedGroupId = null;

            Loading = true;
            NewMessage = string.Empty;

            await LoadChannelMesages();

            if (_drafts.TryGetValue(SelectedChannel.Id, out var draft))
                NewMessage = draft;

            Loading = false;

            StateHasChanged();
        }
        else if(SelectedGroup!=null && SelectedGroup.Id != PreviousSelectedGroupId)
        {
            if (PreviousSelectedUserId.HasValue)
                _drafts[PreviousSelectedUserId.Value] = NewMessage ?? string.Empty;
            if (PreviousSelectedChannelId.HasValue)
                _drafts[PreviousSelectedChannelId.Value] = NewMessage ?? string.Empty;
            if(PreviousSelectedGroupId.HasValue)
                _drafts[PreviousSelectedGroupId.Value] = NewMessage ?? string.Empty;


            PreviousSelectedGroupId = SelectedGroup.Id;
            PreviousSelectedUserId = null;
            PreviousSelectedChannelId= null;

            Loading = true;
            NewMessage = string.Empty;

            await LoadGroupMesages();

            if (_drafts.TryGetValue(SelectedGroup.Id, out var draft))
                NewMessage = draft;

            Loading = false;

            StateHasChanged();
        }
        else if (OtherUser != null && OtherUser.Id != PreviousSelectedUserId && SelectedChannel == null)
        {
            if (PreviousSelectedChannelId.HasValue)
                _drafts[PreviousSelectedChannelId.Value] = NewMessage ?? string.Empty;
            if (PreviousSelectedUserId.HasValue)
                _drafts[PreviousSelectedUserId.Value] = NewMessage ?? string.Empty;
            if (PreviousSelectedGroupId.HasValue)
                _drafts[PreviousSelectedGroupId.Value] = NewMessage ?? string.Empty;

            if (CurrentUser == null)
            {
                CurrentUser = await ((CustomAuthenticationStateProvider)stateprovider).GetUserInfoAsync();
            }

            PreviousSelectedUserId = OtherUser.Id;
            PreviousSelectedChannelId = null;
            PreviousSelectedGroupId = null;
            Loading = true;
            NewMessage = string.Empty;

            await LoadMesages();


            if (_drafts.TryGetValue(OtherUser.Id, out var draft))
                NewMessage = draft;

            CurrentUser!.avatar = MyAvatar;
            Loading = false;

            StateHasChanged();
        }
    }

    private async Task HandleScrollAsync()
    {
        if (LoadScroll)
        {
            NeedScrollToBottom();
            await FalseScroll.InvokeAsync();
        }
    }

    private async Task GetReaction()
    {
        if (reactionType?.MessageType == 0)
        {
            ReactionTitle = "...در حال نوشتن";
            await Task.Delay(3000);
        }
    }

    private async Task MarkVisibleIncomingAsSeenAsync()
    {
        if (_messages.Count == 0 || CurrentUser is null || OtherUser is null) return;

        var unseen = _messages
            .Where(m => !m.IsSeen && m.UserId != CurrentUser.Id)
            .Select(m => m.Id)
            .ToList();

        if (unseen.Count == 0) return;

        foreach (var id in unseen)
        {
            var msg = _messages.FirstOrDefault(x => x.Id == id);
            if (msg != null) msg.IsSeen = true;
        }

        await HandleMessageSeen(new MessageSeenDto(unseen, OtherUser.Id, Conversation!.id, CurrentUser.Id));
    }

    private async Task HandleMessageSeen(MessageSeenDto messagesForSeen)
        => await OnMessageSeen.InvokeAsync(messagesForSeen);
    private async Task UploadFileWithProgress(BrowserFileHandle handle, string caption)
    {
        var fields = new Dictionary<string, string>
        {
            ["ConversationId"] = Conversation!.id.ToString(),
            ["caption"] = caption
        };

        await RunUploadAsync(handle, "/api/v1/message/UploadFile", fields, caption);
    }


    // ------------------------------------------------------------------- گروه

    private async Task UploadGroupFileWithProgress(BrowserFileHandle handle, string caption)
    {
        var fields = new Dictionary<string, string>
        {
            ["conversationId"] = SelectedGroup!.Id.ToString(),
            ["caption"] = caption
        };

        await RunUploadAsync(handle, "/api/v1/Groups/UploadFile", fields, caption);
    }


    // ------------------------------------------------------------------ کانال




    // ------------------------------------------------ منطق مشترک هر سه حالت

    private async Task RunUploadAsync(
        BrowserFileHandle handle,
        string endpoint,
        Dictionary<string, string> fields,
        string caption)
    {
        _activeUploadHandle = handle.Handle;
        IsUploading = true;
        UploadPercent = 0;
        UploadSizeText = string.Empty;
        StateHasChanged();

        try
        {
            var response = await Uploader.SendAsync(
                handle.Handle,
                endpoint,
                fields,
                (percent, loaded, total) =>
                {
                    UploadPercent = percent;
                    UploadSizeText = $"{FormatBytes(loaded)} از {FormatBytes(total)}";
                    InvokeAsync(StateHasChanged);
                });

            if (response.IsAborted)
                return;

            if (!response.IsSuccess)
            {
                ErrorMessage(response.Status switch
                {
                    413 => "حجم فایل بیش از حد مجاز است. فایل کوچک‌تری انتخاب کنید.",
                    401 => "نشست شما منقضی شده. دوباره وارد شوید.",
                    0 => "ارتباط با سرور قطع شد. اتصال اینترنت را بررسی کنید.",
                    _ => "آپلود فایل با خطا مواجه شد لطفا مجدد تلاش فرمایید!"
                });
                return;
            }

            var dto = JsonSerializer.Deserialize<BaseResponseDto<UploadFileResult>>(
                response.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto?.result is null)
            {
                ErrorMessage("پاسخ سرور نامعتبر بود.");
                return;
            }

            _messages.Add(new ChatMessageDto
            {
                Id = dto.result.MessageId,
                SendAt = DateTime.Now,
                SenderName = CurrentUser!.Name,
                IsMine = true,
                Content = caption,
                Type = (MessageType)dto.result.MessageType,
                UserId = CurrentUser!.Id,
                FileContent = new ChatFilesDto
                {
                    FileId = dto.result.FileId,
                    FileName = handle.Name,
                    FileSize = handle.Size.ToString()
                }
            });

            NeedScrollToBottom();

            NewMessage = string.Empty;
            SelectedMessageToReply = null;
            ReplyMode = false;
        }
        catch (Exception)
        {
            ErrorMessage("آپلود فایل با خطا مواجه شد لطفا مجدد تلاش فرمایید!");
        }
        finally
        {
            await Uploader.ReleaseAsync(handle.Handle);
            _activeUploadHandle = null;
            IsUploading = false;
            UploadPercent = 0;
            StateHasChanged();
        }
    }

    private void ToggleSearch()
    {
        _searchOpen = !_searchOpen;
        if (!_searchOpen) ResetSearch();
    }

    private void ResetSearch()
    {
        _searchTerm = string.Empty;
        _searchResults.Clear();
        _searchCursor = null;
        _searchHasMore = false;
        _searchActiveIndex = -1;
        _searchCts?.Cancel();
    }


    // ------------------------------------------------------- تایپ با debounce

    private void OnSearchTermChanged(string value)
    {
        _searchTerm = value;

        _searchDebounce?.Stop();
        _searchDebounce?.Dispose();

        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length < 2)
        {
            _searchResults.Clear();
            _searchHasMore = false;
            StateHasChanged();
            return;
        }

        // بدون debounce هر حرفی که تایپ می‌شود یک کوئری روی ۱۰۰ هزار ردیف می‌زند
        _searchDebounce = new System.Timers.Timer(350) { AutoReset = false };
        _searchDebounce.Elapsed += async (_, _) => await InvokeAsync(() => RunSearch(reset: true));
        _searchDebounce.Start();
    }


    // -------------------------------------------------------------- اجرای جستجو
    /// <summary>
    /// ترتیب مهم است: کانال، بعد گروه، بعد خصوصی.
    /// فیلد Conversation از چت قبلی باقی می‌ماند، پس نباید اول چک شود.
    /// </summary>
    private (Guid? conversationId, Guid? channelId) CurrentScope()
    {
        if (SelectedChannel is not null) return (null, SelectedChannel.Id);
        if (SelectedGroup is not null) return (SelectedGroup.Id, null);
        return (Conversation?.id, null);
    }
    private async Task RunSearch(bool reset)
    {
        if (_searchBusy) return;

        var term = _searchTerm?.Trim() ?? string.Empty;
        if (term.Length < 2) return;

        _searchBusy = true;
        if (reset)
        {
            _searchResults.Clear();
            _searchCursor = null;
            _searchActiveIndex = -1;
        }
        StateHasChanged();

        // اگر کاربر سریع تایپ کند، درخواست قبلی لغو می‌شود
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();

        try
        {
            var (convId, chanId) = CurrentScope();

            var page = await messageService.SearchMessages(
                conversationId: convId,
                channelId: chanId,
                term: term,
                before: _searchCursor,
                cancellationToken: _searchCts.Token);

            if (page is null) return;

            _searchResults.AddRange(page.Items);
            _searchCursor = page.NextCursor;
            _searchHasMore = page.HasMore;
        }
        catch (OperationCanceledException)
        {
            // درخواست جدیدتری جایگزین شده
        }
        finally
        {
            _searchBusy = false;
            StateHasChanged();
        }
    }

    private async Task LoadMoreResults()
    {
        if (_searchHasMore && !_searchBusy)
            await RunSearch(reset: false);
    }


    // ------------------------------------------------- پرش به یک نتیجه

    private async Task JumpToMessage(Guid messageId)
    {
        // اگر پیام از قبل لود شده، فقط virtualization را خاموش کن و برو سراغش.
        // بدون خاموش کردنش، عنصر ممکن است در DOM نباشد و scrollToElement کاری نکند.
        var alreadyLoaded = _messages.Any(m => m.Id == messageId);

        if (!alreadyLoaded)
        {
            var context = await messageService.LoadMessagesAround(messageId);
            if (context is null || context.Count == 0)
            {
                snackbar.Add("پیام مورد نظر یافت نشد یا حذف شده است.", Severity.Warning);
                return;
            }

            _messages.Clear();
            _messages.AddRange(context);
            _oldestCursor = context.Min(m => m.SendAt);
            _hasMoreOlder = true;
        }

        _inJumpMode = true;
        _highlightedMessageId = messageId;

        StateHasChanged();
        await Task.Delay(80);                 // تا DOM رندر شود

        await js.InvokeVoidAsync("scrollToElement", $"msg-{messageId}");

        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);
            _highlightedMessageId = null;
            await InvokeAsync(StateHasChanged);
        });
    }

    private async Task JumpToResult(int index)
    {
        if (index < 0 || index >= _searchResults.Count) return;

        _searchActiveIndex = index;
        await JumpToMessage(_searchResults[index].Id);
    }
    private Task NextResult() => JumpToResult(_searchActiveIndex + 1);
    private Task PreviousResult() => JumpToResult(_searchActiveIndex - 1);


    // --------------------------------------------- بازگشت به آخرین پیام‌ها

    private async Task BackToLatest()
    {
        _inJumpMode = false;
        _highlightedMessageId = null;
        _searchActiveIndex = -1;

        if (SelectedChannel is not null)
            await LoadChannelMesages();
        else if (SelectedGroup is not null)
            await LoadGroupMesages();
        else
            await LoadMesages();

        StateHasChanged();
    }


    // ---------------------------------------------------------------- کمکی

    /// <summary>بریده‌ای از متن پیام حول کلمه‌ی جستجو، برای نمایش در لیست نتایج.</summary>
    private string Snippet(SearchHitDto hit)
    {
        var text = hit.Content;

        if (string.IsNullOrWhiteSpace(text))
            return hit.FileName ?? "(پیوست)";

        var term = _searchTerm.Trim();
        var idx = text.IndexOf(term, StringComparison.OrdinalIgnoreCase);

        if (idx < 0 || text.Length <= 90)
            return text.Length <= 90 ? text : text[..90] + "…";

        var start = Math.Max(0, idx - 30);
        var length = Math.Min(90, text.Length - start);

        return (start > 0 ? "…" : "") + text.Substring(start, length) + "…";
    }

    public void DisposeSearch()
    {
        _searchDebounce?.Stop();
        _searchDebounce?.Dispose();
        _searchCts?.Cancel();
        _searchCts?.Dispose();
    }
    public async Task ClearHistoryDialog()
    {
        var parameters = new DialogParameters<ConfirmDangerDialog>
    {
        { x => x.Title, "پاک کردن تاریخچه" },
        { x => x.Message, $"تمام پیام‌ها و فایل‌های «{CurrentChatTitle}» برای همه‌ی اعضا حذف می‌شود." },
        { x => x.ConfirmLabel, "پاک کن" }
    };

        var dialog = await DialogService.ShowAsync<ConfirmDangerDialog>(
            "پاک کردن تاریخچه", parameters);

        var result = await dialog.Result;
        if (result.Canceled) return;

        (bool status, string message) response;

        if (SelectedChannel is not null)
            response = await _channelservice.ClearChannelHistory(SelectedChannel.Id);
        else if (SelectedGroup is not null)
            response = await chatService.ClearGroupHistory(SelectedGroup.Id);
        else if (Conversation is not null)
            response = await chatService.ClearConversationHistory(Conversation.id);
        else
            return;

        if (!response.status)
        {
            ErrorMessage(response.message);
            return;
        }

        _messages.Clear();
        _oldestCursor = null;
        _hasMoreOlder = false;
        _inJumpMode = false;
        ResetSearch();

        snackbar.Add(response.message, Severity.Success);
        StateHasChanged();
    }


    // ------------------------------------------------------- حذف گروه یا کانال

    private async Task DeleteChatDialog()
    {
        var isChannel = SelectedChannel is not null;
        var deletedId = SelectedChannel?.Id ?? SelectedGroup?.Id;
        if (deletedId is null) return;

        var title = CurrentChatTitle;

        var parameters = new DialogParameters<ConfirmDangerDialog>
    {
        { x => x.Title, isChannel ? "حذف کانال" : "حذف گروه" },
        { x => x.Message, $"«{title}» به همراه تمام پیام‌ها، فایل‌ها و اعضایش برای همیشه حذف می‌شود." },
        { x => x.ConfirmLabel, "حذف کن" },
        { x => x.ConfirmationText, title }
    };

        var dialog = await DialogService.ShowAsync<ConfirmDangerDialog>(
            isChannel ? "حذف کانال" : "حذف گروه", parameters);

        var result = await dialog.Result;
        if (result.Canceled) return;

        var response = isChannel
            ? await _channelservice.DeleteChannel(deletedId.Value)
            : await chatService.DeleteGroup(deletedId.Value);

        if (!response.status)
        {
            ErrorMessage(response.message);
            return;
        }

        snackbar.Add(response.message, Severity.Success);

        _messages.Clear();
        ResetSearch();

        // والد باید پاک کند، نه ما — SelectedGroup اینجا فقط یک پارامتر است
        if (OnChatDeleted.HasDelegate)
            await OnChatDeleted.InvokeAsync(deletedId.Value);
    }
    private async Task HandleClearedHistory()
    {
        if (ClearedHistoryId is null) return;

        // فقط اگر همین گفتگو باز است صفحه را خالی کن
        var current = SelectedChannel?.Id ?? SelectedGroup?.Id ?? Conversation?.id;

        if (current == ClearedHistoryId.Value)
        {
            _messages.Clear();
            _oldestCursor = null;
            _hasMoreOlder = false;
            _inJumpMode = false;
            _highlightedMessageId = null;
            ResetSearch();

            snackbar.Add("تاریخچه‌ی این گفتگو پاک شد.", Severity.Info);
        }

        ClearedHistoryId = null;

        // به والد خبر بده تا فیلدش را خالی کند، وگرنه رندر بعدی دوباره
        // همین مقدار را پاس می‌دهد و پیام تکراری نشان داده می‌شود.
        if (OnHistoryCleared.HasDelegate)
            await OnHistoryCleared.InvokeAsync();
    }

    // -------------------------------------------- دریافت رویداد از سایر اعضا
    // این را کنار بقیه‌ی hubConnection.On<...> در همان متد راه‌اندازی هاب بگذار.


    private bool CanManageCurrentChat =>
    (SelectedChannel is not null && SelectedChannel.channel.CreatorId == CurrentUser?.Id)
    || (SelectedGroup is not null && SelectedGroup.channel.CreatorId == CurrentUser?.Id);

    private string CurrentChatTitle =>
        SelectedChannel?.Name ?? SelectedGroup?.Name ?? OtherUser?.Name ?? string.Empty;

    #endregion
}
