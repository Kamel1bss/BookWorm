using BookWormRazor.Data;
using BookWormRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookWormRazor.Pages.Categories
{
    [BindProperties]
    public class DeleteModel(ApplicationDbContext context) : PageModel
    {
        private readonly ApplicationDbContext _context = context;
        public Category Category { get; set; }
        public void OnGet(int id)
        {
            Category = _context.Categories.Find(id);
        }

        public IActionResult OnPost()
        {

            if (Category is not null)
            {
                _context.Categories.Remove(Category);
                _context.SaveChanges();
                TempData["Deleted"] = "Successfully deleted the category";
                return RedirectToPage("Index");
            }

            return Page();
        }
    }
}
