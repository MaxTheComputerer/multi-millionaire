using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MultiMillionaire.TagHelpers;

public class TextboxTagHelper : TagHelper
{
    public string Id { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        output.PreContent.SetHtmlContent(
            $@"<div class=""row mb-4 mx-0 {Class}"">
                <hr class=""col-1 question-line my-auto px-0""/>
                <div class=""col question"" id=""{Id}"">");
        output.PostContent.SetHtmlContent(
            @"</div>
                <hr class=""col-1 question-line my-auto px-0""/>
            </div>");
    }
}
