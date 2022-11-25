using Dynsec;
using Dynsec.DTO;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DynsecAdmin.Pages.Roles
{
    public class IndexModel : PageModel
    {
        private readonly DynsecClient _dynsec;

        public Role[] Roles { get; set; }

        public IndexModel(DynsecClient dynsec)
        {
            _dynsec = dynsec;
        }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            Roles = await _dynsec.ListRolesVerboseAsync(cancellationToken);
        }
    }
}
