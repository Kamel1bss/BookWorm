using System.Diagnostics;
using System.Security.Claims;
using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookWormWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _context;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context._productRepo.GetAll();
            return View(products);
        }
        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = _context._productRepo.Get(p => p.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            cart.ApplicationUserId = userId;
            var shoppingCartDB = _context._shoppingCartRepo.Get(
                u => u.ApplicationUserId == userId && u.ProductId == cart.ProductId);
            if(shoppingCartDB != null)
            {
                shoppingCartDB.Count += cart.Count;
                _context._shoppingCartRepo.Update(shoppingCartDB);
            }
            else
            {
                _context._shoppingCartRepo.Add(cart);
            }
            _context.Save();
            TempData["success"] = "Cart updated successfully";
            return RedirectToAction(nameof(Index)); 
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
