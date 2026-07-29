namespace NaraChatFrontEnd.Models.BaseModels
{
    public class ThemeChanging
    {
      
            public string backgroundColor { get; set; }
            public string Color { get; set; }
            public string boxShadow { get; set; }
            public bool IsShowSetting { get; set; } = false;

            public string backGroundChatStyle { get; set; } =
                "height: 100%; overflow-y: auto; background-image:url(lib/assets/Images/light.png); background-size: cover; background-repeat:no-repeat; background-position: center; background-attachment: scroll;";

            public string myColorChat { get; set; } = "rgba(219, 234, 254, 0.94)";
            public string OtherColorChat { get; set; } = "rgba(255, 255, 255, 0.94)";

            // جدید — متن حباب دیگر از تم به ارث نمی‌رسد
            public string MyTextColor { get; set; } = "#0F172A";
            public string OtherTextColor { get; set; } = "#0F172A";

            public MudBlazor.Color SeenColor { get; set; } = MudBlazor.Color.Primary;
            public string ClockColor { get; set; } = "#475569";

            public string MyColorReply { get; set; } = "#BFDBFE";
            public string OtherColorReply { get; set; } = "#EFF6FF";

            // جدید — نوار کناری کادر ریپلای
            public string ReplyAccent { get; set; } = "#1D6FD0";

            public ThemeChanging(string className, string Color, string boxShadow)
            {
                backgroundColor = className;
                this.Color = Color;
                this.boxShadow = boxShadow;
            }
        }
    
}
