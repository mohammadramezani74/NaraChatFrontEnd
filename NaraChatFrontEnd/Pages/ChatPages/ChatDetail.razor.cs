
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using NaraChat.Application.Services;
using NaraChat.Contract.Models.Chat.Conversation;
using NaraChat.Contract.Models.Users;
using NaraChatFrontEnd.Models.BaseModels;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace NaraChatFrontEnd.Pages.ChatPages;

public partial class ChatDetail:ComponentBase
{
    #region parameters
    [CascadingParameter(Name = "Theme")]
    public ThemeChanging ThemeCascading { get; set; }
    private MudTextField<string> messageInput;
    private CancellationTokenSource? _cancellationTokenSource;
    public bool IsUploading { get; set; } = false;
    public UserAvatar Avatars { get; set; } = new();

    [Parameter]
    public Guid id { get; set; }
    DateTime? lastDate = null;
    [Parameter]
    [Display(Name ="ایونت سین پیام کاربر")]
    public EventCallback<MessageSeenDto> OnMessageSeen { get; set; }
    [Display(Name = "المنت بادی مسیج برای اسکرول")]
    private ElementReference _chatContainer;
    [Display(Name = "کاربر فعلی")]
    public UserDto? CurrentUser { get; set; }
    [Display(Name = "مکالمه انتخابی")]
    public PrivateConversationDto? Conversation { get; set; } = new();
    [Display(Name = "لیست پیام ها")]
    private List<ChatMessageDto> _messages = new List<ChatMessageDto>();
    [Display(Name = "کارد پیکر ایموجی")]
    private bool ShowEmojiPicker = false;

    [Parameter]
    [Display(Name = "کاربر انتخاب شده")]
    public UserDto? OtherUser { get; set; } = null;
    [Parameter]
    [Display(Name = "ایونت ارسال پیام جدید به ساید مقابل")]
    public EventCallback IncommingMessageRecieved { get; set; }
    [Parameter]
    [Display(Name = "پیام دریافتی جدید ساید مقابل")]
    public ChatMessageDto? NewIncomingMessage { get; set; }
    [Parameter]
    [Display(Name = "پیام دریافتی جدید ساید مقابل")]
    public EditedMessageDto? NewIncomingEditedMessage { get; set; }
    [Parameter]
    [Display(Name = "ایونت بستن سشن مکالمه")]
    public EventCallback<bool> OnCancel { get; set; }
    [Parameter]
    [Display(Name = "لیست پیام های دیده شده")]
    public List<Guid>? MessagedSeened { get; set; }
    [Display(Name = "پیام ورودی نوار")]
    private string NewMessage = "";
    [Display(Name = "هندل همزمانی")]
    private Guid? PreviousSelectedUserId = null;
    [Display(Name = "ایونت کم شدن تعداد پیغام ها")]
    [Parameter]
    public EventCallback<(Guid otherUserId, int MessagesCountSeened)> ReduceMessageSeenedCount { get; set; }
    public bool EditMode { get; set; } = false;
    public EditedMessageDto? EditedMessage { get; set; } = null;
    [Parameter]
    public Guid? DeletedMessageId { get; set; }
    public bool ReplyMode { get; set; } = false;
    [Parameter]
    public string?  MyAvatar { get; set; }
    public ChatMessageDto? SelectedMessageToReply { get; set; } = null;
    [Parameter]
    public EventCallback OnSendMessageSession { get; set; }
    [Parameter]
    public bool IsMobile { get; set; }
    [Parameter]
    public EventCallback BackToUsersList { get; set; }
    [Parameter]
    public EventCallback<Guid> SendReaction { get; set; }
    [Parameter]
    public EventCallback NullReaction { get; set; }
    [Parameter]
    public TypingReactionDto? reactionType { get; set; } = null;
    [Parameter]
    public EventCallback<MessageReaction> MessageReactioncallBack { get; set; }
    [Parameter]
    public MessageReaction? EmojiReaction { get; set; }
    [Parameter]
    public EventCallback<MessageReaction> NullEmojiReactioncallBack { get; set; }
    [Parameter]
    public EventCallback ClearMissedMessageUser { get; set; }
    [Parameter]
    public List<ChatMessageDto> MissedUserMessage { get; set; }
    public string? ReactionTitle { get; set; } = null;
    [Parameter]
    public EventCallback<ChatMessageDto> changeUserLastMessage { get; set; }
    #endregion

    private async Task LoadMesages(int count=15)
    {
        try
        {

      
   
        _messages.Clear();
        Conversation = await chatService.ReadyOrCreateConversationBy(OtherUser.Id);
        var other = Conversation!.users.Where(c => c.Id != CurrentUser!.Id).FirstOrDefault();

        var messages = await messageService.LoadMessages(Conversation!.id,count);

        if (messages?.Any() == true)
        {
            _messages = messages;
            var messageIds = messages.Where(x =>  !x.IsSeen).Select(m => m.Id).ToList();
          if(messageIds?.Count>0)
            await HandleMessageSeen(new MessageSeenDto(messageIds, other.Id,Conversation.id,CurrentUser.Id));
            var unreadedMessageCount = _messages.Where(x => x.UserId == other.Id && !x.IsSeen).Count();
    

            await ReduceMessageSeenedCount.InvokeAsync((OtherUser.Id, 1100));
            await trueScroll.InvokeAsync();
        }
        else
        {
        }
        }
        catch (Exception ex)
        {

            throw;
        }
    }
    private async Task LoadMoreMesages(int count = 15)
    {

 
        Conversation = await chatService.ReadyOrCreateConversationBy(OtherUser.Id);
        var other = Conversation!.users.Where(c => c.Id != CurrentUser!.Id).FirstOrDefault();

        var messages = await messageService.LoadMessages(Conversation!.id, count);
        var newMessages = messages.Except(_messages, new MessageComparer()).ToList();
        if (newMessages.Any())
        {
            _messages.AddRange(newMessages);

       
            _messages.Sort((a, b) => a.SendAt.CompareTo(b.SendAt));

        }
    }
    [Parameter]
    public bool LoadScroll { get; set; }
    [Parameter]
    public EventCallback FalseScroll { get; set; }
    [Parameter]
    public EventCallback trueScroll {  get; set; }
    private async Task SendMessage(ChatMessageDto? Mapmessage=null)
    {
        
        if (!string.IsNullOrWhiteSpace(NewMessage)|| Mapmessage!=null)
        {
            var me = Conversation.users.Where(c => c.Id == CurrentUser!.Id).FirstOrDefault();
            var other = Conversation.users.Where(c => c.Id != CurrentUser!.Id).FirstOrDefault();

            if (OtherUser.IsBlocked || CurrentUser.IsBlocked || OtherUser.OtherUserBlocked)
            {
                ErrorMessage("این مکالمه مسدود شده است و امکان ارسال پیام وجود ندارد.");
                return;
            }
            var messagenew = NewMessage;
            Guid? ParentId = null;
            if (ReplyMode ) {
                ParentId = SelectedMessageToReply!.Id;
            }
            NewMessage = string.Empty;

            var newMessage = new ChatMessageDto
            {
                Id =Guid.Empty,
                SendAt = DateTime.Now,
                SenderName = CurrentUser.Name,
                IsMine = true,
                Content = messagenew,
                Type = 0,
                UserId = CurrentUser.Id,
                ParentId = ParentId,




            };
            if (Mapmessage != null) {
                newMessage= Mapmessage;
                newMessage.ParentId = ParentId;
                newMessage.UserId=CurrentUser.Id;
                newMessage.IsMine = true;
                newMessage.SenderName = CurrentUser.Name;
            }
            if(!EditMode) 
            _messages.Add(newMessage);
            await messageInput.FocusAsync();
            StateHasChanged();

            await OnSendMessageSession.InvokeAsync();
         

            if (EditMode)
            {
                EditedMessage!.Message = messagenew;
                var response = await messageService.EditMessageAsync(EditedMessage!);
                if (response.Item1)
                {
                    SuccessMessage();
                }
                else
                {
                    ErrorMessage(response.message);
                }
                EditMode = false;
           
               var message= _messages.Where(x => x.Id == EditedMessage.Id).Single();
                message.Content = EditedMessage.Message;
                messagenew = string.Empty;
                message.isEdited = true;
                return;

            }
        

            var result = await messageService.SendMessageAsync(Conversation!.id, newMessage.Content, ParentId, newMessage.Latitude,newMessage.Longitude);
            if (result.Item1)
            {
                changeUserLastMessage.InvokeAsync();
             

                newMessage.Id=result.MessageId;
                await ScrollToBottom();
                NewMessage = string.Empty;
                SelectedMessageToReply = null;
                ReplyMode = false;


            }
            ;

        }
        else
        {
            ErrorMessage("ارسال مسیج با مشکل مواجه شد");
        }

    }
  private async Task AddMessageToUi()
    {
        await OnSendMessageSession.InvokeAsync();
        Guid? ParentId = null;
        if (ReplyMode)
        {
            ParentId = SelectedMessageToReply!.Id;
        }
        var other = Conversation.users.Where(c => c.Id != CurrentUser!.Id).FirstOrDefault();
        var newMessage = new ChatMessageDto
        {
            Id = Guid.Empty,
            SendAt = DateTime.Now,
            SenderName = CurrentUser.Name,
            IsMine = true,
            Content = NewMessage,
            Type = 0,
            UserId = CurrentUser.Id,
            ParentId = ParentId,



        };
        _messages.Add(newMessage);
        await ScrollToBottom();
        await messageInput.FocusAsync();
        NewMessage = string.Empty;
        SelectedMessageToReply = null;
        ReplyMode = false;
        await trueScroll.InvokeAsync();
 

        var newMessageForUsers = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            SendAt = DateTime.Now,
            SenderName = CurrentUser.Name,
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
            if (response.Item1)
            {
                SuccessMessage();
            }
            else
            {
                ErrorMessage(response.message);
            }
            EditMode = false;

            var message = _messages.Where(x => x.Id == EditedMessage.Id).Single();
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
    private async Task UploadFile(IBrowserFile file,string caption)
    {
        try
        {

    
        if (file is null) 
        {
            return;
        }
        IsUploading = true;
            StateHasChanged();
        _cancellationTokenSource=new CancellationTokenSource();
        var extension = Path.GetExtension(file.Name);
        var contentType = file.ContentType;
        if (string.IsNullOrEmpty(contentType)) {
            contentType = "application/octet-stream";
        }
        var uploaddto = new UploadFileDto(Conversation!.id,
            caption,
            new StreamContent(file.OpenReadStream(100*1024*1024,_cancellationTokenSource.Token)),
            contentType,
           extension,
           file.Name
            );
        var result = await messageService.UploadChatFileAsync(uploaddto, _cancellationTokenSource.Token);
        if (result.Item1)
        {
            var me = Conversation.users.Where(c => c.Id == CurrentUser!.Id).FirstOrDefault();
            var newMessage = new ChatMessageDto
            {
                Id = result.result!.MessageId,
                SendAt = DateTime.Now,
                SenderName = me!.Name,
                IsMine = true,
                Content = caption,
                Type =(MessageType) result.result.MessageType,
                UserId = CurrentUser.Id,
          FileContent=new ChatFilesDto
          {
              FileId = result.result!.FileId,
              FileName= file.Name,
              FileSize= file.Size.ToString()
          }
            };
            _messages.Add(newMessage);
                IsUploading = false;
            await ScrollToBottom();
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
        public bool Equals(ChatMessageDto x, ChatMessageDto y)
        {
            return x.Id == y.Id; 
        }

        public int GetHashCode(ChatMessageDto obj)
        {
            return obj.Id.GetHashCode();
        }
    }
    public async Task BackToUserList()
    {
       await BackToUsersList.InvokeAsync();
    }
    public async Task SendReactions(MessageReaction reaction)
    {
        await MessageReactioncallBack.InvokeAsync(reaction);
    }
    [JSInvokable]
   public  async Task ReceiveLocationFromJS(object coords)
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
                StateHasChanged(); // لازم چون UI واکنش باید بروز بشه
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
        }
    }

    private async Task HandleMissedMessagesAsync()
    {
        if (MissedUserMessage?.Count > 0)
        {
            var newMessages = MissedUserMessage
                .Where(m => !_messages.Any(existing => existing.Id == m.Id))
                .OrderBy(x => x.SendAt)
                .ToList();

            if (newMessages.Count > 0)
            {
                _messages.AddRange(newMessages);
                await ClearMissedMessageUser.InvokeAsync();
            }
        }
    }

    private void HandleSeenMessages()
    {
        if (MessagedSeened?.Count > 0)
        {
            var messages = _messages.Where(m => MessagedSeened.Contains(m.Id)).ToList();
            foreach (var chatMessage in messages)
            {
                chatMessage.IsSeen = true;
            }
        }
    }

    private async Task HandleUserChangedAsync()
    {
        if (OtherUser != null && OtherUser.Id != PreviousSelectedUserId)
        {
            if (CurrentUser == null)
            {
                CurrentUser = await ((CustomAuthenticationStateProvider)stateprovider).GetUserInfoAsync();
            }

            PreviousSelectedUserId = OtherUser.Id;

            Loading = true;
            NewMessage = string.Empty;

            await LoadMesages();

            CurrentUser.avatar = MyAvatar;
            Loading = false;

            StateHasChanged();
        }
    }

    private async Task HandleScrollAsync()
    {
        if (LoadScroll)
        {
            await ScrollToBottom();
            await FalseScroll.InvokeAsync();
        }
    }


}
