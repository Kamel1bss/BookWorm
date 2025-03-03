using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using BookWorm.Models.ViewModels;
using BookWorm.Utility.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookWormWeb.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController(IUnitOfWork context) : Controller
{
    private readonly IUnitOfWork _context = context;
    [BindProperty]
    public ShoppingCartVM ShoppingCartVM { get; set; }
    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        ShoppingCartVM = new ShoppingCartVM()
        {
            ShoppingCartList = _context._shoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
            OrderHeader = new OrderHeader()
        };

        foreach(var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        return View(ShoppingCartVM);
    }

    public IActionResult Plus(int cartId)
    {
        var cart = _context._shoppingCartRepo.Get(u => u.Id == cartId, includeProperties: "Product");
        cart.Count += 1;
        _context._shoppingCartRepo.Update(cart);
        _context.Save();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Minus(int cartId)
    {
        var cart = _context._shoppingCartRepo.Get(u => u.Id == cartId, includeProperties: "Product");
        if (cart.Count <= 1)
        {
            _context._shoppingCartRepo.Remove(cart);
        }
        else
        {
            cart.Count -= 1;
            _context._shoppingCartRepo.Update(cart);
        }
        _context.Save();
        return RedirectToAction(nameof(Index));
    }
    public IActionResult Remove(int cartId)
    {
        var cart = _context._shoppingCartRepo.Get(u => u.Id == cartId);
        _context._shoppingCartRepo.Remove(cart);
        _context.Save();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Summary()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        ShoppingCartVM = new ShoppingCartVM()
        {
            ShoppingCartList = _context._shoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
            OrderHeader = new OrderHeader()
        };

        ShoppingCartVM.OrderHeader.ApplicationUser = _context._applicationUserRepo.Get(u => u.Id == userId);
        ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
        ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
        ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
        ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
        ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
        ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;


        foreach (var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        return View(ShoppingCartVM);
    }

    [HttpPost]
    [ActionName("Summary")]
    public IActionResult SummaryPOST()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        ShoppingCartVM.ShoppingCartList = _context._shoppingCartRepo.GetAll(u => u.ApplicationUserId == userId,
            includeProperties: "Product");

        ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
        ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
        ApplicationUser applicationUser = _context._applicationUserRepo.Get(u => u.Id == userId);


        foreach (var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;

        }
        else
        {
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
        }

        _context._orderHeaderRepo.Add(ShoppingCartVM.OrderHeader);
        _context.Save();

        foreach (var cart in ShoppingCartVM.ShoppingCartList)
        {
            OrderDetail orderDetail = new OrderDetail
            {
                ProductId = cart.ProductId,
                OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                Price = cart.Price,
                Count = cart.Count
            };
            _context._orderDetailRepo.Add(orderDetail);
            _context.Save();
        }

        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            // stripe logic

        }
        return RedirectToAction(nameof(OrderConfirmation), new {id = ShoppingCartVM.OrderHeader.Id});
    }

    public IActionResult OrderConfirmation(int id)
    {
        return View(id);
    }
    private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
    {
        if (shoppingCart.Count <= 50)
        {
            return shoppingCart.Product.Price;
        }
        else
        {
            if (shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            else
            {
                return shoppingCart.Product.Price100;
            }
        }
    }


}
