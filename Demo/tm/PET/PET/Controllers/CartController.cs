using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PET.Data;
using PET.Models;

namespace PET.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string CART_KEY = "FETCH_PET_CART";

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CART_KEY);
            return string.IsNullOrEmpty(json)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CART_KEY, JsonSerializer.Serialize(cart));
        }

        // GET: /Cart (Xem giỏ hàng)
        public IActionResult Index()
        {
            var cart = GetCart();
            ViewBag.GrandTotal = cart.Sum(c => c.Total);
            return View(cart);
        }

        // POST: /Cart/AddToCart (Thêm vào giỏ)
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsActive || product.StockQuantity <= 0)
            {
                TempData["WarningMessage"] = "Sản phẩm hiện ngừng kinh doanh hoặc đã hết hàng.";
                return RedirectToAction("Index", "Products");
            }

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existingItem != null)
            {
                if (existingItem.Quantity + quantity > product.StockQuantity)
                {
                    TempData["WarningMessage"] = $"Kho chỉ còn tối đa {product.StockQuantity} sản phẩm.";
                    return RedirectToAction(nameof(Index));
                }
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = Math.Min(quantity, product.StockQuantity)
                });
            }

            SaveCart(cart);
            TempData["SuccessMessage"] = $"Đã thêm \"{product.Name}\" vào giỏ hàng!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/UpdateQuantity (Cập nhật số lượng)
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    var product = await _context.Products.FindAsync(productId);
                    int maxStock = product?.StockQuantity ?? quantity;
                    item.Quantity = Math.Min(quantity, maxStock);
                }
                SaveCart(cart);
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Remove (Xóa khỏi giỏ)
        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/Checkout (Trang thanh toán)
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            var user = await _userManager.GetUserAsync(User);
            ViewBag.Cart = cart;
            ViewBag.GrandTotal = cart.Sum(c => c.Total);

            var order = new Order
            {
                PhoneNumber = user?.PhoneNumber ?? ""
            };
            return View(order);
        }

        // POST: /Cart/Checkout (Chốt đơn & trừ tồn kho)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout([Bind("ShippingAddress,PhoneNumber")] Order order)
        {
            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            order.UserId = _userManager.GetUserId(User)!;
            order.OrderDate = DateTime.Now;
            order.Status = "Pending";
            order.TotalAmount = cart.Sum(c => c.Total);
            order.OrderDetails = new List<OrderDetail>();

            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                foreach (var item in cart)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null || product.StockQuantity < item.Quantity)
                    {
                        ModelState.AddModelError("", $"Sản phẩm \"{item.ProductName}\" không đủ số lượng trong kho.");
                        ViewBag.Cart = cart;
                        ViewBag.GrandTotal = order.TotalAmount;
                        return View(order);
                    }

                    product.StockQuantity -= item.Quantity;
                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    });
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove(CART_KEY);
                TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng của bạn là #{order.Id}.";
                return RedirectToAction(nameof(MyOrders));
            }

            ViewBag.Cart = cart;
            ViewBag.GrandTotal = order.TotalAmount;
            return View(order);
        }

        // GET: /Cart/MyOrders (Lịch sử đơn hàng của khách & Quản lý đơn cho Admin)
        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.Product)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(o => o.UserId == userId);
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        // POST: Cập nhật trạng thái đơn (Pending -> Shipping -> Delivered / Cancelled)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && (order.UserId != userId || status != "Cancelled"))
            {
                return Forbid();
            }

            // Hoàn lại tồn kho nếu đơn bị hủy
            if (status == "Cancelled" && order.Status != "Cancelled" && order.OrderDetails != null)
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += detail.Quantity;
                    }
                }
            }

            order.Status = status;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{order.Id}!";
            return RedirectToAction(nameof(MyOrders));
        }
        // GET: /Cart/Invoice/5 (Trang hóa đơn chuẩn in ấn A4/A5)
        [Authorize]
        public async Task<IActionResult> Invoice(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && order.UserId != userId)
            {
                return Forbid();
            }

            return View(order);
        }
    }
}