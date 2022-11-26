using Dynsec;
using Dynsec.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DynsecAdmin.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly DynsecClient _dynsec;

        public Client[] Clients { get; set; }

        public IndexModel(DynsecClient dynsec)
        {
            _dynsec = dynsec;
        }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            Clients = await _dynsec.ListClientsVerboseAsync(cancellationToken);
        }
    }
}
