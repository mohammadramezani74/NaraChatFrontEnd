
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using NaraChat.Contract.Models.Chat.Conversation;
using NaraChat.Contract.Models.Users;
using NaraChatFrontEnd.Models.BaseModels;
using System.ComponentModel.DataAnnotations;

namespace NaraChatFrontEnd.Pages.ChatPages;

public partial class ChatDetail:ComponentBase
{
    #region parameters
    [CascadingParameter(Name = "Theme")]
    public ThemeChanging ThemeCascading { get; set; }
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
    #endregion

    private async Task LoadMesages(int count=20)
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
            await HandleMessageSeen(new MessageSeenDto(messageIds, other.Id));
            var unreadedMessageCount = _messages.Where(x => x.UserId == other.Id && !x.IsSeen).Count();
            StateHasChanged();
           
            await ReduceMessageSeenedCount.InvokeAsync((OtherUser.Id, 1100));
        }
        else
        {
        }
    }
    private async Task LoadMoreMesages(int count = 20)
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
    private async Task SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(NewMessage))
        {
            await OnSendMessageSession.InvokeAsync();
            Guid? ParentId = null;
            if (ReplyMode)
            {
                ParentId = SelectedMessageToReply!.Id;
            }
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
           
               var message= _messages.Where(x => x.Id == EditedMessage.Id).Single();
                message.Content = EditedMessage.Message;  
                NewMessage = string.Empty;
                message.isEdited = true;
                return;

            }
        

            var result = await messageService.SendMessageAsync(Conversation!.id, NewMessage, ParentId);
            if (result.Item1)
            {
               
                var me = Conversation.users.Where(c => c.Id == CurrentUser!.Id).FirstOrDefault();
                var other = Conversation.users.Where(c => c.Id != CurrentUser!.Id).FirstOrDefault();
                var newMessage = new ChatMessageDto
                {
                    Id = result.MessageId,
                    SendAt = DateTime.Now,
                    SenderName = me.Name,
                    IsMine = true,
                    Content = NewMessage,
                    Type = 0,
                    UserId = CurrentUser.Id,
                    ParentId= ParentId,



                };
                _messages.Add(newMessage);
                await ScrollToBottom();
                NewMessage = string.Empty;
                SelectedMessageToReply = null;
                ReplyMode = false;

            };

        }
        else
        {
            ErrorMessage("ارسال مسیج با مشکل مواجه شد");
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
}
