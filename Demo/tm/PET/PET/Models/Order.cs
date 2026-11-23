using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PET.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public string ShippingAddress { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        // Trạng thái đơn: Pending (Chờ xử lý), Shipping (Đang giao), Delivered (Đã giao), Cancelled (Đã hủy)
        public string Status { get; set; } = "Pending";

        // Khóa ngoại liên kết người mua
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }

    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Lưu lại giá tại thời điểm mua
    }
}
