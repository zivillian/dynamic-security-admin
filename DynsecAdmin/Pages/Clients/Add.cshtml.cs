using System.ComponentModel.DataAnnotations;
using Dynsec;
using Dynsec.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DynsecAdmin.Pages.Clients
{
    public class AddModel : PageModel
    {
        [Required]
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        [Display(Name = "Client ID")]
        public string? ClientId { get; set; }

        [Required]
        [BindProperty]
        public string Password { get; set; }

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
            var user = new Client
            {
                Username = Username,
                Password = Password,
                ClientId = ClientId,
                Name = Name,
                Description = Description,
            };
            await client.CreateClientAsync(user, cancellationToken);
            return RedirectToPage("Edit", new { id = Username });
        }
    }
}
