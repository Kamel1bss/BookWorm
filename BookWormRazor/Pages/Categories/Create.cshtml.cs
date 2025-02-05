using BookWormRazor.Data;
using BookWormRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookWormRazor.Pages.Categories
{
    [BindProperties]
    public class CreateModel(ApplicationDbContext context) : PageModel
    {
        private readonly ApplicationDbContext _context = context;
        public Category Category {  get; set; }
        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(Category);
                _context.SaveChanges();
                TempData["Created"] = "Successfully created a new category";
                return RedirectToPage("Index");
            }

            return Page();
           
        }
    }
}
