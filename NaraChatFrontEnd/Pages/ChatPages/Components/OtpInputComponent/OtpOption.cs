namespace BlazorOTPInput.Pages.OtpInputComponent
{
    public class OtpOption: BaseOtpOption
    {
        public int Space { get; set; } = 2;
        public string ComponentStyle() => $"gap:{Space}px;";
    }

    public class BaseOtpOption
    {
        public OtpViewMode ViewMode { get; set; }
        public int Width { get; set; } = 45;
        public int Height { get; set; } = 45;
        public int BorderRadius { get; set; } = 4;
        public string FontFamily { get; set; } = "Tahoma";
        public int FontSize { get; set; } = 14;
        public string BorderColor { get; set; } = "#dcdcdc";
        public string BorderColorOnFocus { get; set; } = "darkblue";
        public string Style() => ViewMode switch
        {
            OtpViewMode.Default => $"""
                    width: {Width}px; 
                    height: {Height}px;
                    border: 1px solid {BorderColor};
                    border-radius: {BorderRadius}px;
                    font-family:{FontFamily};
                    font-size:{FontSize}px;
                    """,
            OtpViewMode.Underlined => $"""
                    width: {Width}px;
                    height: {Height}px;
                    border:0;
                    border-bottom: 2px solid {BorderColor};
                    border-radius: 0px; 
                    font-family:{FontFamily};
                    font-size:{FontSize}px;
                    """
        };
        public string OnFocusStyle() => ViewMode switch
        {
            OtpViewMode.Default => $"""
                    width: {Width}px;
                    height: {Height}px;
                    border: 1px solid {BorderColorOnFocus};
                    border-radius: {BorderRadius}px;
                    font-family:{FontFamily};
                    font-size:{FontSize}px;
                    """,
            OtpViewMode.Underlined => $"""
                    width: {Width}px;
                    height: {Height}px;
                    border:0; 
                    border-bottom: 2px solid {BorderColorOnFocus};
                    border-radius: 0px;
                    font-family:{FontFamily};
                    font-size:{FontSize}px;
                    """
        };
    }

    public enum OtpViewMode
    {
        Default,
        Underlined
    }
}
