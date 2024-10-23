using System.ComponentModel.DataAnnotations;

namespace MasstransitSaga.Core.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Quantity { get; set; } // Số lượng tồn kho
    }
}
