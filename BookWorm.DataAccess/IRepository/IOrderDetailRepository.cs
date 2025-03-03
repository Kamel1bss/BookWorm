using BookWorm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWorm.DataAccess.IRepository;

public interface IOrderDetailRepository : IRepository<OrderDetail>
{
    void Update (OrderDetail detail);
}
