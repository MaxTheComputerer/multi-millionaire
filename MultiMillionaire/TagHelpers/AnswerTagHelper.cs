using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MultiMillionaire.TagHelpers;

public class AnswerTagHelper : TagHelper
{
    public string Id { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string TextStyle { get; set; } = string.Empty;
    public string Position { get; set; } = "left";
    public bool HasDiamond { get; set; } = true;
    public string Onclick { get; set; } = string.Empty;
    public string Letter { get; set; } = string.Empty;
    private const string DiamondContent = @"<span class=""answer-diamond me-1"">◆</span>";


    private string GetLeftPreContent()
    {
        var content = $@"<hr class=""col question-line my-auto px-0""/>
            <div class=""col-sm-auto answer answer-left mb-2 mb-sm-0 {Class}"" id=""{Id}"" onclick=""{Onclick}"">";
        if (HasDiamond) content += DiamondContent;

        if (Letter != string.Empty)
            content += $@"<span class=""answer-letter me-1"">{Letter.ToUpper()}:</span>";

        content += $@"<span class=""answer-text"" style=""{TextStyle}"">";
        return content;
    }

    private static string GetLeftPostContent()
    {
        return "</span></div>";
    }

    private string GetRightPreContent()
    {
        var content = $@"<div class=""col-sm-auto answer answer-right {Class}"" id=""{Id}"" onclick=""{Onclick}"">";
        if (HasDiamond) content += DiamondContent;

        if (Letter != string.Empty)
            content += $@"<span class=""answer-letter me-1"">{Letter.ToUpper()}:</span>";

        content += $@"<span class=""answer-text"" style=""{TextStyle}"">";
        return content;
    }

    private static string GetRightPostContent()
    {
        return @"</span></div><hr class=""col question-line my-auto px-0""/>";
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        output.PreContent.SetHtmlContent(Position == "left" ? GetLeftPreContent() : GetRightPreContent());
        output.PostContent.SetHtmlContent(Position == "left" ? GetLeftPostContent() : GetRightPostContent());
    }
}