using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using BookWorm.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWormWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController(IUnitOfWork context, IWebHostEnvironment webHostEnvironment) : Controller
{
    private readonly IUnitOfWork _context = context;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
    public IActionResult Index()
    {
        var products = _context._productRepo.GetAll(includeProperties:"Category");
   
        return View(products);
    }

    public IActionResult Upsert(int? id)
    {
        ProductVM product_categories = new ProductVM
        {
            Product = new Product(),
            CategoryList = _context._categoryRepo.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.CategoryId.ToString()
            })
        };

        if (id is not null && id != 0)
            product_categories.Product = _context._productRepo.Get(p => p.Id == id);

        return View(product_categories);
    }

    [HttpPost]
    public IActionResult Upsert(ProductVM productVM, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            string wwwrootPath = _webHostEnvironment.WebRootPath;
            if(file is not null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwrootPath, @"images\products");

                if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                {
                    string oldPath = Path.Combine(wwwrootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                productVM.Product.ImageUrl = @"\images\products\" + fileName;
            }

            if (productVM.Product.Id == 0)
            {
                _context._productRepo.Add(productVM.Product);
            }
            else
            {
                _context._productRepo.Update(productVM.Product);
            }
            _context.Save();
            TempData["success"] = "Product created successfully";
            return RedirectToAction("Index");
        }

        productVM.CategoryList = _context._categoryRepo.GetAll().Select(c => new SelectListItem
        {
            Text = c.Name,
            Value = c.CategoryId.ToString()
        });

        return View(productVM);
    }

    public IActionResult Delete(int? id)
    {
        if (id == 0 || id is null)
            return NotFound();

        var product = _context._productRepo.Get(p => p.Id == id);

        if (product is null)
            return NotFound();

        IEnumerable<SelectListItem> categories = _context._categoryRepo.GetAll().Select(c => new SelectListItem
        {
            Text = c.Name,
            Value = c.CategoryId.ToString()
        });

        ViewBag.Categories = categories;
        return View(product);
    }

    [HttpPost]
    public IActionResult Delete(Product product)
    {
        if(product is not null)
        {
            _context._productRepo.Remove(product);
            _context.Save();
            TempData["delete"] = "Product deleted successfully";
            return RedirectToAction("Index");
        }

        return View();
    }
}
