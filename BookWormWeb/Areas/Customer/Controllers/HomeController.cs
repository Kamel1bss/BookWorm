using System.Diagnostics;
using System.Security.Claims;
using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using BookWorm.Utility.Constants;
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

        public IActionResult Index(string searchTerm, string filterBy = "all", int? categoryId = null, int page = 1)
        {
            int pageSize = 8;

            var query = _context._productRepo.GetAll(includeProperties: "Category");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();

                query = filterBy switch
                {
                    "title" => query.Where(p => p.Title.ToLower().Contains(searchTerm)),
                    "author" => query.Where(p => p.Author.ToLower().Contains(searchTerm)),
                    _ => query.Where(p => p.Title.ToLower().Contains(searchTerm) ||
                                          p.Author.ToLower().Contains(searchTerm))
                };
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            int totalItems = query.Count();

            var products = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.FilterBy = filterBy;
            ViewBag.CategoryId = categoryId;

            ViewBag.Categories = _context._categoryRepo.GetAll().ToList();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductList", products);
            }

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

            if (shoppingCartDB != null)
            {
                shoppingCartDB.Count += cart.Count;
                _context._shoppingCartRepo.Update(shoppingCartDB);
            }
            else
            {
                _context._shoppingCartRepo.Add(cart);
            }

            _context.Save();

            HttpContext.Session.SetInt32(
                SD.SessionCart,
                _context._shoppingCartRepo
                    .GetAll(u => u.ApplicationUserId == userId)
                    .Count()
            );

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
