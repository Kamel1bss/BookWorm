using BookWormRazor.Data;
using BookWormRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookWormRazor.Pages.Categories
{
    [BindProperties]
    public class EditModel(ApplicationDbContext context) : PageModel
    {
        private readonly ApplicationDbContext _context = context;
        public Category Category { get; set; }
        public void OnGet(int id)
        {
            Category = _context.Categories.Find(id);
        }

        public IActionResult OnPost() 
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(Category);
                _context.SaveChanges();
                TempData["Updated"] = "Successfully updated the category";
                return RedirectToPage("Index");
            }

            return Page();
        }
    }
}
