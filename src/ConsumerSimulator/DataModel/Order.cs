using Newtonsoft.Json;
using System.Text;

namespace ConsumerSimulator
{
    public class Order
    {
        [JsonProperty("customer_id")]
        public Guid CustomerId { get; set; }
        [JsonProperty("order_items")]
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Customer ID: {0}\n", CustomerId);

            sb.AppendLine();
            sb.AppendLine("Order Items:");

            // Add header row
            sb.AppendFormat("{0,-10} {1,-10}\n", "Product ID", "Amount");
            sb.AppendLine(new String('-', 25));

            // Add data rows
            foreach (var item in OrderItems) {
                sb.AppendFormat("{0,-10} {1,-10}\n", item.ProductId, item.Amount);
            }

            return sb.ToString();
        }

    }
}
