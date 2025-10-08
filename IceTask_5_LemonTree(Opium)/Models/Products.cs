using System.ComponentModel.DataAnnotations;

namespace IceTask_5_LemonTree_Opium_.Models
{
    public class Products
    {
        [Key]
        public int product_id { get; set; }

        [Required, MaxLength(255)]
        public string name { get; set; }

        [Required]
        public string description { get; set; }

        [Required]
        public decimal price { get; set; }

        [Required]
        public int stock_quantity { get; set; }

        public string image_url { get; set; }

        public DateTimeOffset? created_at { get; set; } = DateTimeOffset.Now;
    }
}
