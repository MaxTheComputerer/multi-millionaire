using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MultiMillionaire.TagHelpers;

public class WebpTagHelper : TagHelper
{
    public string Name { get; set; } = string.Empty;
    public string Alt { get; set; } = string.Empty;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "picture";
        output.PostContent.SetHtmlContent(
            $@"<source type=""image/webp"" srcset=""/images/webp/{Name}.webp""/>
            <img src=""/images/png/{Name}.png"" alt=""{Alt}""/>");
    }
}
