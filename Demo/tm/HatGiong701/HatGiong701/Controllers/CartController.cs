using HatGiong701.Data;
using HatGiong701.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HatGiong701.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private const string CART_KEY = "shopcart";

        public CartController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Đọc danh sách giỏ hàng từ Session
        private List<CartItem> GetCartItems()
        {
            var session = HttpContext.Session;
            string? jsonCart = session.GetString(CART_KEY);
            if (!string.IsNullOrEmpty(jsonCart))
            {
                return JsonSerializer.Deserialize<List<CartItem>>(jsonCart) ?? new List<CartItem>();
            }
            return new List<CartItem>();
        }

        // Lưu danh sách giỏ hàng vào Session
        private void SaveCartSession(List<CartItem> list)
        {
            var session = HttpContext.Session;
            string jsonCart = JsonSerializer.Serialize(list);
            session.SetString(CART_KEY, jsonCart);
        }

        // Xóa giỏ hàng sau khi đặt hàng thành công
        private void ClearCart()
        {
            HttpContext.Session.Remove(CART_KEY);
        }

        // 1. Xem giỏ hàng
        public IActionResult Index()
        {
            return View(GetCartItems());
        }

        // 2. Thêm hạt giống vào giỏ
        public async Task<IActionResult> AddToCart(int id)
        {
            var seed = await _context.Seeds.FindAsync(id);
            if (seed == null) return NotFound("Hạt giống không tồn tại");
            if (seed.StockQuantity <= 0)
            {
                TempData["Error"] = "Hạt giống này hiện đã hết hàng!";
                return RedirectToAction("Index", "Home");
            }

            var cart = GetCartItems();
            var cartItem = cart.Find(p => p.Seed.SeedId == id);
            if (cartItem != null)
            {
                if (cartItem.Quantity + 1 <= seed.StockQuantity)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    TempData["Error"] = User.IsInRole("Admin")
                        ? $"Chỉ còn {seed.StockQuantity} gói trong kho!"
                        : "Số lượng hạt giống trong kho không đủ cho yêu cầu của bạn!";
                }
            }
            else
            {
                cart.Add(new CartItem { Seed = seed, Quantity = 1 });
            }

            SaveCartSession(cart);
            return RedirectToAction(nameof(Index));
        }

        // 3. Cập nhật số lượng
        [HttpPost]
        public async Task<IActionResult> UpdateCart(int id, int quantity)
        {
            var cart = GetCartItems();
            var cartItem = cart.Find(p => p.Seed.SeedId == id);
            if (cartItem != null)
            {
                var seed = await _context.Seeds.FindAsync(id);
                int maxStock = seed?.StockQuantity ?? cartItem.Seed.StockQuantity;

                if (quantity <= 0)
                {
                    cart.Remove(cartItem);
                }
                else if (quantity <= maxStock)
                {
                    cartItem.Quantity = quantity;
                }
                else
                {
                    TempData["Error"] = User.IsInRole("Admin")
                        ? $"Số lượng vượt quá tồn kho ({maxStock} gói)!"
                        : "Số lượng đặt vượt quá số lượng hiện có của cửa hàng!";
                }
            }
            SaveCartSession(cart);
            return RedirectToAction(nameof(Index));
        }

        // 4. Xóa 1 sản phẩm khỏi giỏ
        public IActionResult RemoveCart(int id)
        {
            var cart = GetCartItems();
            var cartItem = cart.Find(p => p.Seed.SeedId == id);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }
            SaveCartSession(cart);
            return RedirectToAction(nameof(Index));
        }

        // 5. Hiển thị trang Đặt hàng (Bắt buộc đăng nhập mới được đặt)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCartItems();
            if (cart.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            var order = new Order
            {
                ReceiverName = user?.FullName ?? "",
                PhoneNumber = user?.PhoneNumber ?? "",
                ShippingAddress = user?.Address ?? "",
                TotalAmount = cart.Sum(i => i.TotalPrice)
            };

            ViewBag.CartItems = cart;
            return View(order);
        }

        // 6. Xử lý lưu Đơn hàng & Chi tiết đơn hàng vào Database
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = GetCartItems();
            if (cart.Count == 0) return RedirectToAction(nameof(Index));

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Gán thông tin hệ thống tự động
            order.UserId = user.Id;
            order.OrderDate = DateTime.Now;
            order.TotalAmount = cart.Sum(i => i.TotalPrice);
            order.Status = "Chờ xác nhận";

            // Bỏ qua kiểm tra ModelState cho UserId vì ta vừa gán bằng code
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Lưu để lấy OrderId tự tăng

                // Lưu từng chi tiết đơn hàng
                foreach (var item in cart)
                {
                    var detail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        SeedId = item.Seed.SeedId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Seed.Price
                    };
                    _context.OrderDetails.Add(detail);
                }

                await _context.SaveChangesAsync();
                ClearCart(); // Xóa sạch giỏ hàng

                return RedirectToAction(nameof(OrderSuccess), new { id = order.OrderId });
            }

            ViewBag.CartItems = cart;
            return View(order);
        }

        // 7. Trang thông báo đặt hàng thành công
        [Authorize]
        public async Task<IActionResult> OrderSuccess(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}