using Dynsec;
using Dynsec.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DynsecAdmin.Pages
{
    public class AccessModel : PageModel
    {
        private readonly DynsecClient _dynsec;

        [BindProperty(SupportsGet = true)]
        public string Topic { get; set; }

        public IEnumerable<RoleAccess> Roles { get; set; }

        public AccessModel(DynsecClient dynsec)
        {
            _dynsec = dynsec;
            Roles = Array.Empty<RoleAccess>();
        }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(Topic)) return Page();

            var roles = await _dynsec.ListRolesVerboseAsync(cancellationToken);
            var result = new List<RoleAccess>();
            foreach (var role in roles)
            {
                var access = new RoleAccess
                {
                    Rolename = role.Rolename,
                    Name = role.Name,
                    Description = role.Description
                };
                //TODO also check deny in correct priority order
                foreach (var acl in role.Acls.Where(x => x.Allow).OrderBy(x => x.Priority))
                {
                    if (TopicMatches(Topic, acl.Topic))
                    {
                        access.Access.Add(acl.Type);
                    }
                }

                if (access.Access.Count > 0)
                {
                    result.Add(access);
                }
            }

            Roles = result;
            return Page();
        }

        private bool TopicMatches(string topic, string wildcard)
        {
            var topicParts = topic.Split('/');
            var wildcardParts = wildcard.Split('/');
            for (int i = 0; i < topicParts.Length; i++)
            {
                if (wildcardParts.Length <= i) return false;
                var topicSegment = topicParts[i];
                var wildcardSegment = wildcardParts[i];
                if (wildcardSegment == "#") return true;
                if (wildcardSegment == "+") continue;
                if (topicSegment != wildcardSegment) return false;
            }

            if (wildcardParts.Length > topicParts.Length) return false;
            return true;
        }
    }

    public class RoleAccess
    {
        public string Rolename { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<AclType> Access { get; set; } = new();
    }
}
