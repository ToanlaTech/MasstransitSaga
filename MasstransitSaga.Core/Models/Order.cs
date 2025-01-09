using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasstransitSaga.Core.Models;

[Table("order")]
public class Order
{
    [Key]
    public int Id { get; set; }
    public string CountryCode { get; set; } // Liên kết với bảng Countries
    public decimal TotalAmount { get; set; } // Tổng số tiền đơn hàng
    public DateTime OrderDate { get; set; } // Ngày giao dịch
    public virtual Country Country { get; set; }
}

