using BookWorm.DataAccess.Data;
using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWorm.DataAccess.Repository
{
    public class ProductRepository(ApplicationDbContext db) : Repository<Product>(db), IProductRepository
    {
        private readonly ApplicationDbContext _db = db;
        public void Update(Product entity)
        {
            db.Products.Update(entity);
        }
    }
}
