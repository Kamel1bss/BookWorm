using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookWormWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController(IUnitOfWork context) : Controller
{
    private readonly IUnitOfWork _context = context;
    public IActionResult Index()
    {
        var entities = _context._productRepo.GetAll();
        return View(entities);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Product product)
    {
        if (ModelState.IsValid)
        {
            _context._productRepo.Add(product);
            _context.Save();
            TempData["success"] = "Product created successfully";
            return RedirectToAction("Index");
        }

        return View();
    }

    public IActionResult Edit(int? id)
    {
        if (id == 0 || id is null)
            return NotFound();

        var product = _context._productRepo.Get(p => p.Id == id);

        if (product is null)
            return NotFound();

        return View(product);
    }

    [HttpPost]
    public IActionResult Edit(Product product)
    {
        if (product is not null && ModelState.IsValid)
        {
            _context._productRepo.Update(product);
            _context.Save();
            TempData["update"] = "Product updated successfully";
            return RedirectToAction("Index");
        }

        return View(product);
    }

    public IActionResult Delete(int? id)
    {
        if (id == 0 || id is null)
            return NotFound();

        var product = _context._productRepo.Get(p => p.Id == id);

        if (product is null)
            return NotFound();

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
