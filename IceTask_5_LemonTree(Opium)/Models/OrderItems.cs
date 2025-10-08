using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IceTask_5_LemonTree_Opium_.Models
{
    public class OrderItems
    {
        [Key]
        public int order_item_id { get; set; }

        public int order_id { get; set; }

        public int product_id { get; set; }

        [ForeignKey("order_id")]
        public Orders Order { get; set; }

        [ForeignKey("product_id")]
        public Products Product { get; set; }

        [Required]
        public int quantity { get; set; }

        [Required]
        public decimal price_at_purchase { get; set; }
    }
}
