using Dynsec;
using Dynsec.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Xml.Linq;

namespace DynsecAdmin.Pages.Groups
{
    public class EditModel : PageModel
    {
        private readonly DynsecClient _dynsec;

        private string[] _availableClients { get; set; }

        private string[] _availableRoles { get; set; }

        public bool Self
        {
            get
            {
                var self = User.FindFirstValue(ClaimTypes.Upn);
                return Clients.Any(x => x.Username == self);
            }
        }

        public string Groupname { get; set; }

        [BindProperty]
        [Display(Name = "Text name")]
        public string? Name { get; set; }

        [BindProperty]
        [Display(Name = "Text description")]
        public string? Description { get; set; }

        public RolePriority[] Roles { get; set; }

        public ClientReference[] Clients { get; set; }

        public IEnumerable<SelectListItem> AvailableClients => _availableClients
            .Where(x => Clients.All(c => c.Username != x))
            .OrderBy(x => x)
            .Select(x => new SelectListItem(x, x, false, User.IsSelf(x)));

        public IEnumerable<SelectListItem> AvailableRoles => _availableRoles
            .Where(x => Roles.All(r => r.Rolename != x))
            .OrderBy(x => x)
            .Select(x => new SelectListItem(x, x));

        public EditModel(DynsecClient dynsec)
        {
            _dynsec = dynsec;
        }

        public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                var selectListItems = LoadSelectlistItemsAsync(cancellationToken);
                var group = await _dynsec.GetGroupAsync(id, cancellationToken);
                Groupname = group.Groupname;
                Name = group.Name;
                Description = group.Description;
                Roles = group.Roles ?? Array.Empty<RolePriority>();
                Clients = group.Clients ?? Array.Empty<ClientReference>();
                await selectListItems;
                return Page();
            }
            catch (DynsecProtocolException ex) when (ex.Error == "Group not found")
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
                    var selectListItems = LoadSelectlistItemsAsync(cancellationToken);
                    var data = await _dynsec.GetGroupAsync(id, cancellationToken);
                    Groupname = data.Groupname;
                    Roles = data.Roles ?? Array.Empty<RolePriority>();
                    Clients = data.Clients ?? Array.Empty<ClientReference>();
                    await selectListItems;
                    return Page();
                }
                var group = new Group
                {
                    Groupname = id,
                    Name = Name ?? String.Empty,
                    Description = Description ?? String.Empty,
                };
                await _dynsec.ModifyGroupAsync(group, cancellationToken);
                return RedirectToPage("Index");
            }
            catch (DynsecProtocolException ex) when (ex.Error == "Group not found")
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id, CancellationToken cancellationToken)
        {
            await _dynsec.DeleteGroupAsync(id, cancellationToken);
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostAddClientAsync(string id, string name, int priority, CancellationToken cancellationToken)
        {
            if (!String.IsNullOrEmpty(name))
            {
                await _dynsec.AddGroupClientAsync(id, name, priority, cancellationToken);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteClientAsync(string id, string name, CancellationToken cancellationToken)
        {
            await _dynsec.RemoveGroupClientAsync(id, name, cancellationToken);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddRoleAsync(string id, string name, int priority, CancellationToken cancellationToken)
        {
            if (!String.IsNullOrEmpty(name))
            {
                await _dynsec.AddGroupRoleAsync(id, name, priority, cancellationToken);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteRoleAsync(string id, string name, CancellationToken cancellationToken)
        {
            await _dynsec.RemoveGroupRoleAsync(id, name, cancellationToken);
            return RedirectToPage();
        }

        private async Task LoadSelectlistItemsAsync(CancellationToken cancellationToken)
        {
            var rolesTask = _dynsec.ListRolesAsync(cancellationToken);
            _availableClients = await _dynsec.ListClientsAsync(cancellationToken);
            _availableRoles = await rolesTask;
        }
    }
}
