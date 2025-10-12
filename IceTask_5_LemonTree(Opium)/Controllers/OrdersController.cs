using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IceTask_5_LemonTree_Opium_.Models;

namespace IceTask_5_LemonTree_Opium_.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            return View(await _context.Orders.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.order_id == id);

            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            var products = await _context.Products
                .Where(p => p.stock_quantity > 0)
                .ToListAsync();

            var viewModel = new CartViewModel
            {
                CartItems = products.Select(p => new CartItemViewModel
                {
                    ProductId = p.product_id,
                    ProductName = p.name,
                    Price = p.price,
                    AvailableStock = p.stock_quantity,
                    Quantity = 0
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Orders/Create
        // STOCK CONTROL IMPLEMENTATION WITH TRANSACTIONS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CartViewModel cartViewModel)
        {
            // Filter out items with quantity 0
            var itemsToOrder = cartViewModel.CartItems.Where(item => item.Quantity > 0).ToList();

            if (!itemsToOrder.Any())
            {
                ModelState.AddModelError("", "Please select at least one product with a quantity greater than 0.");

                // Reload product data for the view
                var products = await _context.Products.Where(p => p.stock_quantity > 0).ToListAsync();
                cartViewModel.CartItems = products.Select(p =>
                {
                    var existingItem = cartViewModel.CartItems.FirstOrDefault(ci => ci.ProductId == p.product_id);
                    int quantity = 0;
                    if (existingItem != null)
                    {
                        quantity = existingItem.Quantity;
                    }

                    return new CartItemViewModel
                    {
                        ProductId = p.product_id,
                        ProductName = p.name,
                        Price = p.price,
                        AvailableStock = p.stock_quantity,
                        Quantity = quantity
                    };
                }).ToList();

                return View(cartViewModel);
            }

            // START TRANSACTION - Critical for stock control!
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // STEP 1: CHECK STOCK for all items
                    foreach (var item in itemsToOrder)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);

                        if (product == null)
                        {
                            ModelState.AddModelError("", $"Product with ID {item.ProductId} not found.");
                            await transaction.RollbackAsync();
                            return View(cartViewModel);
                        }

                        // Check if sufficient stock is available
                        if (product.stock_quantity < item.Quantity)
                        {
                            ModelState.AddModelError("",
                                $"Insufficient stock for {product.name}. Available: {product.stock_quantity}, Requested: {item.Quantity}");
                            await transaction.RollbackAsync();

                            // Reload product data for the view
                            var products = await _context.Products.Where(p => p.stock_quantity > 0).ToListAsync();
                            cartViewModel.CartItems = products.Select(p => new CartItemViewModel
                            {
                                ProductId = p.product_id,
                                ProductName = p.name,
                                Price = p.price,
                                AvailableStock = p.stock_quantity,
                                Quantity = cartViewModel.CartItems.FirstOrDefault(ci => ci.ProductId == p.product_id)?.Quantity ?? 0
                            }).ToList();

                            return View(cartViewModel);
                        }
                    }

                    // STEP 2: DECREASE STOCK for all items
                    foreach (var item in itemsToOrder)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        product.stock_quantity -= item.Quantity;
                        _context.Products.Update(product);
                    }

                    // STEP 3: CREATE ORDER
                    var order = new Orders
                    {
                        customer_id = cartViewModel.CustomerId,
                        order_date = DateTimeOffset.Now,
                        total_amount = itemsToOrder.Sum(item => item.LineTotal),
                        status = "pending"
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync(); // Save to get order_id

                    // STEP 4: CREATE ORDER ITEMS
                    foreach (var item in itemsToOrder)
                    {
                        var orderItem = new OrderItems
                        {
                            order_id = order.order_id,
                            product_id = item.ProductId,
                            quantity = item.Quantity,
                            price_at_purchase = item.Price
                        };

                        _context.OrderItems.Add(orderItem);
                    }

                    await _context.SaveChangesAsync();

                    // COMMIT TRANSACTION - All operations successful!
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = $"Order #{order.order_id} created successfully!";
                    return RedirectToAction(nameof(Details), new { id = order.order_id });
                }
                catch (Exception ex)
                {
                    // ROLLBACK on any error
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", $"An error occurred while processing your order: {ex.Message}");

                    // Reload product data for the view
                    var products = await _context.Products.Where(p => p.stock_quantity > 0).ToListAsync();
                    cartViewModel.CartItems = products.Select(p => new CartItemViewModel
                    {
                        ProductId = p.product_id,
                        ProductName = p.name,
                        Price = p.price,
                        AvailableStock = p.stock_quantity,
                        Quantity = cartViewModel.CartItems.FirstOrDefault(ci => ci.ProductId == p.product_id)?.Quantity ?? 0
                    }).ToList();

                    return View(cartViewModel);
                }
            }
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders.FindAsync(id);
            if (orders == null)
            {
                return NotFound();
            }
            return View(orders);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("order_id,customer_id,order_date,total_amount,status")] Orders orders)
        {
            if (id != orders.order_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orders);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdersExists(orders.order_id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(orders);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .FirstOrDefaultAsync(m => m.order_id == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orders = await _context.Orders.FindAsync(id);
            if (orders != null)
            {
                _context.Orders.Remove(orders);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdersExists(int id)
        {
            return _context.Orders.Any(e => e.order_id == id);
        }
    }
}