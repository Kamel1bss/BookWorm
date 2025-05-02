using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using BookWorm.Models.ViewModels;
using BookWorm.Utility.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWormWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class CompanyController(IUnitOfWork context) : Controller
{
    private readonly IUnitOfWork _context = context;
    public IActionResult Index()
    {
        var Companys = _context._companyRepo.GetAll();
   
        return View(Companys);
    }

    public IActionResult Upsert(int? id)
    {
        Company company;
        if (id is not null && id != 0)
             company = _context._companyRepo.Get(c => c.Id == id);
        else
             company = new Company();

        return View(company);
    }

    [HttpPost]
    public IActionResult Upsert(Company company)
    {
        if (ModelState.IsValid)
        {
            if (company.Id == 0)
            {
                _context._companyRepo.Add(company);
            }
            else
            {
                _context._companyRepo.Update(company);
            }
            _context.Save();
            TempData["success"] = "Company created successfully";
            return RedirectToAction("Index");
        }

        return View(company);
    }

    #region API CALLS
    [HttpGet]
    public IActionResult GetAll()
    {
        var Companys = _context._companyRepo.GetAll();
        return Json(new { data = Companys });
    }

    [HttpDelete]
    public IActionResult Delete(int id)
    {
        var Company = _context._companyRepo.Get(p => p.Id == id);
        if(Company is null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }

        _context._companyRepo.Remove(Company);
        _context.Save();
        return Json(new { success = true, message = "Delete successful" });
    }
    #endregion
}
