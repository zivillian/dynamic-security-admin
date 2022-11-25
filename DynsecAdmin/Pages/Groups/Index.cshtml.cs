using Dynsec;
using Dynsec.DTO;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DynsecAdmin.Pages.Groups
{
    public class IndexModel : PageModel
    {
        private readonly DynsecClient _dynsec;

        public Group[] Groups { get; set; }

        public IndexModel(DynsecClient dynsec)
        {
            _dynsec = dynsec;
        }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            Groups = await _dynsec.ListGroupsVerboseAsync(cancellationToken);
        }
    }
}
