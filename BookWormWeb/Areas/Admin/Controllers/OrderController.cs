using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using BookWorm.Models.ViewModels;
using BookWorm.Utility.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookWormWeb.Areas.Admin.Controllers;

[Authorize]
[Area("Admin")]
public class OrderController(IUnitOfWork context) : Controller
{
    private readonly IUnitOfWork _context = context;
    [BindProperty]
    public OrderVM OrderVM { get; set; }
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Details(int orderId)
    {
        OrderVM = new()
        {
            OrderHeader = _context._orderHeaderRepo.Get(u => u.Id == orderId, includeProperties:"ApplicationUser"),
            OrderDetails = _context._orderDetailRepo.GetAll(o => o.OrderHeaderId == orderId, includeProperties: "Product")
        };

        return View(OrderVM);
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult UpdateOrderDetail()
    {
        var orderFromDb = _context._orderHeaderRepo.Get(u => u.Id == OrderVM.OrderHeader.Id);
        orderFromDb.Name = OrderVM.OrderHeader.Name;
        orderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
        orderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
        orderFromDb.City = OrderVM.OrderHeader.City;
        orderFromDb.State = OrderVM.OrderHeader.State;
        orderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

        if(!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
        {
            orderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
        }
        if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
        {
            orderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
        }

        _context._orderHeaderRepo.Update(orderFromDb);
        _context.Save();
        TempData["Success"] = "Order Details Updated Successfully";
        return RedirectToAction(nameof(Details), new { orderId = orderFromDb.Id });
    }

    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    [HttpPost]
    public IActionResult startProcessing()
    {
        _context._orderHeaderRepo.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
        _context.Save();
        TempData["Success"] = "Order Details Updated Successfully";
        return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
    }

    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    [HttpPost]
    public IActionResult shipOrder()
    {
        var orderFromDb = _context._orderHeaderRepo.Get(u => u.Id == OrderVM.OrderHeader.Id);
        orderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
        orderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
        orderFromDb.OrderStatus = SD.StatusShipped;
        orderFromDb.ShippingDate = DateTime.Now;

        if(orderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
        {
            orderFromDb.PaymentDueDate = DateOnly.FromDateTime (DateTime.Now.AddDays(30));
        }
        _context._orderHeaderRepo.Update(orderFromDb);
        _context.Save();
        TempData["Success"] = "Order Shipped Successfully";
        return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
    }

    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    [HttpPost]
    public IActionResult cancelOrder()
    {
        var orderFromDb = _context._orderHeaderRepo.Get(u => u.Id == OrderVM.OrderHeader.Id);
        if(orderFromDb.PaymentStatus == SD.PaymentStatusApproved)
        {
            var options = new RefundCreateOptions
            {
                Reason = RefundReasons.RequestedByCustomer,
                PaymentIntent = orderFromDb.PaymentIntentId
            };
            var service = new RefundService();
            Refund refund = service.Create(options);
            _context._orderHeaderRepo.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusCancelled ,SD.StatusRefunded);
        }
        else
        {
            _context._orderHeaderRepo.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);

        }

        _context.Save();
        TempData["Success"] = "Order Canceled Successfully";
        return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
    }


    [ActionName("Details")]
    [HttpPost]
    public IActionResult DetailsPayNow()
    {
        OrderVM.OrderHeader = _context.
            _orderHeaderRepo.Get(u => u.Id == OrderVM.OrderHeader.Id, includeProperties:"ApplicationUser");
        OrderVM.OrderDetails = _context.
            _orderDetailRepo.GetAll(o => o.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: "Product");
        var domain = "https://localhost:7043/";
        var options = new SessionCreateOptions
        {
            SuccessUrl = domain + "Admin/Order/PaymentConfirmation?orderHeaderId=" + OrderVM.OrderHeader.Id,
            CancelUrl = domain + "Admin/Order/Details?orderId=" + OrderVM.OrderHeader.Id,
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment"

        };

        foreach (var item in OrderVM.OrderDetails)
        {
            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)item.Price * 100,
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product.Title,
                    }
                },
                Quantity = item.Count
            };

            options.LineItems.Add(sessionLineItem);
        }

        var service = new SessionService();
        Session session = service.Create(options);
        _context._orderHeaderRepo.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
        _context.Save();

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }

    public IActionResult PaymentConfirmation(int orderHeaderId)
    {
        OrderHeader orderHeader = _context._orderHeaderRepo.Get(u => u.Id == orderHeaderId);
        if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
        {
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _context._orderHeaderRepo.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                _context._orderHeaderRepo.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                _context.Save();
            }
        }

        List<ShoppingCart> shoppingCarts = _context._shoppingCartRepo
            .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
        _context._shoppingCartRepo.RemoveRange(shoppingCarts);
        _context.Save();

        return View(orderHeaderId);
    }
    #region API CALLS
    [HttpGet]
    public IActionResult GetAll(string status)
    {
        IEnumerable<OrderHeader> orderHeaders;

        if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
        {
            orderHeaders = _context._orderHeaderRepo.GetAll(includeProperties: "ApplicationUser");
        }
        else
        {
            var identity = (ClaimsIdentity)User.Identity;
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier).Value;
            orderHeaders = _context._orderHeaderRepo.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
        }
        
        switch (status)
        {
            case "pending":
                orderHeaders = orderHeaders.Where(o => o.PaymentStatus == BookWorm.Utility.Constants.SD.PaymentStatusDelayedPayment);
                break;
            case "inprocess":
                orderHeaders = orderHeaders.Where(o => o.OrderStatus == BookWorm.Utility.Constants.SD.StatusInProcess);                break;
            case "completed":
                orderHeaders = orderHeaders.Where(o => o.OrderStatus == BookWorm.Utility.Constants.SD.StatusShipped);
                break;
            case "approved":
                orderHeaders = orderHeaders.Where(o => o.OrderStatus == BookWorm.Utility.Constants.SD.StatusApproved);
                break;
            default:
                break;
        }


        return Json(new { data = orderHeaders });
    }

    #endregion

}