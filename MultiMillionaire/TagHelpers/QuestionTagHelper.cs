using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MultiMillionaire.TagHelpers;

public class QuestionTagHelper : TagHelper
{
    public string Id { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string RowClass { get; set; } = string.Empty;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        output.PreContent.SetHtmlContent(
            $@"<div class=""row mb-2 {RowClass}"">
                <hr class=""col question-line my-auto px-0""/>
                <div class=""col-md-auto question {Class}"" id=""{Id}"">");
        output.PostContent.SetHtmlContent(
            @"</div>
                <hr class=""col question-line my-auto px-0""/>
            </div>");
    }
}