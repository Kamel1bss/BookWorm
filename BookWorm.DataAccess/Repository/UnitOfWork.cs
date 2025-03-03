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
    public ICompanyRepository _companyRepo { get; private set; }
    public IShoppingCartRepository _shoppingCartRepo { get; private set; }
    public IApplicationUserRepository _applicationUserRepo { get; private set; }
    public IOrderHeaderRepository _orderHeaderRepo { get; private set; }
    public IOrderDetailRepository _orderDetailRepo { get; private set; }

    private readonly ApplicationDbContext _db;
    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        _categoryRepo = new CategoryRepository(_db);
        _productRepo = new ProductRepository(_db);
        _companyRepo = new CompanyRepository(_db);
        _shoppingCartRepo = new ShoppingCartRepository(_db);
        _applicationUserRepo = new ApplicationUserRepository(_db);
        _orderHeaderRepo = new OrderHeaderRepository(_db);
        _orderDetailRepo = new OrderDetailRepository(_db);

    }
    public void Save()
    {
        _db.SaveChanges();
    }
}
