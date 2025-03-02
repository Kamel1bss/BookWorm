using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using BookWorm.Models.ViewModels;
using BookWorm.Utility.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWormWeb.Areas.Admin.Controllers;

[Area("Admin")]
//[Authorize(Roles = SD.Role_Admin)]
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

    #region API CALLS
    [HttpGet]
    public IActionResult GetAll()
    {
        var products = _context._productRepo.GetAll(includeProperties: "Category");
        return Json(new { data = products });
    }

    [HttpDelete]
    public IActionResult Delete(int id)
    {
        var product = _context._productRepo.Get(p => p.Id == id);
        if(product is null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }

        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));
        if(System.IO.File.Exists(oldImagePath))
        {
            System.IO.File.Delete(oldImagePath);
        }

        _context._productRepo.Remove(product);
        _context.Save();
        return Json(new { success = true, message = "Delete successful" });
    }
    #endregion
}
