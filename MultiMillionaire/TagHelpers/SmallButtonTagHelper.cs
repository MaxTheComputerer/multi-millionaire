using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MultiMillionaire.TagHelpers;

public class SmallButtonTagHelper : TagHelper
{
    public string Id { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string Onclick { get; set; } = string.Empty;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "button";
        output.Attributes.SetAttribute("type", "button");
        output.Attributes.SetAttribute("class", $"small-button mx-2 px-4 {Class}");
        output.Attributes.SetAttribute("id", Id);
        output.Attributes.SetAttribute("onclick", Onclick);
        output.TagMode = TagMode.StartTagAndEndTag;
    }
}
