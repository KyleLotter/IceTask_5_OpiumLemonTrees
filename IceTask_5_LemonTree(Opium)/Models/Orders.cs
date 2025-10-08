using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IceTask_5_LemonTree_Opium_.Models
{
    public class Orders
    {
        [Key]
        public int order_id { get; set; }

        public int customer_id { get; set; }

        [ForeignKey("customer_id")]
        public Customers Customer { get; set; }

        public DateTimeOffset order_date { get; set; } = DateTimeOffset.Now;

        [Required]
        public decimal total_amount { get; set; }

        public string status { get; set; } = "pending";

        public List<OrderItems> OrderItems { get; set; }
    }
}