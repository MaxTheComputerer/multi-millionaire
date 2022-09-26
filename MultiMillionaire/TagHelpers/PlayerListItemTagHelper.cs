using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MultiMillionaire.TagHelpers;

public class PlayerLiTagHelper : TagHelper
{
    public string Id { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string RightText { get; set; } = string.Empty;
    public string RightTextClass { get; set; } = string.Empty;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        output.PreContent.SetHtmlContent(
            $@"<div class=""row mb-2 mx-0 player-list {Class}"">
                <hr class=""col-1 question-line my-auto px-0""/>
                <div class=""col question"" id=""{Id}"">
                    <span class=""answer-diamond me-2"">◆</span>
                    <span class=""player-list-text"">");
        output.PostContent.SetHtmlContent(
            $@"</span>
            <span class=""player-list-text ms-auto me-1 {RightTextClass}"">{RightText}</span>
            <span class=""answer-diamond text-orange"">◆</span>
        </div>
        <hr class=""col-1 question-line my-auto px-0""/>
        </div>");
    }
}