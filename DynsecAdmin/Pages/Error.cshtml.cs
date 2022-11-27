using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dynsec;
using Microsoft.AspNetCore.Diagnostics;

namespace DynsecAdmin.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string Error { get; set; }

        public string Message { get; set; }

        public string Hint { get; set; }

        public void OnGet()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature?.Error is DynsecProtocolException ex)
            {
                Error = ex.Error;
                Message = ex.Message;
            }
            else if (exceptionFeature?.Error is DynsecTimeoutException timeout)
            {
                Error = "Timeout";
                Message = timeout.Message;
                Hint = "This is probably caused by a modification which affected your current user. In this case, the client is disconnected from the mqtt server to enforce the modified permissions.";
            }
        }

        public void OnPost()
        {
            OnGet();
        }
    }
}