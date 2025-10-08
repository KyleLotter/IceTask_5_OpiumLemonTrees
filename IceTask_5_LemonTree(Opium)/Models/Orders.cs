using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace IceTask_5_LemonTree_Opium_.Models
{
    public class Orders
    {
        [Key]
        public int order_id { get; set; }

        public int customer_id { get; set; }

        public DateTimeOffset order_date { get; set; } = DateTimeOffset.Now;

        [Required]
        public decimal total_amount { get; set; }

        public string status { get; set; } = "pending";

        // Navigation
        public Customers Customer { get; set; }
        public List<OrderItems> OrderItems { get; set; }
    }
}
