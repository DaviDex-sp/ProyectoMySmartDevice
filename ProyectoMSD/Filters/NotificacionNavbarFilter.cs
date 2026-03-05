using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;

namespace ProyectoMSD.Filters
{
    public class NotificacionNavbarFilter : IAsyncPageFilter
    {
        private readonly AppDbContext _context;

        public NotificacionNavbarFilter(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (context.HandlerInstance is PageModel pageModel)
            {
                var userIdClaim = pageModel.User.FindFirst("UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var count = await _context.Notificaciones
                        .CountAsync(n => n.IdUsuarios == userId && !n.Leida);
                    pageModel.ViewData["NotifCount"] = count;
                }
            }
            await next();
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }
    }
}
