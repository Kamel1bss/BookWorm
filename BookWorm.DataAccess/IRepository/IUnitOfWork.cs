using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWorm.DataAccess.IRepository;

public interface IUnitOfWork
{
    ICategoryRepository _categoryRepo { get; }
    IProductRepository _productRepo { get; }
    ICompanyRepository _companyRepo { get; }
    IShoppingCartRepository _shoppingCartRepo { get; }
    IApplicationUserRepository _applicationUserRepo { get; }
    IOrderHeaderRepository _orderHeaderRepo { get; }
    IOrderDetailRepository _orderDetailRepo { get; }

    void Save();
}
