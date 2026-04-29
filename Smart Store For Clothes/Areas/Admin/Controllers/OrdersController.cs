using DataAccess.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Entities;
using Models.VM;

namespace Smart_Store_For_Clothes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrdersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public IActionResult Index()
        {
            var orders = _unitOfWork.Orders.GetAll()
                .OrderBy(o => o.Status == "Accepted")
                .ThenByDescending(o => o.OrderDate)
                .ToList();

            ViewBag.PendingCount = orders.Count(o => o.Status == "Pending");
            ViewBag.AcceptedCount = orders.Count(o => o.Status == "Accepted");

            return View(orders);
        }
        public IActionResult Details(int id)
        {
            var order = _unitOfWork.Orders.GetById(id);

            if (order == null)
            {
                return NotFound();
            }

            var orderItems = _unitOfWork.OrderItems.GetAll()
                .Where(oi => oi.OrderId == id)
                .ToList();

            var orderItemsVM = orderItems.Select(item => new OrderItemDetailsVM
            {
                ProductId = item.ProductId,
                ProductName = _unitOfWork.Products.GetById(item.ProductId)?.Name ?? "Unknown Product",
                SizeId = item.SizeId,
                SizeName = _unitOfWork.Sizes.GetById(item.SizeId)?.Name ?? "Unknown Size",
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            }).ToList();

            OrderDetailsVM vm = new OrderDetailsVM
            {
                Order = order,
                OrderItems = orderItemsVM
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Accept(int id)
        {
            var order = _unitOfWork.Orders.GetById(id);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Accepted";
            _unitOfWork.Orders.Update(order);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}
