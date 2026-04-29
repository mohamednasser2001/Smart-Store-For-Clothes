using DataAccess.Data;
using DataAccess.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Entities;
using Models.VM;

namespace Smart_Store_For_Clothes.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public CartController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            var cart = _unitOfWork.Carts.GetAll()
                .FirstOrDefault(u => u.UserId == userId);

            if (cart != null)
            {
                cart.CartItems = _unitOfWork.CartItems.GetAll()
                    .Where(ci => ci.CartId == cart.Id)
                    .ToList();

                foreach (var item in cart.CartItems)
                {
                    item.Product = _unitOfWork.Products.GetById(item.ProductId);
                    // السطر ده عشان نجيب بيانات المقاس
                    item.Size = _unitOfWork.Sizes.GetById(item.SizeId);
                }
            }
            else
            {
                cart = new Cart { UserId = userId, CartItems = new List<CartItem>() };
            }

            CartVM cartVM = new CartVM() { Cart = cart };
            return View(cartVM);
        }

        [HttpPost]
        [Authorize]
        public IActionResult AddToCart(int productId, int sizeId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User);

            var cart = _unitOfWork.Carts.GetAll()
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                _unitOfWork.Carts.Add(cart);
                _unitOfWork.Save();
            }

            var cartItem = _unitOfWork.CartItems.GetAll()
                .FirstOrDefault(ci => ci.CartId == cart.Id
                                   && ci.ProductId == productId
                                   && ci.SizeId == sizeId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                _unitOfWork.CartItems.Update(cartItem);
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    SizeId = sizeId,
                    Quantity = quantity
                };

                _unitOfWork.CartItems.Add(cartItem);
            }

            _unitOfWork.Save();

            return RedirectToAction("Index");
        }

        public IActionResult Plus(int cartItemId)
        {
            var cartItem = _unitOfWork.CartItems.GetById(cartItemId);

            if (cartItem == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var productSize = _unitOfWork.ProductSizes.GetAll()
                .FirstOrDefault(ps => ps.ProductId == cartItem.ProductId && ps.SizeId == cartItem.SizeId);

            if (productSize == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (cartItem.Quantity < productSize.QuantityInStock)
            {
                cartItem.Quantity += 1;
                _unitOfWork.CartItems.Update(cartItem);
                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartItemId)
        {
            var cartItem = _unitOfWork.CartItems.GetById(cartItemId);

            if (cartItem.Quantity <= 1)
            {
                _unitOfWork.CartItems.Delete(cartItem);
            }
            else
            {
                cartItem.Quantity -= 1;
                _unitOfWork.CartItems.Update(cartItem);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartItemId)
        {
            var cartItem = _unitOfWork.CartItems.GetById(cartItemId);
            _unitOfWork.CartItems.Delete(cartItem);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var user = _userManager.GetUserAsync(User).Result;

            var cart = _unitOfWork.Carts.GetAll()
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                return RedirectToAction("Index");
            }

            var cartItems = _unitOfWork.CartItems.GetAll()
                .Where(ci => ci.CartId == cart.Id)
                .ToList();

            decimal subTotal = 0;

            foreach (var item in cartItems)
            {
                var product = _unitOfWork.Products.GetById(item.ProductId);
                if (product != null)
                {
                    subTotal += product.Price * item.Quantity;
                }
            }

            CheckoutVM checkoutVM = new CheckoutVM
            {
                CartId = cart.Id,
                Email = user?.Email ?? "",
                ShippingCost = 0,
                SubTotal = subTotal,
                Total = subTotal
            };

            return View(checkoutVM);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CheckoutVM model)
        {
            var userId = _userManager.GetUserId(User);

            var cart = _unitOfWork.Carts.GetAll()
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                return RedirectToAction("Index");
            }

            var cartItems = _unitOfWork.CartItems.GetAll()
                .Where(ci => ci.CartId == cart.Id)
                .ToList();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index");
            }

            decimal subTotal = 0;

            foreach (var item in cartItems)
            {
                var product = _unitOfWork.Products.GetById(item.ProductId);
                if (product != null)
                {
                    subTotal += product.Price * item.Quantity;
                }
            }

            decimal shippingCost = 0;
            decimal total = subTotal + shippingCost;

            Order order = new Order
            {
                UserId = userId,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                City = model.City,
                Address = model.Address,
                SubTotal = subTotal,
                ShippingCost = shippingCost,
                Total = total,
                OrderDate = DateTime.Now,
                Status = "Pending"
            };

            _unitOfWork.Orders.Add(order);
            _unitOfWork.Save();

            foreach (var item in cartItems)
            {
                var product = _unitOfWork.Products.GetById(item.ProductId);

                if (product != null)
                {
                    var productSize = _unitOfWork.ProductSizes.GetAll()
                        .FirstOrDefault(ps => ps.ProductId == item.ProductId && ps.SizeId == item.SizeId);

                    if (productSize == null || productSize.QuantityInStock < item.Quantity)
                    {
                        return RedirectToAction("Index");
                    }

                    productSize.QuantityInStock -= item.Quantity;
                    _unitOfWork.ProductSizes.Update(productSize);

                    OrderItem orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        SizeId = item.SizeId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        TotalPrice = product.Price * item.Quantity
                    };

                    _unitOfWork.OrderItems.Add(orderItem);
                }
            }

            _unitOfWork.Save();
            foreach (var item in cartItems)
            {
                _unitOfWork.CartItems.Delete(item);
            }

            _unitOfWork.Save();

            return RedirectToAction("OrderSuccess");
        }

        [Authorize]
        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}
