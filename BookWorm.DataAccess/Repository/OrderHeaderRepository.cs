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

    public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
    {
        var orderFromDB = _db.OrderHeaders.FirstOrDefault(o => o.Id == id);
        if(orderFromDB != null)
        {
            orderFromDB.OrderStatus = orderStatus;
            if (!string.IsNullOrEmpty(orderFromDB.PaymentStatus))
            {
                orderFromDB.PaymentStatus = paymentStatus;
            }
        }
    }

    public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
    {
        var orderFormDB = _db.OrderHeaders.FirstOrDefault(o => o.Id == id);
        if(!string.IsNullOrEmpty(sessionId))
        {
            orderFormDB.SessionId = sessionId;
        }

        if (!string.IsNullOrEmpty(paymentIntentId))
        {
            orderFormDB.PaymentIntentId = paymentIntentId;
            orderFormDB.PaymentDate = DateTime.Now;
        }
    }
}
