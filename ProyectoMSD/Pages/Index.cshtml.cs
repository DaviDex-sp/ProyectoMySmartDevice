using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProyectoMSD.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Usuarios/Index");
            }
            return Page();
        }
    }
}
