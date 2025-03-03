using BookWorm.DataAccess.Data;
using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWorm.DataAccess.Repository;

public class OrderHeaderRepository(ApplicationDbContext db) : Repository<OrderHeader>(db), IOrderHeaderRepository
{
    private readonly ApplicationDbContext _db = db;
    public void Update(OrderHeader header)
    {
        _db.OrderHeaders.Update(header);
    }
}
