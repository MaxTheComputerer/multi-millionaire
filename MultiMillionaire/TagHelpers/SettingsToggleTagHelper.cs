using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MultiMillionaire.TagHelpers;

public class SettingsToggleTagHelper : TagHelper
{
    public string Id { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public bool Checked { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        output.PreContent.SetHtmlContent(
            $@"<div class=""form-check form-switch {Class}"">
                <input class=""form-check-input"" type=""checkbox"" role=""switch"" id=""{Id}"" onchange=""settings.updateSwitchSetting('{Id}')"" " +
            (Checked ? "checked" : "") + $@">
                <label class=""form-check-label"" for=""{Id}"">");
        output.PostContent.SetHtmlContent(
            @"</label>
            </div>");
    }
}