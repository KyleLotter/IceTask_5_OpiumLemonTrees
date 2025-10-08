using System.ComponentModel.DataAnnotations;

namespace IceTask_5_LemonTree_Opium_.Models
{
    public class OrderItems
    {
        [Key]
        public int order_item_id { get; set; }

        public int order_id { get; set; }

        public int product_id { get; set; }

        [Required]
        public int quantity { get; set; }

        [Required]
        public decimal price_at_purchase { get; set; }

        // Navigation
        public Orders Order { get; set; }
        public Products Product { get; set; }
    }
}
