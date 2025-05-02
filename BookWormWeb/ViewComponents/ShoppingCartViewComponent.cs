using BookWorm.DataAccess.IRepository;
using BookWorm.Utility.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookWormWeb.ViewComponents;

public class ShoppingCartViewComponent(IUnitOfWork context) : ViewComponent
{
    IUnitOfWork _context = context;
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var claims = (ClaimsIdentity)User.Identity;
        var claim = claims.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null)
        {
            if (HttpContext.Session.GetInt32(SD.SessionCart) == null)
            {
                HttpContext.Session.SetInt32(SD.SessionCart,
                    _context._shoppingCartRepo.GetAll(u => u.ApplicationUserId == claim.Value).Count());
            }

            return View(HttpContext.Session.GetInt32(SD.SessionCart));
        }
        else
        {
            HttpContext.Session.Clear();
            return View(0);
        }
    }
}
