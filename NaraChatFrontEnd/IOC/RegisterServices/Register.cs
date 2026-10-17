using Microsoft.AspNetCore.Components.Authorization;
using NaraChat.Application.Services;
using NaraChat.Application.Services.Auth;
using NaraChat.Application.Services.ChatServices;
using NaraChat.Application.Services.ChatServices.Channels;
using NaraChat.Application.Services.ChatServices.Conversation;
using NaraChat.Application.Services.ContactTasks;
using NaraChat.Application.Services.Bots;
using NaraChat.Application.Services.LocalStorage;
using NaraChat.Application.Services.TokenProvider;
using NaraChat.Application.Services.Upload;
using NaraChat.Application.Settings;
using NaraChatFrontEnd.Layout;

namespace NaraChatFrontEnd.IOC.RegisterServices
{
    public  static class Register
    {
        public static IServiceCollection RegisterSevices(this IServiceCollection services) 
        {
            services.AddHttpClient<IAuthService, AuthService>(client =>
            {
                client.BaseAddress = new Uri(SiteSettings.ApiUrl);
            });
            services.AddScoped<ILocalStorageService, LocalStorageService>();
            services.AddScoped<ITokenService, TokenService>();
           services.AddScoped<CustomAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
            services.AddScoped<JwtAuthorizationMessageHandler>();
            services.AddSingleton<VersionCheckService>();
            services.AddHttpClient("apiwithAuth", client =>
            {
                client.BaseAddress = new Uri(SiteSettings.ApiUrl);
            }).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();
        
            services.AddScoped<IUserChatService, UserChatService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new UserChatService(httpClient);
            });
            services.AddScoped<IConversationService, ConversationService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new ConversationService(httpClient);
            });
            services.AddScoped<IChannelService, ChannelService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new ChannelService(httpClient);
            });
            services.AddScoped<IMessageService, MessageService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new MessageService(httpClient);
            });

            services.AddScoped<IContactTaskService, ContactTaskService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new ContactTaskService(httpClient);
            });
            services.AddScoped<ContactTaskStore>();
            services.AddScoped<IBotService, BotService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new BotService(httpClient);
            });
            services.AddScoped<IBotAdminService, BotAdminService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new BotAdminService(httpClient);
            });
            services.AddScoped<BotStore>();
            services.AddScoped<NaraChat.Application.Services.Admin.IAdminDashboardService, NaraChat.Application.Services.Admin.AdminDashboardService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new NaraChat.Application.Services.Admin.AdminDashboardService(httpClient);
            });
            services.AddScoped<NaraChat.Application.Services.Admin.IAdminUsersService, NaraChat.Application.Services.Admin.AdminUsersService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new NaraChat.Application.Services.Admin.AdminUsersService(httpClient);
            });
            services.AddScoped<NaraChat.Application.Services.Admin.IAdminRoomsService, NaraChat.Application.Services.Admin.AdminRoomsService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new NaraChat.Application.Services.Admin.AdminRoomsService(httpClient);
            });
            services.AddScoped<NaraChat.Application.Services.Admin.IAdminBroadcastService, NaraChat.Application.Services.Admin.AdminBroadcastService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new NaraChat.Application.Services.Admin.AdminBroadcastService(httpClient);
            });
            services.AddScoped<NaraChat.Application.Services.Admin.IAdminReportsService, NaraChat.Application.Services.Admin.AdminReportsService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new NaraChat.Application.Services.Admin.AdminReportsService(httpClient);
            });
            services.AddScoped<NaraChat.Application.Services.Admin.IAdminSettingsService, NaraChat.Application.Services.Admin.AdminSettingsService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new NaraChat.Application.Services.Admin.AdminSettingsService(httpClient);
            });
            services.AddScoped<NaraChat.Application.Services.Admin.IAppInfoService, NaraChat.Application.Services.Admin.AppInfoService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new NaraChat.Application.Services.Admin.AppInfoService(httpClient);
            });
            services.AddScoped<IBrowserUploader, BrowserUploader>();
            return services;
        }
    }
}
