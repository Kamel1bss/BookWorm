using BookWorm.DataAccess.Data;
using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWorm.DataAccess.Repository;

public class OrderDetailRepository(ApplicationDbContext db) : Repository<OrderDetail>(db), IOrderDetailRepository
{
    private readonly ApplicationDbContext _db = db;
    public void Update(OrderDetail detail)
    {
        _db.OrderDetails.Update(detail);
    }
}
