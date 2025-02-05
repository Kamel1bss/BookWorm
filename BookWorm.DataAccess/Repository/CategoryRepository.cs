using BookWorm.DataAccess.Data;
using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWorm.DataAccess.Repository
{
    public class CategoryRepository(ApplicationDbContext db) : Repository<Category>(db), ICategoryRepository
    {
        private readonly ApplicationDbContext _db = db;
        public void Update(Category category)
        {
            _db.Update(category);
        }
    }
}
