using System.Diagnostics;
using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
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
            var product = _context._productRepo.Get(p => p.Id == productId, includeProperties:"Category");
            return View(product);
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
