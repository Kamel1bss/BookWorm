using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using BookWorm.Models.ViewModels;
using BookWorm.Utility.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookWormWeb.Areas.Admin.Controllers;

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