using Microsoft.AspNetCore.Components.Authorization;
using NaraChat.Application.Services;
using NaraChat.Application.Services.Auth;
using NaraChat.Application.Services.ChatServices;
using NaraChat.Application.Services.ChatServices.Conversation;
using NaraChat.Application.Services.LocalStorage;
using NaraChat.Application.Services.TokenProvider;
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
            services.AddScoped<IMessageService, MessageService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("apiwithAuth");
                return new MessageService(httpClient);
            });


            return services;
        }
    }
}
