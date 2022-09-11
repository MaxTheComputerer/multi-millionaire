using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MultiMillionaire.TagHelpers;

[HtmlTargetElement("textbox", Attributes = "text,id", TagStructure = TagStructure.WithoutEndTag)]
public class TextBoxTagHelper : TagHelper
{
    public string Text { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        output.Content.SetHtmlContent(
            $@"<div class=""row mb-4 mx-0"">
                <hr class=""col-1 question-line my-auto px-0""/>
                <div class=""col question"" id=""{Id}"">{Text}</div>
                <hr class=""col-1 question-line my-auto px-0""/>
            </div>");
        output.TagMode = TagMode.SelfClosing;
    }
}
