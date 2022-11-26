using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Dynsec;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Client;

namespace DynsecAdmin.Pages
{
    public class LoginModel : PageModel
    {
        [Required]
        [BindProperty]
        public string Username { get; set; }

        [Required]
        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public bool Kmsi { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync([FromServices]MqttFactory factory, [FromServices]MqttClientOptions options, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return Page();
            var previous = HttpContext.User;

            try
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Upn, Username),
                    new(ClaimTypes.Hash, Password),
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                //fake logged in user for DynsecClient
                HttpContext.User = new ClaimsPrincipal(identity);

                var client = factory.CreateMqttClient();

                await client.ConnectAsync(options, cancellationToken);
                HttpContext.User = previous;
                var dynsec = new DynsecClient(client);
                try
                {
                    await dynsec.ListClientsAsync(0, 0, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    //no response - assume invalid credentials
                    ModelState.AddModelError(nameof(Username), "invalid username, password or no access to $CONTROL");
                    return Page();
                }

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = Kmsi,
                    AllowRefresh = true
                };
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    authProperties);
            }
            catch (DynsecProtocolException)
            {
                ModelState.AddModelError(nameof(Username), "invalid username, password or no access to $CONTROL");
                return Page();
            }
            catch (MqttConnectingFailedException)
            {
                HttpContext.User = previous;
                ModelState.AddModelError(nameof(Username), "invalid username, password or no access to $CONTROL");
                return Page();
            }

            if (Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);
            return RedirectToPage("/Clients/Index");
        }
    }
}
