namespace NaraChatFrontEnd.Models.BaseModels
{
    public class ThemeChanging
    {
        public string backgroundColor  { get; set; }
        public string  Color { get; set; }
        public string  boxShadow { get; set; }
        public bool IsShowSetting { get; set; } = false;
        public string backGroundChatStyle { get; set; } = "height: 100%; overflow-y: auto;background-image:url(lib/assets/Images/light.jpg);    background-size: cover;background-repeat:no-repeat;background-position: center; background-attachment: scroll;";
        public string myColorChat { get; set; } = "rgb(235 198 255 / 56%)";
        public string OtherColorChat { get; set; } = "rgb(233 236 239 / 86%)";
        public MudBlazor.Color SeenColor { get; set; } = MudBlazor.Color.Secondary;
        public string ClockColor { get; set; } = "#270aff";
        public string MyColorReply { get; set; } = "#c4beff";
        public string OtherColorReply { get; set; } = "#f6effd";

        public ThemeChanging(string className, string Color, string boxShadow)
        {
            backgroundColor = className;
            this.Color = Color;
            this.boxShadow = boxShadow;
        }
    }
    
}
