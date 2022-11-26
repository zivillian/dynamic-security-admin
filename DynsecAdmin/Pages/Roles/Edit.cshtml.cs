using Dynsec;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Dynsec.DTO;

namespace DynsecAdmin.Pages.Roles
{
    public class EditModel : PageModel
    {
        private readonly DynsecClient _dynsec;

        public string Rolename { get; set; }

        [BindProperty]
        [Display(Name = "Text name")]
        public string? Name { get; set; }

        [BindProperty]
        [Display(Name = "Text description")]
        public string? Description { get; set; }

        public Acl[] Acls { get; set; }

        public EditModel(DynsecClient dynsec)
        {
            _dynsec = dynsec;
        }

        public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                var role = await _dynsec.GetRoleAsync(id, cancellationToken);
                Rolename = role.Rolename;
                Name = role.Name;
                Description = role.Description;
                Acls = role.Acls ?? Array.Empty<Acl>();
                return Page();
            }
            catch (DynsecProtocolException ex) when(ex.Error == "Role not found")
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var acls = await _dynsec.GetRoleAsync(id, cancellationToken);
                    Acls = acls.Acls ?? Array.Empty<Acl>();
                    return Page();
                }
                var role = new Role
                {
                    Rolename = id,
                    Name = Name ?? String.Empty,
                    Description = Description ?? String.Empty
                };
                await _dynsec.ModifyRoleAsync(role, cancellationToken);
                return RedirectToPage("Index");
            }
            catch (DynsecProtocolException ex) when(ex.Error == "Role not found")
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id, CancellationToken cancellationToken)
        {
            await _dynsec.DeleteRoleAsync(id, cancellationToken);
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostAddAclAsync(string id, Acl acl, CancellationToken cancellationToken)
        {
            await _dynsec.AddRoleACLAsync(id, acl, cancellationToken);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAclAsync(string id, AclType type, string topic, CancellationToken cancellationToken)
        {
            await _dynsec.RemoveRoleACLAsync(id, type, topic, cancellationToken);
            return RedirectToPage();
        }
    }
}
