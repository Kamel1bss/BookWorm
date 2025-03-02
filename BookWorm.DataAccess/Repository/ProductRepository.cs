using BookWorm.DataAccess.Data;
using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWorm.DataAccess.Repository;

public class ProductRepository(ApplicationDbContext db) : Repository<Product>(db), IProductRepository
{
    private readonly ApplicationDbContext _db = db;
    public void Update(Product entity)
    {
        var productFromDb = _db.Products.FirstOrDefault(p => p.Id == entity.Id);
        if(productFromDb is not null)
        {
            productFromDb.Title = entity.Title;
            productFromDb.Description = entity.Description;
            productFromDb.CategoryId = entity.CategoryId;
            productFromDb.ISBN = entity.ISBN;
            productFromDb.ListPrice = entity.ListPrice;
            productFromDb.Price = entity.Price;
            productFromDb.Price100 = entity.Price100;
            productFromDb.Price50 = entity.Price50;
            productFromDb.Author = entity.Author;

            if(entity.ImageUrl is not null)
            {
                productFromDb.ImageUrl = entity.ImageUrl;
            }

        }
    }
}
