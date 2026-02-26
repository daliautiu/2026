using System;
using System.Collections.Generic;
using System.Linq;

namespace SieMarket
{
    public class OrderItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;

        public OrderItem(string productName, int quantity, decimal unitPrice)
        {
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
    }

    public class Order
    {
        private const decimal DiscountThreshold = 500m;
        private const decimal DiscountRate = 0.10m;

        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCountry { get; set; }
        public List<OrderItem> Items { get; set; }

        public decimal TotalBeforeDiscount => Items.Sum(item => item.Subtotal);
        public bool IsEligibleForDiscount => TotalBeforeDiscount > DiscountThreshold;
        public decimal DiscountAmount => IsEligibleForDiscount ? TotalBeforeDiscount * DiscountRate : 0m;
        public decimal TotalAfterDiscount => TotalBeforeDiscount - DiscountAmount;

        public Order(int orderId, string customerName, string customerCountry)
        {
            OrderId = orderId;
            OrderDate = DateTime.Now;
            CustomerName = customerName;
            CustomerCountry = customerCountry;
            Items = new List<OrderItem>();
        }

        public void AddItem(OrderItem item) => Items.Add(item);

        // 2.2 - Calculate final price with discount
        public decimal CalculateFinalPrice()
        {
            decimal total = Items.Sum(item => item.Quantity * item.UnitPrice);
            if (total > 500m)
                total -= total * 0.10m;
            return Math.Round(total, 2);
        }

        // 2.3 - Find top spender across all orders
        public static string FindTopSpender(List<Order> orders)
        {
            return orders
                .GroupBy(o => o.CustomerName)
                .Select(group => new
                {
                    CustomerName = group.Key,
                    TotalSpent = group.Sum(o => o.CalculateFinalPrice())
                })
                .OrderByDescending(c => c.TotalSpent)
                .First()
                .CustomerName;
        }

        // 2.4 - Get product popularity (name -> total qty sold)
        public static Dictionary<string, int> GetProductPopularity(List<Order> orders)
        {
            return orders
                .SelectMany(o => o.Items)
                .GroupBy(item => item.ProductName)
                .OrderByDescending(g => g.Sum(item => item.Quantity))
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(item => item.Quantity)
                );
        }

        public void PrintSummary()
        {
            Console.WriteLine($"Order #{OrderId} | {OrderDate:dd/MM/yyyy}");
            Console.WriteLine($"Customer: {CustomerName} ({CustomerCountry})");
            Console.WriteLine(new string('-', 45));
            foreach (var item in Items)
                Console.WriteLine($"  {item.ProductName,-20} x{item.Quantity}  {item.UnitPrice,8:C}  = {item.Subtotal,9:C}");
            Console.WriteLine(new string('-', 45));
            Console.WriteLine($"  {"Subtotal:",-30} {TotalBeforeDiscount,9:C}");
            if (IsEligibleForDiscount)
                Console.WriteLine($"  {"Discount (10%):",-30} -{DiscountAmount,8:C}");
            Console.WriteLine($"  {"TOTAL:",-30} {TotalAfterDiscount,9:C}");
        }
    }

    class Program
    {
        static void Main()
        {
            
            var order1 = new Order(1001, "Utiu Dalia", "Romania");
            order1.AddItem(new OrderItem("Keyboard", 2, 69.99m));
            order1.AddItem(new OrderItem("Monitor", 1, 549.00m));
            order1.AddItem(new OrderItem("Mouse", 3, 39.99m));

            var order2 = new Order(1002, "pers2", "Portugal");
            order2.AddItem(new OrderItem("Laptop", 1, 1200.00m));
            order2.AddItem(new OrderItem("Keyboard", 3, 69.99m));

            var order3 = new Order(1003, "pers3", "Romania");
            order3.AddItem(new OrderItem("Mouse", 2, 39.99m));
            order3.AddItem(new OrderItem("Monitor", 1, 549.00m));

            var orders = new List<Order> { order1, order2, order3 };

            // 2.1 Print summaries 
            foreach (var order in orders)
            {
                order.PrintSummary();
                Console.WriteLine();
            }

            //  2.2 Final price 
            Console.WriteLine("=== Final Prices (after discount) ===");
            foreach (var order in orders)
                Console.WriteLine($"  Order #{order.OrderId}: {order.CalculateFinalPrice():C}");

            // 2.3 Top spender 
            Console.WriteLine("\n=== Top Spender ===");
            Console.WriteLine($"  {Order.FindTopSpender(orders)}");

            //  2.4 Product popularity
            Console.WriteLine("\n=== Product Popularity ===");
            var popularity = Order.GetProductPopularity(orders);
            foreach (var p in popularity)
                Console.WriteLine($"  {p.Key,-20} x{p.Value} sold");
        }
    }
}