using Dynsec;
using Dynsec.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DynsecAdmin.Pages.Clients
{
    public class EditModel : PageModel
    {
        private readonly DynsecClient _dynsec;

        private string[] _availableGroups { get; set; }

        public string[] _availableRoles { get; set; }

        public bool Self => User.IsSelf(Username);

        public string Username { get; set; }

        public bool Disabled { get; set; }

        [BindProperty]
        [Display(Name = "Client ID")]
        public string? ClientId { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        [BindProperty]
        [Display(Name = "Text name")]
        public string? Name { get; set; }

        [BindProperty]
        [Display(Name = "Text description")]
        public string? Description { get; set; }

        public RolePriority[] Roles { get; set; }

        public GroupPriority[] Groups { get; set; }

        public IEnumerable<SelectListItem> AvailableGroups => _availableGroups
            .Where(x => Groups.All(g => g.Groupname != x))
            .OrderBy(x => x)
            .Select(x => new SelectListItem(x, x));

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
                var user = await _dynsec.GetClientAsync(id, cancellationToken);
                Username = user.Username;
                ClientId = user.ClientId;
                Disabled = user.Disabled ?? false;
                Name = user.Name;
                Description = user.Description;
                Roles = user.Roles ?? Array.Empty<RolePriority>();
                Groups = user.Groups ?? Array.Empty<GroupPriority>();
                await selectListItems;
                return Page();
            }
            catch (DynsecProtocolException ex) when (ex.Error == "Client not found")
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
                    var user = await _dynsec.GetClientAsync(id, cancellationToken);
                    Username = user.Username;
                    Roles = user.Roles ?? Array.Empty<RolePriority>();
                    Groups = user.Groups ?? Array.Empty<GroupPriority>();
                    await selectListItems;
                    return Page();
                }
                var client = new Client
                {
                    Username = id,
                    Password = Password,
                    ClientId = ClientId ?? String.Empty,
                    Name = Name ?? String.Empty,
                    Description = Description ?? String.Empty,
                };
                await _dynsec.ModifyClientAsync(client, cancellationToken);
                return RedirectToPage("Index");
            }
            catch (DynsecProtocolException ex) when (ex.Error == "Client not found")
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostEnableAsync(string id, CancellationToken cancellationToken)
        {
            await _dynsec.EnableClientAsync(id, cancellationToken);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDisableAsync(string id, CancellationToken cancellationToken)
        {
            await _dynsec.DisableClientAsync(id, cancellationToken);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id, CancellationToken cancellationToken)
        {
            await _dynsec.DeleteClientAsync(id, cancellationToken);
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostAddGroupAsync(string id, string name, int priority, CancellationToken cancellationToken)
        {
            await _dynsec.AddGroupClientAsync(name, id, priority, cancellationToken);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddRoleAsync(string id, string name, int priority, CancellationToken cancellationToken)
        {
            await _dynsec.AddClientRoleAsync(id, name, priority, cancellationToken);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteGroupAsync(string id, string name, CancellationToken cancellationToken)
        {
            await _dynsec.RemoveGroupClientAsync(name, id, cancellationToken);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteRoleAsync(string id, string name, CancellationToken cancellationToken)
        {
            await _dynsec.RemoveClientRoleAsync(id, name, cancellationToken);
            return RedirectToPage();
        }

        private async Task LoadSelectlistItemsAsync(CancellationToken cancellationToken)
        {
            var rolesTask = _dynsec.ListRolesAsync(cancellationToken);
            _availableGroups = await _dynsec.ListGroupsAsync(cancellationToken);
            _availableRoles = await rolesTask;
        }
    }
}
