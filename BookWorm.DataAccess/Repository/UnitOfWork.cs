using BookWorm.DataAccess.Data;
using BookWorm.DataAccess.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWorm.DataAccess.Repository;

public class UnitOfWork : IUnitOfWork
{
    public ICategoryRepository _categoryRepo { get; private set; }
    public IProductRepository _productRepo { get; private set; }
    private readonly ApplicationDbContext _db;
    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        _categoryRepo = new CategoryRepository(_db);
        _productRepo = new ProductRepository(_db);
    }
    public void Save()
    {
        _db.SaveChanges();
    }
}
