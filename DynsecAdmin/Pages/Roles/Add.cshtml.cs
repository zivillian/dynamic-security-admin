using Dynsec;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Dynsec.DTO;

namespace DynsecAdmin.Pages.Roles
{
    public class AddModel : PageModel
    {
        [Required]
        [BindProperty]
        public string Rolename { get; set; }

        [BindProperty]
        [Display(Name = "Text name")]
        public string? Name { get; set; }

        [BindProperty]
        [Display(Name = "Text description")]
        public string? Description { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync([FromServices] DynsecClient client, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var role = new Role
            {
                Rolename = Rolename,
                Name = Name,
                Description = Description
            };
            await client.CreateRoleAsync(role, cancellationToken);
            return RedirectToPage("Edit", new { id = Rolename });
        }
    }
}
