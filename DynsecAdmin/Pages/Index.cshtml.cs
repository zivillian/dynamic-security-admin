using System.ComponentModel.DataAnnotations;
using Dynsec;
using Dynsec.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DynsecAdmin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DynsecClient _dynsec;

        private string[] _availableGroups { get; set; }

        [BindProperty]
        [Display(Name = "Anonymous group")]
        public string AnonymousGroup { get; set; }

        [BindProperty]
        [Display(Name = "publishClientSend")]
        public bool PublishClientSend { get; set; }

        [BindProperty]
        [Display(Name = "publishClientReceive")]
        public bool PublishClientReceive { get; set; }

        [BindProperty]
        [Display(Name = "subscribe")]
        public bool Subscribe { get; set; }

        [BindProperty]
        [Display(Name = "unsubscribe")]
        public bool Unsubscribe { get; set; }

        public IEnumerable<SelectListItem> AvailableGroups => _availableGroups
            .OrderBy(x => x)
            .Select(x => new SelectListItem(x, x))
            .Prepend(new SelectListItem("", "", String.IsNullOrEmpty(AnonymousGroup), true));

        public IndexModel(DynsecClient dynsec)
        {
            _dynsec = dynsec;
        }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            var anonGroup = _dynsec.GetAnonymousGroupAsync(cancellationToken);
            var defaultAclTask = _dynsec.GetDefaultAclAccessAsync(cancellationToken);
            _availableGroups = await _dynsec.ListGroupsAsync(cancellationToken);
            AnonymousGroup = await anonGroup;
            var defaultAcl = await defaultAclTask;
            PublishClientSend = defaultAcl.Where(x => x.Type == AclType.PublishClientSend).Select(x => x.Allow).FirstOrDefault();
            PublishClientReceive = defaultAcl.Where(x => x.Type == AclType.PublishClientReceive).Select(x => x.Allow).FirstOrDefault();
            Subscribe = defaultAcl.Where(x => x.Type == AclType.Subscribe).Select(x => x.Allow).FirstOrDefault();
            Unsubscribe = defaultAcl.Where(x => x.Type == AclType.Unsubscribe).Select(x => x.Allow).FirstOrDefault();
            return Page();
        }

        public IActionResult OnPost()
        {
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSetAnonGroupAsync(CancellationToken cancellationToken)
        {
            if (!String.IsNullOrEmpty(AnonymousGroup))
            {
                await _dynsec.SetAnonymousGroup(AnonymousGroup, cancellationToken);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSetDefaultAclAsync(CancellationToken cancellationToken)
        {
            var acls = new[]
            {
                new Acl { Type = AclType.PublishClientSend, Allow = PublishClientSend },
                new Acl { Type = AclType.PublishClientReceive, Allow = PublishClientReceive },
                new Acl { Type = AclType.Subscribe, Allow = Subscribe },
                new Acl { Type = AclType.Unsubscribe, Allow = Unsubscribe },
            };
            await _dynsec.SetDefaultAclAccess(acls, cancellationToken);
            return RedirectToPage();
        }
    }
}
