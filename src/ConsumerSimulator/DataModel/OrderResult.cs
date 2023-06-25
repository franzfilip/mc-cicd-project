using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerSimulator
{
    public class OrderResult
    {
        [JsonProperty("customer_id")]
        public Guid CustomerId { get; set; }
        [JsonProperty("order_id")]
        public int OrderId { get; set; }
        [JsonProperty("total_amount")]
        public float TotalAmount { get; set; }
        [JsonProperty("order_result_items")]
        public List<OrderResultItem> OrderResultItems { get; set; } = new List<OrderResultItem>();

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Order ID: {0}\n", OrderId);
            sb.AppendFormat("Customer ID: {0}\n", CustomerId);
            sb.AppendFormat("Total Amount: {0} EUR\n", TotalAmount);

            sb.AppendLine();
            sb.AppendLine("Order Items:");

            // Add header row
            sb.AppendFormat("{0,-10} {1,-38} {2,-15}\n", "Product ID", "Product UUID", "Warehouse ID");
            sb.AppendLine(new String('-', 65));

            // Add data rows
            foreach (var item in OrderResultItems) {
                sb.AppendFormat("{0,-10} {1,-38} {2,-15}\n", item.ProductId, item.ProductUUID, item.WarehouseId);
            }

            return sb.ToString();
        }

    }
}
