using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace DotNetMvcWeb.TagHelpers
{
    [HtmlTargetElement("page-header")]
    public class PageHeaderTagHelper : TagHelper
    {
        public string Title { get; set; } = string.Empty;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // The outer element will be a <div>
            output.TagName = "div";
            
            // Set the robust flex classes, including flex-wrap as requested by the user
            output.Attributes.SetAttribute("class", "page-header d-flex justify-content-between align-items-center mb-4 flex-wrap");

            // Process the child elements (the "slot" contents like buttons or inputs)
            TagHelperContent childContent = await output.GetChildContentAsync();

            // Construct the title (h1 with m-0 so it centers properly)
            string titleHtml = $"<h1 class=\"m-0\">{Title}</h1>";
            
            // Construct the actions wrapper if there is any child content provided
            string actionsHtml = childContent.IsEmptyOrWhiteSpace 
                ? "" 
                : $"<div class=\"d-flex gap-2 align-items-center\">{childContent.GetContent()}</div>";

            // Inject the content
            output.Content.SetHtmlContent(titleHtml + actionsHtml);
        }
    }
}
