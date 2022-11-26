using Dynsec;
using Dynsec.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace DynsecAdmin.Pages.Groups
{
    public class AddModel : PageModel
    {
        [Required]
        [BindProperty]
        public string Groupname { get; set; }

        [BindProperty]
        [Display(Name = "Text name")]
        public string? Name { get; set; }

        [BindProperty]
        [Display(Name = "Text description")]
        public string? Description { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync([FromServices]DynsecClient client, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var group = new Group
            {
                Groupname = Groupname,
                Name = Name,
                Description = Description,
            };
            await client.CreateGroupAsync(group, cancellationToken);
            return RedirectToPage("Edit", new { id = Groupname });
        }
    }
}
