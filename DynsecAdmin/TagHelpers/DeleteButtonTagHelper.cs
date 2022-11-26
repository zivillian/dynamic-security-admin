using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DynsecAdmin.TagHelpers
{
    public class DeleteButtonTagHelper:TagHelper
    {
        public string AspName { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var id = Guid.NewGuid();
            output.TagName = "button";
            output.Attributes.SetAttribute("type", "button");
            output.Attributes.SetAttribute("data-bs-toggle", "modal");
            output.Attributes.SetAttribute("data-bs-target", $"#deletebutton_{id}");
            output.PostElement.AppendHtml(
                $"<div class=\"modal fade\" id=\"deletebutton_{id}\" tabindex=\"-1\">" +
                $"  <div class=\"modal-dialog\">" +
                $"    <div class=\"modal-content\">" +
                $"      <div class=\"modal-header\">" +
                $"        <h5 class=\"modal-title\">Are you sure?</h5>" +
                $"        <button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"modal\" aria-label=\"Close\"></button>" +
                $"      </div>" +
                $"      <div class=\"modal-body\">" +
                $"        <p>Do you really want to delete {WebUtility.HtmlEncode(AspName)}?</p>" +
                $"      </div>" +
                $"      <div class=\"modal-footer\">" +
                $"        <button type=\"button\" class=\"btn btn-secondary\" data-bs-dismiss=\"modal\">Close</button>" +
                $"        <button type=\"submit\" class=\"btn btn-danger\">Delete</button>" +
                $"      </div>" +
                $"    </div>" +
                $"  </div>" +
                $"</div>");
        }
    }
}
