using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace BookWormWeb.Controllers
{
    public class LanguageController : Controller
    {
        public IActionResult Set(string culture, string returnUrl)
        {
            if (string.IsNullOrEmpty(culture))
                culture = "en";

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl ?? "/");
        }
    }
}