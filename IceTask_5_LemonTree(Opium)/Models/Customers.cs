using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace IceTask_5_LemonTree_Opium_.Models
{
    public class Customers
    {
        [Key]
        public int customer_id { get; set; }

        [Required, MaxLength(100)]
        public string first_name { get; set; }

        [Required, MaxLength(100)]
        public string last_name { get; set; }

        [Required, MaxLength(255)]
        public string email { get; set; }

        [Required, MaxLength(255)]
        public string address { get; set; }

        [Required, MaxLength(100)]
        public string city { get; set; }

        [Required, MaxLength(20)]
        public string postal_code { get; set; }

        public DateTimeOffset? created_at { get; set; } = DateTimeOffset.Now;

        // Navigation
        public List<Orders> Orders { get; set; }
    }
}
