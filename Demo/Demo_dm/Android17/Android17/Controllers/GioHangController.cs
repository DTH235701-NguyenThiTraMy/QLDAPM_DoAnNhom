using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Android17.Controllers;

[Authorize]
public class GioHangController : Controller
{
    public IActionResult Index() => Content("Giỏ hàng (đang xây dựng)");
}