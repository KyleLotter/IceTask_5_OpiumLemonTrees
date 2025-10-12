
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IceTask_5_LemonTree_Opium_.Models
{
    public class CartViewModel
    {
        [Required]
        public int CustomerId { get; set; }

        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

        public decimal TotalAmount { get; set; }
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int AvailableStock { get; set; }

        [Required]
        public int Quantity { get; set; }

        public decimal LineTotal => Price * Quantity;
    }
}