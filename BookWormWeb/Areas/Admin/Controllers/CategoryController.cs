using BookWorm.DataAccess.Data;
using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using BookWorm.Utility.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookWormWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class CategoryController(IUnitOfWork context) : Controller
{
    private readonly IUnitOfWork _context = context;
    public IActionResult Index()
    {
        var categories = _context._categoryRepo.GetAll();
        return View(categories);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context._categoryRepo.Add(category);
            _context.Save();
            TempData["success"] = "Category created successfully";
            return RedirectToAction("Index");
        }

        return View();
    }

    public IActionResult Edit(int? id)
    {
        var category = _context._categoryRepo.Get(c => c.CategoryId == id);
        return View(category);
    }

    [HttpPost]
    public IActionResult Edit(Category category)
    {

        if (category is not null && ModelState.IsValid)
        {
            _context._categoryRepo.Update(category);
            _context.Save();
            TempData["update"] = "Category updated successfully";
            return RedirectToAction("Index");
        }

        return View();
    }

    public IActionResult Delete(int? id)
    {
        var category = _context._categoryRepo.Get(c => c.CategoryId == id);
        return View(category);
    }

    [HttpPost]
    public IActionResult Delete(Category category)
    {
        if (category is not null)
        {
            _context._categoryRepo.Remove(category);
            _context.Save();
            TempData["delete"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }

        return View();
    }

}
