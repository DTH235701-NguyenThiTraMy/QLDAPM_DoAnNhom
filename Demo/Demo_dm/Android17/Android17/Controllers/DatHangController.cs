using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Android17.Controllers;

[Authorize]
public class DatHangController : Controller
{
    public IActionResult Index() => Content("Lịch sử đơn hàng (đang xây dựng)");
}