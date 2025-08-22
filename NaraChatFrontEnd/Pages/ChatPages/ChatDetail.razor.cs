using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using NaraChat.Application.Services;
using NaraChat.Contract.Models.Chat.Conversation;
using NaraChat.Contract.Models.Users;
using NaraChat.Contract.Utilities.FilesExtensions;
using NaraChatFrontEnd.Models.BaseModels;
using NaraChatFrontEnd.Pages.ChatPages.Components.DialogComponents;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using static NaraChatFrontEnd.Pages.ChatPages.Components.DialogComponents.UploadDialogComponent;

namespace NaraChatFrontEnd.Pages.ChatPages;

public partial class ChatDetail : ComponentBase, IAsyncDisposable
{
    #region parameters & fields

    [CascadingParameter(Name = "Theme")] public ThemeChanging ThemeCascading { get; set; }
    private MudTextField<string> messageInput;

    private CancellationTokenSource? _cancellationTokenSource;

    public bool Loading { get; set; } = false;
    private bool isRecording { get; set; } = false;
  [Parameter]  public UserDto? SelectedChannel { get; set; } = null;

    private DotNetObjectReference<ChatDetail>? objRef;

    public bool IsUploading { get; set; } = false;
    public UserAvatar Avatars { get; set; } = new();

    [Parameter] public Guid id { get; set; }
    DateTime? lastDate = null;

    [Parameter, Display(Name = "ایونت سین پیام کاربر")]
    public EventCallback<MessageSeenDto> OnMessageSeen { get; set; }

    [Display(Name = "المنت بادی مسیج برای اسکرول")]
    private ElementReference _chatContainer;

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

    [Display(Name = "ایونت کم شدن تعداد پیغام ها")]
    [Parameter] public EventCallback<(Guid otherUserId, int MessagesCountSeened)> ReduceMessageSeenedCount { get; set; }

    public bool EditMode { get; set; } = false;
    public EditedMessageDto? EditedMessage { get; set; } = null;

    [Parameter] public Guid? DeletedMessageId { get; set; }

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

    public string? ReactionTitle { get; set; } = null;

    [Parameter] public EventCallback<ChatMessageDto> changeUserLastMessage { get; set; }

    [Parameter] public bool LoadScroll { get; set; }

    [Parameter] public EventCallback FalseScroll { get; set; }

    [Parameter] public EventCallback trueScroll { get; set; }

    // اگر لازم بود:
    [Parameter] public EventCallback<UserDto?> GoToSettingInMobile { get; set; }

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

    #endregion

    #region lifecycle

    protected override async Task OnInitializedAsync()
    {
        CurrentUser = await ((CustomAuthenticationStateProvider)stateprovider).GetUserInfoAsync();
        objRef ??= DotNetObjectReference.Create(this);
        await OnSendMessageSession.InvokeAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (isProcessing) return;
        isProcessing = true;

        try
        {
            HandleDeletedMessage();
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
        if (firstRender)
        {
            objRef ??= DotNetObjectReference.Create(this);
            await js.InvokeVoidAsync("initScrollListener", _chatContainer, objRef);
            NeedScrollToBottom(); // بار اول پایین
        }

        if (_pendingScrollToBottom)
        {
            _pendingScrollToBottom = false;
            await js.InvokeVoidAsync("scrollToBottom", _chatContainer);
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
        }
        catch { /* ignore */ }
    }

    #endregion

    #region JS helpers

    private void NeedScrollToBottom() => _pendingScrollToBottom = true;

    [JSInvokable]
    public async Task LoadMoreMessages()
    {
        if (_messages.Count >= MessageCount)
        {
            IsloadedOldMessages = true;

            var previousScrollHeight = await js.InvokeAsync<int>("getScrollHeight", _chatContainer);

            MessageCount += 15;

            await LoadMoreMesages(MessageCount);

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

    public string? HandelParentMessage(Guid? ParentMessageId)
    {
        if (ParentMessageId == null) return null;
        var parent = _messages.FirstOrDefault(x => x.Id == ParentMessageId.Value);
        return parent?.Content;
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

        if (!result.Canceled && result.Data is UploadResult uploadResult)
        {
            await UploadFile(uploadResult.File, uploadResult.Caption ?? string.Empty);
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

    private async Task LoadMesages(int count = 15)
    {
        _messages.Clear();
        Conversation = await chatService.ReadyOrCreateConversationBy(OtherUser!.Id);
        var other = Conversation!.users.First(c => c.Id != CurrentUser!.Id);

        var messages = await messageService.LoadMessages(Conversation!.id, count);
        if (messages?.Any() == true)
        {
            _messages.AddRange(messages);

            var messageIds = messages.Where(x => !x.IsSeen).Select(m => m.Id).ToList();
            if (messageIds.Count > 0)
                await HandleMessageSeen(new MessageSeenDto(messageIds, other.Id, Conversation.id, CurrentUser!.Id));

            await ReduceMessageSeenedCount.InvokeAsync((OtherUser.Id, 1100));

            NeedScrollToBottom();
            await trueScroll.InvokeAsync();
        }

        // ✅ بازیابی پیش‌نویس برای این گفتگو
        if (OtherUser != null && _drafts.TryGetValue(OtherUser.Id, out var draft))
            NewMessage = draft;
    }
    public async Task LoadChannelMesages(int count = 30)
    {
        _messages.Clear();



        var messages = await _channelservice.LoadChannelMessages(SelectedChannel!.Id, count);
        if (messages?.Any() == true)
        {
            _messages.AddRange(messages);

            var messageIds = messages.Where(x => !x.IsSeen).Select(m => m.Id).ToList();

            NeedScrollToBottom();
            await trueScroll.InvokeAsync();
        }
        if (OtherUser != null && _drafts.TryGetValue(OtherUser.Id, out var draft))
            NewMessage = draft;
    }

    private async Task LoadMoreMesages(int count = 15)
    {
        Conversation ??= await chatService.ReadyOrCreateConversationBy(OtherUser!.Id);
        var messages = await messageService.LoadMessages(Conversation!.id, count);

        var known = new HashSet<Guid>(_messages.Select(m => m.Id));
        var newMessages = messages.Where(m => !known.Contains(m.Id)).ToList();

        if (newMessages.Any())
        {
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
            messagenew = string.Empty;
            msg.isEdited = true;
            return;
        }

        var result = await messageService.SendMessageAsync(Conversation!.id, newMessage.Content, ParentId, newMessage.Latitude, newMessage.Longitude);
        if (result.Item1)
        {
            await changeUserLastMessage.InvokeAsync();
            newMessage.Id = result.MessageId;

            // ✅ بعد از ارسال موفق، پیش‌نویس این گفتگو پاک می‌شود
            if (OtherUser != null) _drafts.Remove(OtherUser.Id);

            NeedScrollToBottom();

            NewMessage = string.Empty;
            SelectedMessageToReply = null;
            ReplyMode = false;
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

    public void cancelUpload()
    {
        if (_cancellationTokenSource is not null)
        {
            _cancellationTokenSource.Cancel();
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
            _messages.Add(NewIncomingMessage);
            await IncommingMessageRecieved.InvokeAsync();
            NewIncomingMessage = null;

            NeedScrollToBottom();
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
                _messages.AddRange(newMessages);
                await ClearMissedMessageUser.InvokeAsync();

                NeedScrollToBottom();
            }
        }
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
        if (SelectedChannel != null)
        {
            await LoadChannelMesages();
            if (_drafts.TryGetValue(OtherUser.Id, out var draft))
                NewMessage = draft;
            Loading = false;

            StateHasChanged();
        }
        else if (OtherUser != null && OtherUser.Id != PreviousSelectedUserId && SelectedChannel == null)
        {
            // ✅ قبل از سوییچ، پیش‌نویس گفتگو قبلی ذخیره می‌شود
            if (PreviousSelectedUserId.HasValue)
                _drafts[PreviousSelectedUserId.Value] = NewMessage ?? string.Empty;

            if (CurrentUser == null)
            {
                CurrentUser = await ((CustomAuthenticationStateProvider)stateprovider).GetUserInfoAsync();
            }

            PreviousSelectedUserId = OtherUser.Id;

            Loading = true;
            NewMessage = string.Empty;

                await LoadMesages();

            // ✅ بعد از لود، اگر برای کاربر فعلی پیش‌نویس داریم، برگردان
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

    #endregion
}
